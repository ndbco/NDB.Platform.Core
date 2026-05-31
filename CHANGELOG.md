# Changelog — NDB.Platform.Core

All notable changes to this package are documented here.

Format follows [Keep a Changelog](https://keepachangelog.com/en/1.1.0/).
This project follows [Semantic Versioning](https://semver.org/).

---

## [1.0.0] — 2026-05-31

First stable release. All source file comments translated to English.

### Added — New infrastructure contracts

- **`Abstraction/Security`**: `IPermissionResolver` — granular RBAC contract (`GetEffectivePermissionsAsync`, `HasPermissionAsync`, `InvalidateAsync`). DENY grants always take precedence over ALLOW.
- **`Abstraction/Storage`**: `IFileStore` — provider-agnostic file storage contract (`SaveAsync`, `OpenAsync`, `DeleteAsync`, `ExistsAsync`, `ProviderName`). Implement with Local, S3, Azure Blob, or any custom provider.
- **`Abstraction/Exports`**: `IExportRenderer` + `ExportDataset` — format-agnostic dataset export (CSV, XLSX, etc.). Implementation provided by the consuming project.
- **`Abstraction/Notification`**: `INotificationDispatcher` — multi-channel dispatcher (IN_APP, EMAIL, PUSH) with DND window support.
- **`Abstraction/Notification`**: `INotificationHub` — real-time per-user notification push (SignalR / WebSocket).
- **`Abstraction/Messaging`**: `IMessageHub` — real-time message push to thread participants (SignalR / WebSocket).

### Added — New utility extensions

- **`Kit/Caching`**: `DistributedCacheExtensions` — `GetOrSetAsync<T>()` for `IDistributedCache` (cache-aside pattern), complementing `MemoryCacheExtensions` for Redis scenarios.
- **`Kit/Caching`**: TTL presets: `MasterDataOptions()` (15 min), `ReferenceDataOptions()` (30 min), `ShortLivedOptions()` (5 min).
- **`Kit/Format`**: `IRegionService` — locale-aware formatting interface (`FormatCurrency`, `FormatDate`, `FormatPhone` in E.164). Implementation provided by the consuming project.

### Added — Dependencies

- Added `Microsoft.Extensions.Caching.Abstractions` as an explicit dependency for `IDistributedCache` type resolution.

### Tests

- `DistributedCacheExtensionsTests` — 9 test cases (cache miss/hit, null factory, cancellation, TTL presets).
- `IRegionServiceContractTests` — 4 test cases (contract mock verification).
- `NotificationInterfaceContractTests` — 5 test cases (`INotificationDispatcher` + `INotificationHub`).
- `IMessageHubContractTests` — 3 test cases (`IMessageHub` contract verification).

---

## [0.5.0] — 2026-05-24

### Added

- **Abstraction**: `AuditActor` sealed record — immutable actor identity for audit trail (`Actor`, `ActorId`, `Role`, `CorrelationId`). `AuditActor.System` constant for background jobs and CLI.
- **Abstraction**: `IActorAccessor` interface — resolves actor identity. Web: `HttpContextActorAccessor` (from `NDB.Platform.API`). Background: `SystemActorAccessor` (from Core, registered as default).
- **Kit.Identifiers**: `SystemActorAccessor` — default `IActorAccessor` that returns `AuditActor.System` with the current correlation ID. Used as fallback when no `HttpContext` is available.
- **Kit.Mapping**: `IServiceMapper` marker interface — implementations are auto-registered as Scoped via `AddNdbMapping(assemblies)`.
- **Cqrs**: `AddNdbCqrs()` now registers `SystemActorAccessor` as `TryAddSingleton<IActorAccessor>` (overridable by `HttpContextActorAccessor` from the API layer).
- **Analyzer NDB001**: Expanded to flag direct constructor calls for `new PagedResult<T>()`, `new ListResult<T>()`, and `new CollectionResult<T>()`. Previously only covered `Result` and `Result<T>`.

### Changed

- **Kit.Mapping**: `AddNdbMapping(assemblies)` now also auto-scans `IServiceMapper` implementations and registers them as Scoped. Backward-compatible; existing Mapster scan behavior is unchanged.
- **Analyzer NDB001**: Message format updated with `{0}` placeholder for the type name, producing more informative error messages.

### Fixed

- NDB001 analyzer was not covering `PagedResult<T>`, `ListResult<T>`, `CollectionResult<T>`. All Result subtypes are now protected.

---

## [0.4.0] — 2026-05-24

### Added

- **Abstraction**: `Result.Error(string, Exception?)` overload.
- **Abstraction**: `Result<T>.Error(string, Exception?)` overload.
- **Abstraction**: `Result.Validation(IDictionary<string,string[]>, string?)` non-generic factory.
- **Abstraction**: `Result.Exception` property for carrying exception context.
- **Abstraction**: `Result.ValidationErrors` property on non-generic Result.
- **Http**: `HttpResult<T>.Headers` property for capturing response headers.
- **Http**: `BaseApiService.IsTokenExpiredHeader()` — checks `Token-Expired: true` before triggering a refresh.

### Changed (Breaking)

- **Http**: `BaseApiService.SendWithRefreshAsync` — token refresh is now triggered only when the response is `401` **and** the `Token-Expired: true` header is present. Previously all `401` responses triggered a refresh.
  - **Migration**: Ensure the auth server sends `Token-Expired: true` on expired-token 401 responses (e.g. in `NdbJwtBearerEvents.OnChallenge`).
- **Http**: `BaseApiService.TryRefreshAsync` — fixed double-check race: if another concurrent request already completed the refresh, the method returns `true` immediately without calling the refresher again.
- **Http**: `HttpResult<T>.Ok()` and `HttpResult<T>.Fail()` factory methods now accept an optional `headers` parameter.

### Fixed

- Refresh storm on `401` for wrong password / bad credentials — refresh is now gated on `Token-Expired: true`.
- Race condition in `TryRefreshAsync` — missing early-exit `return true` after double-check pass.
- `Result.Error(string, Exception?)` overload was missing.

---

## [0.1.0] — 2026-05-20

Initial release.

### Added

- **Infrastructure**: `Directory.Build.props`, `Directory.Packages.props`, `.slnx`, `.editorconfig`, Central Package Management (CPM), multi-target net8.0 + net10.0.
- **Abstraction**: `Result<T>` and `Result` factory-only pattern (private constructor, enforced by Roslyn analyzer).
- **Abstraction**: `ListResult<T>`, `PagedResult<T>`, `CollectionResult<T>` sealed factory classes.
- **Abstraction**: `PageInfo` paging metadata. `ResultStatus` enum.
- **Abstraction**: Marker interfaces (`IEntity`, `IRequestDto`, `IResponseDto`).
- **Abstraction**: Request DTOs (`PagingRequest`, `SortRequest`, `FilterRequest`, `ListRequest`).
- **Abstraction**: Common items (`LookupItem`, `ReferenceItem`, `KeyValueItem`, `FileObject`).
- **Cqrs**: Mediator.SourceGenerator setup, `ICommand<T>`, `IQuery<T>`, handler interfaces. Pipeline: `LoggingBehavior`, `ValidationBehavior` (FluentValidation). `AddNdbCqrs(assemblies)`.
- **Kit.Mapping**: Mapster integration, `IMapFrom<T>`, `IMapTo<T>`, `IMapObject`, `AutoMappingProfile` auto-scan. `AddNdbMapping(assemblies)`.
- **Kit.Guards**: `Guard` static class — `NotNull`, `NotEmpty`, `Positive`, `NonNegative`, `InRange`, `MaxLength`.
- **Kit.Crypto**: `PasswordHasher` (BCrypt factor 12), `PiiHasher` (Blake3), `Base64Helper` (standard + URL-safe).
- **Kit.Text**: `StringNormalize` (ToSlug, ToCamelCase, ToPascalCase, ToSnakeCase, StripHtml, Truncate). `RegexPatterns` (Email, Phone, NIK, NPWP) via source-generated `[GeneratedRegex]`.
- **Kit.Format**: `NumberFormat`, `DateTimeFormat`, `PhoneFormat`, `FileSizeFormat`.
- **Kit.Parse**: `Parse` (type-converter based), `EnumHelper`.
- **Kit.Identifiers**: `IdGenerator` (ULID, Nanoid, Snowflake), `CorrelationId`.
- **Http**: `HttpResult<T>`, `BaseApiService`, request helpers, `IAccessTokenProvider`, `CorrelationIdHandler`, `PiiScrubLoggingHandler`. `AddNdbHttpClient<TClient,TImpl>` with resilience (retry 3×, timeouts).
- **Analyzers**: `NDB001 ResultFactoryAnalyzer` — compile-time error on direct `new Result()` or `new Result<T>()`.
- **Tests**: 191 unit tests, 100% passing on net8.0 and net10.0.
