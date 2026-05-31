# NDB.Platform.Core

<div align="center">

**The foundation library for every NDB Platform project**

Built by [PT. Navigate Digital Boundaries](https://ndb.co.id/) — *Navigate Digital Boundaries*

[![NuGet](https://img.shields.io/nuget/v/NDB.Platform.Core?label=NuGet&color=blue)](https://www.nuget.org/packages/NDB.Platform.Core/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/NDB.Platform.Core?color=green)](https://www.nuget.org/packages/NDB.Platform.Core/)
[![License: GPL v3](https://img.shields.io/badge/License-GPL%20v3-yellow.svg)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET-8.0%20%7C%2010.0-purple)](https://dotnet.microsoft.com/)

</div>

---

## What is NDB.Platform.Core?

**NDB.Platform.Core** is the shared foundation used across every NDB Platform project. It provides the building blocks that every enterprise application needs — result patterns, CQRS infrastructure, utility helpers, HTTP client infrastructure, and a set of clean contracts for storage, permissions, exports, notifications, and messaging.

**Goal:** Cut 60–80% of boilerplate setup time on every new NDB project by shipping verified, production-ready building blocks.

---

## Installation

```bash
dotnet add package NDB.Platform.Core
```

```xml
<!-- Or in your .csproj -->
<PackageReference Include="NDB.Platform.Core" Version="1.0.0" />
```

---

## Table of Contents

| Namespace | Documentation | Description |
|---|---|---|
| `NDB.Platform.Abstraction` | [📄 Abstraction](src/Abstraction/README.md) | Result pattern, shared contracts (storage, permissions, exports, notification, messaging) |
| `NDB.Platform.Cqrs` | [📄 CQRS](src/Cqrs/README.md) | Compile-time CQRS via Mediator.SourceGenerator, logging & validation pipeline |
| `NDB.Platform.Kit` | [📄 Kit](src/Kit/README.md) | Utility helpers: caching, crypto, format, guards, identifiers, mapping, parse, text |
| `NDB.Platform.Http` | [📄 Http](src/Http/README.md) | Typed HTTP client infrastructure: resilience, token refresh, PII scrubbing |
| — | [🔬 Analyzers](analyzers/README.md) | Roslyn analyzer NDB001 — compile-time enforcement of Result factory pattern |
| — | [🧪 Tests](tests/README.md) | Unit test suite — 33 test files, net8.0 + net10.0, coverage ≥80% |

---

## Minimum Setup

```csharp
// Program.cs
using NDB.Platform.Cqrs;
using NDB.Platform.Kit.Mapping;

// Register CQRS pipeline (logging + validation behaviors)
builder.Services.AddNdbCqrs(typeof(Program).Assembly);

// Register Mediator source-generated dispatcher
// Must be called AFTER AddNdbCqrs
builder.Services.AddMediator(opt => opt.ServiceLifetime = ServiceLifetime.Scoped);

// Register Mapster auto-mapping profiles
builder.Services.AddNdbMapping(typeof(Program).Assembly);
```

---

## Core Concepts

### 1 · Result Pattern (No Exceptions, No Nulls)

Every operation returns a `Result` or `Result<T>`. No throwing exceptions for expected failures, no returning null.

```csharp
// Factory-only — constructor is private, enforced by Roslyn analyzer NDB001
return Result.Success(dto);
return Result.NotFound("User not found");
return Result.BadRequest("Email already registered");
return Result.Forbidden("Access denied");
return Result.Conflict("Duplicate entry");
return Result.ValidationError(errors);   // IDictionary<string, string[]>
return Result.Error("Unexpected error", exception);

// Reading the result
var result = await handler.Handle(query, ct);
if (!result.IsSuccess) return result.ToActionResult();
var value = result.Value; // guaranteed non-null when IsSuccess
```

**Types available:**

| Type | Use case |
|---|---|
| `Result` | Void operations (Delete, Publish, Send) |
| `Result<T>` | Operations returning a value |
| `ListResult<T>` | Flat collection |
| `PagedResult<T>` | Paginated collection with metadata |
| `CollectionResult<T>` | Generic collection |

> The Roslyn analyzer **NDB001** triggers a compile-time error if `new Result<T>()` is used directly. Always use the factory methods.

---

### 2 · CQRS (Compile-time, Zero Reflection)

Commands and queries are dispatched at compile time by [Mediator.SourceGenerator](https://github.com/martinothamar/Mediator). No runtime reflection, no performance overhead.

```csharp
// Query
public sealed record GetOrderQuery(Guid Id) : IQuery<Result<OrderDto>>;

public sealed class GetOrderHandler(AppDbContext db)
    : IQueryHandler<GetOrderQuery, Result<OrderDto>>
{
    public async ValueTask<Result<OrderDto>> Handle(GetOrderQuery q, CancellationToken ct)
    {
        var order = await db.Orders.FindAsync([q.Id], ct);
        return order is null
            ? Result.NotFound($"Order {q.Id} not found")
            : Result.Success(order.Adapt<OrderDto>());
    }
}

// Command
public sealed record CancelOrderCommand(Guid Id, string Reason) : ICommand<Result>;

public sealed class CancelOrderHandler(AppDbContext db)
    : ICommandHandler<CancelOrderCommand, Result>
{
    public async ValueTask<Result> Handle(CancelOrderCommand cmd, CancellationToken ct)
    {
        var updated = await db.Orders
            .Where(o => o.Id == cmd.Id && o.Status != "CANCELLED")
            .ExecuteUpdateAsync(s => s
                .SetProperty(o => o.Status, "CANCELLED")
                .SetProperty(o => o.CancelReason, cmd.Reason), ct);

        return updated == 0 ? Result.NotFound("Order not found") : Result.Success();
    }
}
```

**Automatic pipeline behaviors** (registered by `AddNdbCqrs`):

| Behavior | What it does |
|---|---|
| `LoggingBehavior` | Logs request type, execution duration, and status |
| `ValidationBehavior` | Runs all `IValidator<TRequest>` before the handler; returns `ValidationError` on failure |

---

### 3 · Infrastructure Contracts

All interfaces in `Abstraction.*` are **pure contracts** — no implementations shipped in this package. Register your implementation in the consuming project's DI container.

**IFileStore** — Provider-agnostic file storage:
```csharp
// Inject without knowing whether it's Local, S3, or Azure:
public class UploadHandler(IFileStore store) : ICommandHandler<UploadCommand, Result<string>>
{
    public async ValueTask<Result<string>> Handle(UploadCommand cmd, CancellationToken ct)
    {
        var key = await store.SaveAsync(cmd.Stream, cmd.FileName, cmd.MimeType, ct);
        return Result.Success(key);
    }
}

// Register in Program.cs:
builder.Services.AddSingleton<IFileStore, LocalFileStore>();
// or: builder.Services.AddSingleton<IFileStore, S3FileStore>();
```

**IPermissionResolver** — Granular RBAC:
```csharp
// Effective permissions = union(role_permissions) + grants(ALLOW) - grants(DENY)
// DENY always takes precedence
builder.Services.AddScoped<IPermissionResolver, PermissionResolver>();
```

**INotificationDispatcher** — Multi-channel fan-out:
```csharp
await dispatcher.DispatchAsync(
    recipientId: userId,
    category: "WORKFLOW",
    title: "Task assigned to you",
    body: $"Task '{task.Title}' requires your attention",
    actionUrl: $"/tasks/{task.Id}",
    priority: "HIGH");
```

**IExportRenderer** — Format-agnostic export:
```csharp
// Render to CSV or XLSX depending on implementation:
var bytes = await renderer.RenderAsync(dataset, ct);
return File(bytes, renderer.MimeType, $"export.{renderer.Extension}");
```

---

### 4 · Caching (Memory + Distributed)

```csharp
using NDB.Platform.Kit.Caching;

// IMemoryCache — cache-aside with TTL presets:
var roles = await cache.GetOrSetAsync(
    CacheKeyBuilder.For("roles", "all"),
    () => db.Roles.OrderBy(r => r.Name).ToListAsync(ct),
    CacheEntryDefaults.DefaultMasterDataOptions());  // 60s sliding / 1h absolute

// IDistributedCache (Redis) — same pattern:
var settings = await distributedCache.GetOrSetAsync(
    CacheKeyBuilder.For("settings", orgId),
    () => db.Settings.Where(s => s.OrgId == orgId).ToListAsync(ct),
    DistributedCacheExtensions.MasterDataOptions(),  // 15 min absolute
    ct);

// Cache key builder — consistent format, no magic strings:
CacheKeyBuilder.For("user", "perms", userId)  // → "user:perms:{guid}"
```

---

### 5 · Crypto

```csharp
using NDB.Platform.Kit.Crypto;

// BCrypt password hashing:
var hash = PasswordHasher.Hash("plaintextPassword");
bool ok   = PasswordHasher.Verify("plaintextPassword", hash);

// Blake3 one-way hashing for PII indexing (NIK, email):
var nik = PiiHasher.Hash("3271234567890001");

// Base64 encoding:
string encoded = Base64Helper.Encode(bytes);
string urlSafe = Base64Helper.EncodeUrlSafe(bytes);
byte[] decoded = Base64Helper.Decode(encoded);
```

---

### 6 · Identifiers

```csharp
using NDB.Platform.Kit.Identifiers;

var ulid      = IdGenerator.NewUlid();       // sortable, URL-safe
var nanoid    = IdGenerator.NewNanoid();     // short, URL-safe
var nanoid8   = IdGenerator.NewNanoid(8);   // custom length
var snowflake = IdGenerator.NextSnowflake(); // monotonic 64-bit

var correlationId = CorrelationId.Generate(); // "req-{ulid}"
```

---

### 7 · Object Mapping (Mapster)

```csharp
using NDB.Platform.Kit.Mapping;

// Auto-map from entity to DTO:
public class OrderDto : IMapFrom<Order>
{
    public Guid Id { get; set; }
    public string Status { get; set; } = default!;
    // Fields with matching names are mapped automatically
}

// Use in handler:
var dto  = order.Adapt<OrderDto>();
var list = orders.Adapt<List<OrderDto>>();

// Efficient EF projection (no over-fetching):
var page = await db.Orders
    .ProjectToType<OrderDto>()
    .ToPagedAsync(req.Page, req.Size, ct);
```

---

### 8 · HTTP Client (Service-to-Service)

```csharp
using NDB.Platform.Http;

// Register typed client:
builder.Services.AddNdbHttpClient<IInventoryApi, InventoryApi>(opt =>
{
    opt.BaseUrl = config["Services:Inventory:BaseUrl"]!;
    opt.EnableRetry = true;
});

// Implement:
public class InventoryApi(IHttpClientHelper http) : BaseApiService(http), IInventoryApi
{
    public Task<HttpResult<StockDto>> GetStockAsync(Guid productId, CancellationToken ct)
        => GetAsync<StockDto>($"/stock/{productId}", ct);

    public Task<HttpResult<Guid>> ReserveAsync(ReserveStockRequest req, CancellationToken ct)
        => PostAsync<ReserveStockRequest, Guid>("/stock/reserve", req, ct);
}

// Check result without try/catch:
var result = await inventoryApi.GetStockAsync(productId, ct);
if (!result.IsSuccess)
    return Result.Error($"Inventory service error: {result.Error}");

var stock = result.Value;
```

---

## Requirements

| Requirement | Detail |
|---|---|
| .NET | 8.0 or 10.0 |
| `AddMediator()` | Must be called in the entry-point project (`Mediator.SourceGenerator`) |
| DI for contracts | All `Abstraction.*` interfaces require a registered implementation |
| FluentValidation | Validators are discovered automatically via `AddNdbCqrs(Assembly[])` |

---

## Ecosystem

| Package | Version | Role |
|---|---|---|
| **NDB.Platform.Core** | 1.0.0 | Foundation ← *you are here* |
| [NDB.Platform.Data](https://www.nuget.org/packages/NDB.Platform.Data/) | 1.0.0 | EF Core audit, CodeGen, IQueryable extensions |
| NDB.Platform.API | coming soon | JWT auth, Swagger, Hangfire, middleware, permission handler |

---

## Open Source

NDB.Platform.Core is **free and open source**, licensed under [GPL v3](LICENSE). You are free to use it in your own projects, fork it, clone it, and contribute back.

```bash
git clone https://github.com/ndbco/NDB.Platform.Core.git
```

### Contributing

All contributions are welcome — bug reports, feature requests, documentation improvements, and pull requests.

1. Fork the repository on [GitHub](https://github.com/ndbco/NDB.Platform.Core)
2. Create a branch: `git checkout -b feat/my-improvement`
3. Make your changes and add tests
4. Ensure all tests pass: `dotnet test`
5. Open a pull request

**Guidelines:**
- All public API must have XML doc comments in English
- Unit test coverage must stay at or above 80%
- No breaking changes without a major version bump
- Follow the existing code style (`.editorconfig` is included)

### Reporting Issues

Open an issue at [github.com/ndbco/NDB.Platform.Core/issues](https://github.com/ndbco/NDB.Platform.Core/issues).

---

<div align="center">

**PT. Navigate Digital Boundaries** · [ndb.co.id](https://ndb.co.id/)

*Navigate Digital Boundaries*

[GPL v3](LICENSE) · Copyright © PT. Navigate Digital Boundaries · Open Source

</div>
