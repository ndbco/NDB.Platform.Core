# NDB.Platform.Abstraction

> Namespace: `NDB.Platform.Abstraction.*`
> [← Back to main README](../../README.md)

Result patterns, shared marker interfaces, request DTOs, and infrastructure contracts used across all NDB projects.

---

## Result Pattern

Every operation returns `Result` or `Result<T>` — no exceptions for expected failures, no null returns. The constructor is **private**; use factory methods only. The Roslyn analyzer **NDB001** produces a compile-time error if `new Result<T>()` is used directly.

```csharp
// ✅ Correct — use factory methods
return Result.Success(user);
return Result.NotFound("User not found");
return Result.BadRequest("Email already registered");
return Result.Forbidden("Access denied");
return Result.Conflict("Record already exists");
return Result.Unauthorized("Token expired");
return Result.Error("Internal error", exception);
return Result<PagedResult<UserDto>>.ValidationError(validationErrors);

// ❌ Wrong — NDB001 fails the build
return new Result<UserDto>();
```

### Result Types

| Type | Factory | Use case |
|---|---|---|
| `Result` | `Result.Success()` | Void operations (Delete, Send, Publish) |
| `Result<T>` | `Result.Success(value)` | Operations returning a value |
| `ListResult<T>` | `Result.SuccessList(items)` | Flat collection responses |
| `PagedResult<T>` | `Result.SuccessPaged(items, total, page, size)` | Paginated responses |
| `CollectionResult<T>` | `Result.SuccessCollection(items)` | Generic collection |

### Reading a Result

```csharp
var result = await mediator.Send(query, ct);

if (!result.IsSuccess)
{
    return result.Status switch
    {
        ResultStatus.NotFound      => NotFound(result.Error),
        ResultStatus.BadRequest    => BadRequest(result.Error),
        ResultStatus.Forbidden     => Forbid(),
        ResultStatus.Conflict      => Conflict(result.Error),
        ResultStatus.Unauthorized  => Unauthorized(),
        ResultStatus.ValidationError => UnprocessableEntity(result.ValidationErrors),
        _                          => StatusCode(500, result.Error)
    };
}

var data = result.Value; // non-null when IsSuccess
```

### Status Enum

```csharp
result.Status; // ResultStatus:
// Success | NotFound | BadRequest | Forbidden |
// Conflict | Unauthorized | Error | ValidationError
```

---

## Marker Interfaces

Marker interfaces used for type safety and automatic registration.

```csharp
// Entity — used by EF conventions and audit trail
public class Order : IEntity
{
    public Guid Id { get; set; }
    // ...
}

// Request/Response — used by Mapster auto-scan
public class CreateOrderRequest : IRequestDto { }
public class OrderDto : IResponseDto { }
```

### Built-in Request DTOs

```csharp
// Combine as base class for CQRS queries:
public class GetOrdersQuery : IQuery<Result<PagedResult<OrderDto>>>
{
    // PagingRequest fields:
    public int Page { get; init; } = 1;
    public int Size { get; init; } = 20;

    // SortRequest fields:
    public string? SortBy  { get; init; }
    public string? SortDir { get; init; } = "asc";

    // FilterRequest — add custom filter fields:
    public string? Status      { get; init; }
    public Guid?   CustomerId  { get; init; }
    public string? Search      { get; init; }
}
```

### Common Reference Types

```csharp
// Dropdown / lookup items:
var item = new LookupItem { Id = id, Label = "Customer Name" };

// Key-value pairs:
var kv = new KeyValueItem { Key = "status", Value = "active" };

// Full reference item with code:
var ref = new ReferenceItem { Id = id, Code = "ORD-001", Name = "Order #1" };
```

---

## Infrastructure Contracts

All interfaces here are **pure contracts** — no implementation is provided by this package. Register your implementation in the consuming project's DI container.

---

### `Abstraction.Storage` — IFileStore

Provider-agnostic file storage. Handlers inject `IFileStore` without knowing whether storage is local disk, S3, or Azure Blob.

```csharp
using NDB.Platform.Abstraction.Storage;

public interface IFileStore
{
    Task<string> SaveAsync(Stream content, string fileName, string mimeType, CancellationToken ct = default);
    Task<Stream> OpenAsync(string storageKey, CancellationToken ct = default);
    Task DeleteAsync(string storageKey, CancellationToken ct = default);
    Task<bool> ExistsAsync(string storageKey, CancellationToken ct = default);
    string ProviderName { get; }
}
```

**Usage in a handler:**

```csharp
public class UploadDocumentHandler(IFileStore store, AppDbContext db)
    : ICommandHandler<UploadDocumentCommand, Result<string>>
{
    public async ValueTask<Result<string>> Handle(UploadDocumentCommand cmd, CancellationToken ct)
    {
        if (!cmd.File.ContentType.StartsWith("application/"))
            return Result.BadRequest("Only document files are allowed");

        await using var stream = cmd.File.OpenReadStream();
        var storageKey = await store.SaveAsync(stream, cmd.FileName, cmd.File.ContentType, ct);

        var doc = new Document { Id = Guid.NewGuid(), StorageKey = storageKey, ... };
        db.Documents.Add(doc);
        await db.SaveChangesAsync(ct);

        return Result.Success(storageKey);
    }
}
```

**Registering an implementation:**

```csharp
// Program.cs
builder.Services.AddSingleton<IFileStore, LocalFileStore>();
// or: S3FileStore, AzureBlobFileStore, etc.
```

---

### `Abstraction.Security` — IPermissionResolver

Granular RBAC contract. Computes effective permissions as:

```
effective = union(role_permissions) + grants(ALLOW) − grants(DENY)
```

**DENY always takes precedence** over ALLOW, regardless of role membership.

```csharp
using NDB.Platform.Abstraction.Security;

public interface IPermissionResolver
{
    Task<IReadOnlySet<string>> GetEffectivePermissionsAsync(Guid userId, CancellationToken ct = default);
    Task<bool> HasPermissionAsync(Guid userId, string permissionKey, CancellationToken ct = default);
    Task InvalidateAsync(Guid userId, CancellationToken ct = default);
}
```

**Sample implementation with caching:**

```csharp
public class PermissionResolver(AppDbContext db, IDistributedCache cache) : IPermissionResolver
{
    public async Task<bool> HasPermissionAsync(Guid userId, string key, CancellationToken ct)
    {
        var perms = await GetEffectivePermissionsAsync(userId, ct);
        return perms.Contains(key);
    }

    public async Task<IReadOnlySet<string>> GetEffectivePermissionsAsync(Guid userId, CancellationToken ct)
    {
        return await cache.GetOrSetAsync($"perm:user:{userId}", async () =>
        {
            // Collect denied keys first
            var denied = await db.PermissionGrants
                .Where(g => g.UserId == userId && g.Type == "DENY")
                .Select(g => g.Key).ToHashSetAsync(ct);

            // Collect allowed keys from roles + explicit grants
            var allowed = await db.UserEffectivePermissions
                .Where(p => p.UserId == userId && !denied.Contains(p.Key))
                .Select(p => p.Key).ToHashSetAsync(ct);

            return (IReadOnlySet<string>)allowed;
        }, DistributedCacheExtensions.ShortLivedOptions(), ct) ?? new HashSet<string>();
    }

    public Task InvalidateAsync(Guid userId, CancellationToken ct)
        => cache.RemoveAsync($"perm:user:{userId}", ct);
}
```

**Registering:**

```csharp
builder.Services.AddScoped<IPermissionResolver, PermissionResolver>();
```

> Superadmin bypass is handled in `NDB.Platform.API` via `PermissionAuthorizationHandler` — no special-casing needed in the resolver.

---

### `Abstraction.Exports` — IExportRenderer

Format-agnostic export contract. The same handler works for CSV, XLSX, or any future format.

```csharp
using NDB.Platform.Abstraction.Exports;

public sealed record ExportDataset
{
    public IReadOnlyList<string> Columns { get; init; } = [];
    public IReadOnlyList<IReadOnlyList<object?>> Rows { get; init; } = [];
    public string SheetName { get; init; } = "Export";
}

public interface IExportRenderer
{
    Task<byte[]> RenderAsync(ExportDataset dataset, CancellationToken ct = default);
    string MimeType { get; }
    string Extension { get; }
}
```

**Usage:**

```csharp
public class ExportOrdersHandler(AppDbContext db, IEnumerable<IExportRenderer> renderers)
    : ICommandHandler<ExportOrdersCommand, Result<(byte[] Bytes, string Mime, string Ext)>>
{
    public async ValueTask<Result<(byte[], string, string)>> Handle(
        ExportOrdersCommand cmd, CancellationToken ct)
    {
        var orders = await db.Orders
            .AsNoTracking()
            .Select(o => new { o.Id, o.Reference, o.Status, o.TotalAmount, o.CreatedAt })
            .ToListAsync(ct);

        var dataset = new ExportDataset
        {
            Columns = ["Reference", "Status", "Amount", "Date"],
            Rows = orders.Select(o => new object?[]
                { o.Reference, o.Status, o.TotalAmount, o.CreatedAt.ToString("yyyy-MM-dd") }).ToList(),
            SheetName = "Orders"
        };

        var renderer = renderers.First(r => r.Extension == cmd.Format); // "csv" or "xlsx"
        var bytes = await renderer.RenderAsync(dataset, ct);
        return Result.Success((bytes, renderer.MimeType, renderer.Extension));
    }
}
```

---

### `Abstraction.Notification` — INotificationDispatcher & INotificationHub

**INotificationDispatcher** fans out to all active channels (IN_APP, EMAIL, PUSH) respecting user preferences and Do-Not-Disturb windows.

```csharp
using NDB.Platform.Abstraction.Notification;

// Dispatch after a business event:
var notifId = await dispatcher.DispatchAsync(
    recipientId:  assignee.Id,
    category:     "WORKFLOW",
    title:        "New task assigned",
    body:         $"Task '{task.Title}' is waiting for you",
    actionUrl:    $"/tasks/{task.Id}",
    notifModule:  "workflow",
    priority:     "HIGH",
    ct:           ct);
// Returns null if all channels are blocked (DND, preferences)
```

**Priority levels:** `"NORMAL"` · `"HIGH"` · `"URGENT"`

**INotificationHub** pushes a real-time notification to a connected user via SignalR, WebSocket, or SSE:

```csharp
// Used internally by implementations of INotificationDispatcher:
await hub.PushNotificationAsync(recipientId, notifId, "New task", "...", ct);
```

---

### `Abstraction.Messaging` — IMessageHub

Pushes a new message to all members of a conversation thread in real time.

```csharp
using NDB.Platform.Abstraction.Messaging;

// Used in SendMessageHandler after persisting the message:
if (_hub is not null)
    await _hub.PushMessageAsync(thread.Id, message.Id, sender.Id, message.Body, ct);
```

**SignalR adapter example:**

```csharp
public class ChatHubAdapter(IHubContext<ChatHub> hub) : IMessageHub
{
    public Task PushMessageAsync(Guid threadId, Guid messageId,
        Guid senderId, string body, CancellationToken ct)
        => hub.Clients.Group($"thread:{threadId}")
            .SendAsync("NewMessage",
                new { messageId, threadId, senderId, body, sentAt = DateTime.UtcNow }, ct);
}

// Program.cs:
builder.Services.AddScoped<IMessageHub, ChatHubAdapter>();
```

---

## Source Files

```
src/Abstraction/
├── Result.cs                       ← Non-generic Result
├── ResultOfT.cs                    ← Result<T>
├── ResultStatus.cs                 ← Status enum
├── ListResult.cs                   ← Flat collection result
├── PagedResult.cs                  ← Paginated result
├── CollectionResult.cs             ← Generic collection result
├── PageInfo.cs                     ← Paging metadata
├── AuditActor.cs                   ← Immutable actor identity for audit
├── IActorAccessor.cs               ← Resolve current actor from context
├── Markers/
│   ├── IEntity.cs
│   ├── IRequestDto.cs
│   └── IResponseDto.cs
├── Requests/
│   ├── PagingRequest.cs
│   ├── SortRequest.cs
│   ├── FilterRequest.cs
│   └── ListRequest.cs
├── Common/
│   ├── KeyValueItem.cs
│   ├── LookupItem.cs
│   ├── ReferenceItem.cs
│   └── FileObject.cs
├── Security/
│   └── IPermissionResolver.cs
├── Storage/
│   └── IFileStore.cs
├── Exports/
│   └── IExportRenderer.cs          ← + ExportDataset record
├── Notification/
│   ├── INotificationDispatcher.cs
│   └── INotificationHub.cs
└── Messaging/
    └── IMessageHub.cs
```

---

> Part of [NDB.Platform.Core](https://github.com/ndbco/NDB.Platform.Core) — free and open source under [GPL v3](../../LICENSE).
> Built by [PT. Navigate Digital Boundaries](https://ndb.co.id/)
