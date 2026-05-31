# NDB.Platform.Core — Analyzers

> [← Back to main README](../README.md)

Roslyn compile-time analyzers shipped with `NDB.Platform.Core`. These analyzers enforce library design rules at build time — violations produce **compile errors**, not warnings.

---

## NDB001 — ResultFactoryAnalyzer

**Prevents direct instantiation** of Result types via `new`. Forces use of static factory methods.

### What it flags

```csharp
// ❌ NDB001 — compile error
var r1 = new Result();
var r2 = new Result<UserDto>();
var r3 = new PagedResult<UserDto>();
var r4 = new ListResult<UserDto>();
var r5 = new CollectionResult<UserDto>();
```

### What to use instead

```csharp
// ✅ Use factory methods
return Result.Success();
return Result.Success(dto);
return Result.NotFound("User not found");
return Result.BadRequest("Invalid input");
return Result.Forbidden("Access denied");
return Result.Conflict("Already exists");
return Result.Unauthorized("Token expired");
return Result.Error("Unexpected failure", exception);
return Result.ValidationError(validationErrors);

// Paged / List collections:
return Result.SuccessPaged(items, totalCount, page, pageSize);
return Result.SuccessList(items);
```

### Error message

```
NDB001: 'Result<T>' must be created via factory methods
        (e.g. Result.Success(), PagedResult.Success()).
        Direct constructor invocation is not allowed.
```

### Why this rule exists

All Result types have private constructors by design. The factory pattern guarantees that every result carries a valid `ResultStatus` and that no result is constructed in an inconsistent state. It also makes the intent explicit at the call site.

### Suppressing (not recommended)

In rare cases — e.g. test infrastructure or reflection utilities — you can suppress per-line:

```csharp
#pragma warning disable NDB001
var r = new Result<string>(); // intentional — test infrastructure only
#pragma warning restore NDB001
```

Or per-file via `.editorconfig`:

```ini
[tests/**/*.cs]
dotnet_diagnostic.NDB001.severity = none
```

> Suppressing NDB001 in production code is strongly discouraged.

---

## Covered Types

| Type | MetadataName |
|---|---|
| `Result` | `NDB.Platform.Abstraction.Result` |
| `Result<T>` | `NDB.Platform.Abstraction.Result\`1` |
| `PagedResult<T>` | `NDB.Platform.Abstraction.PagedResult\`1` |
| `ListResult<T>` | `NDB.Platform.Abstraction.ListResult\`1` |
| `CollectionResult<T>` | `NDB.Platform.Abstraction.CollectionResult\`1` |

---

## Source Files

```
analyzers/
├── ResultFactoryAnalyzer.cs          ← NDB001 implementation
├── NDB.Platform.Core.Analyzers.csproj
├── AnalyzerReleases.Shipped.md       ← Required by Roslyn packaging
└── AnalyzerReleases.Unshipped.md
```

---

> Part of [NDB.Platform.Core](https://github.com/ndbco/NDB.Platform.Core) — free and open source under [GPL v3](../LICENSE).
> Built by [PT. Navigate Digital Boundaries](https://ndb.co.id/)
