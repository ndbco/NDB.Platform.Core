# NDB.Platform.Core — Tests

> [← Back to main README](../README.md)

Unit test suite for `NDB.Platform.Core`. All tests target **net8.0** and **net10.0** simultaneously. Minimum coverage requirement: **80%**.

---

## Running Tests

```bash
# From the NDB.Platform.Core root:
dotnet test

# With coverage report:
dotnet test --collect:"XPlat Code Coverage"

# Specific test file:
dotnet test --filter "FullyQualifiedName~ResultTests"

# Both targets explicitly:
dotnet test -f net8.0
dotnet test -f net10.0
```

---

## Test Coverage by Namespace

| Folder | Tests | What is covered |
|---|---|---|
| `Abstraction/` | Result, ListResult, PagedResult, PageInfo | Factory methods, status checks, error/validation states |
| `Abstraction/Messaging/` | IMessageHubContractTests | Interface mock contract verification |
| `Abstraction/Notification/` | NotificationInterfaceContractTests | INotificationDispatcher + INotificationHub contracts |
| `Analyzers/` | ResultFactoryAnalyzerTests | NDB001 compile-time error on direct `new Result<T>()` |
| `Http/` | HttpResult, BaseApiService refresh logic | Token refresh, session expiry, result mapping |
| `Http/Resilience/` | SessionExpiredEventArgs, TokenResponse | Token model, event args |
| `Kit/Caching/` | CacheKeyBuilder, CacheEntryDefaults, MemoryCacheExtensions, DistributedCacheExtensions | Cache-aside pattern, TTL presets, null-factory behavior |
| `Kit/Crypto/` | PasswordHasher, PiiHasher, Base64Helper | Hash/verify, one-way hashing, encode/decode |
| `Kit/Format/` | DateTimeFormat, NumberFormat, PhoneFormat, FileSizeFormat, IRegionService | Output correctness, edge cases |
| `Kit/Guards/` | Guard | All guard methods, exception types and messages |
| `Kit/Identifiers/` | IdGenerator, CorrelationId, SystemActorAccessor | Uniqueness, format, actor identity |
| `Kit/Mapping/` | AutoMappingProfile, IServiceMapper | Auto-scan registration, mapping correctness |
| `Kit/Parse/` | EnumHelper | Parse, default fallback, IsDefined |
| `Kit/Text/` | StringNormalize, RegexPatterns | Slug, snake_case, camelCase, regex matches |

---

## Test Stack

| Library | Purpose |
|---|---|
| [xUnit](https://xunit.net/) | Test framework |
| [FluentAssertions](https://fluentassertions.com/) | Readable assertions |
| [NSubstitute](https://nsubstitute.github.io/) | Mocking interfaces |
| [coverlet](https://github.com/coverlet-coverage/coverlet) | Code coverage collection |

---

## Writing Tests

Follow the existing convention:

```csharp
// File: tests/Kit/Caching/MyExtensionsTests.cs
public sealed class MyExtensionsTests
{
    [Fact]
    public async Task MethodName_Scenario_ExpectedBehavior()
    {
        // Arrange
        var cache = Substitute.For<IDistributedCache>();
        cache.GetAsync("key", Arg.Any<CancellationToken>()).Returns((byte[]?)null);

        // Act
        var result = await cache.GetOrSetAsync("key", () => Task.FromResult<string?>("value"),
            DistributedCacheExtensions.MasterDataOptions());

        // Assert
        result.Should().Be("value");
    }
}
```

**Rules:**
- Method names: `MethodName_Scenario_ExpectedBehavior`
- One assertion per test where possible
- Use `NSubstitute` for all interfaces — no hand-rolled fakes
- Tests must pass on both net8.0 and net10.0

---

> Part of [NDB.Platform.Core](https://github.com/ndbco/NDB.Platform.Core) — free and open source under [GPL v3](../LICENSE).
> Built by [PT. Navigate Digital Boundaries](https://ndb.co.id/)
