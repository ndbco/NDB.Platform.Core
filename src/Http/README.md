# NDB.Platform.Http

> Namespace: `NDB.Platform.Http`
> [← Back to main README](../../README.md)

Typed HTTP client infrastructure for service-to-service communication. Provides `BaseApiService`, resilience (retry + circuit breaker), automatic token injection, token refresh on 401, correlation ID propagation, and PII scrubbing from logs.

---

## Setup

```csharp
using NDB.Platform.Http;

// Register a typed HTTP client:
builder.Services.AddNdbHttpClient<IOrderServiceApi, OrderServiceApi>(opt =>
{
    opt.BaseUrl          = config["Services:Order:BaseUrl"]!;
    opt.EnableRetry      = true;
    opt.MaxRetryAttempts = 3;
    opt.TimeoutSeconds   = 30;
});
```

---

## Defining a Typed Client

```csharp
// 1. Interface — defines the API contract:
public interface IOrderServiceApi
{
    Task<HttpResult<OrderDto>>            GetAsync(Guid id, CancellationToken ct = default);
    Task<HttpResult<PagedResult<OrderDto>>> ListAsync(int page, int size, CancellationToken ct = default);
    Task<HttpResult<Guid>>                CreateAsync(CreateOrderRequest req, CancellationToken ct = default);
    Task<HttpResult<byte[]>>              ExportAsync(string format, CancellationToken ct = default);
    Task<HttpResult>                      CancelAsync(Guid id, CancellationToken ct = default);
}

// 2. Implementation — inherits BaseApiService:
public class OrderServiceApi(IHttpClientHelper http)
    : BaseApiService(http), IOrderServiceApi
{
    public Task<HttpResult<OrderDto>> GetAsync(Guid id, CancellationToken ct)
        => GetAsync<OrderDto>($"/orders/{id}", ct);

    public Task<HttpResult<PagedResult<OrderDto>>> ListAsync(int page, int size, CancellationToken ct)
        => GetAsync<PagedResult<OrderDto>>($"/orders?page={page}&size={size}", ct);

    public Task<HttpResult<Guid>> CreateAsync(CreateOrderRequest req, CancellationToken ct)
        => PostAsync<CreateOrderRequest, Guid>("/orders", req, ct);

    public Task<HttpResult<byte[]>> ExportAsync(string format, CancellationToken ct)
        => GetBytesAsync($"/orders/export?format={format}", ct);

    public Task<HttpResult> CancelAsync(Guid id, CancellationToken ct)
        => DeleteAsync($"/orders/{id}", ct);
}
```

---

## HttpResult

Every HTTP call returns `HttpResult` or `HttpResult<T>` — no try/catch needed.

```csharp
var result = await orderApi.GetAsync(orderId, ct);

// Check success:
if (!result.IsSuccess)
{
    return result.StatusCode switch
    {
        HttpStatusCode.NotFound  => Result.NotFound("Order not found in remote service"),
        HttpStatusCode.Forbidden => Result.Forbidden("No access to this order"),
        _                        => Result.Error($"Remote error: {result.Error}")
    };
}

var order = result.Value; // non-null when IsSuccess

// Access response headers:
var requestId = result.Headers?["X-Request-ID"];
var version   = result.Headers?["X-Api-Version"];
```

---

## Token Management

### IAccessTokenProvider

Supplies the current access token that is injected into outgoing requests.

```csharp
// Default implementation (from NDB.Platform.API):
// Reads the Bearer token from the current HttpContext.

// For background jobs or service-to-service (machine-to-machine):
public class ClientCredentialsProvider(IHttpClientFactory factory, IConfiguration config)
    : IAccessTokenProvider
{
    private string? _cachedToken;
    private DateTime _expiresAt;

    public async Task<string?> GetTokenAsync(CancellationToken ct)
    {
        if (_cachedToken is not null && DateTime.UtcNow < _expiresAt)
            return _cachedToken;

        using var client = factory.CreateClient("auth");
        var response = await client.PostAsync("/connect/token",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"]    = "client_credentials",
                ["client_id"]     = config["Auth:ClientId"]!,
                ["client_secret"] = config["Auth:ClientSecret"]!,
            }), ct);

        var token = await response.Content.ReadFromJsonAsync<TokenDto>(cancellationToken: ct);
        _cachedToken = token!.AccessToken;
        _expiresAt   = DateTime.UtcNow.AddSeconds(token.ExpiresIn - 30);
        return _cachedToken;
    }
}
```

### Automatic Token Refresh

`BaseApiService` automatically refreshes the token when a `401 Unauthorized` response is received **with** the `Token-Expired: true` header. Requests that fail with `401` for other reasons (wrong credentials, permission denied) are **not** retried.

```csharp
// Implement ITokenRefresher to define how the token is refreshed:
public class JwtTokenRefresher(IHttpClientFactory factory) : ITokenRefresher
{
    public async Task<TokenResponse?> RefreshAsync(string refreshToken, CancellationToken ct)
    {
        using var client = factory.CreateClient("auth");
        var response = await client.PostAsJsonAsync(
            "/auth/refresh", new { refreshToken }, ct);

        return response.IsSuccessStatusCode
            ? await response.Content.ReadFromJsonAsync<TokenResponse>(cancellationToken: ct)
            : null;
    }
}

// Implement ITokenStorage to persist the current token pair:
public class InMemoryTokenStorage : ITokenStorage
{
    private TokenResponse? _tokens;

    public Task<TokenResponse?> GetAsync(CancellationToken ct)
        => Task.FromResult(_tokens);

    public Task SetAsync(TokenResponse tokens, CancellationToken ct)
    {
        _tokens = tokens;
        return Task.CompletedTask;
    }
}
```

> **Thread safety:** `BaseApiService` uses a `SemaphoreSlim(1,1)` lock and a double-check to prevent multiple simultaneous refresh calls.

---

## Pipeline Handlers

Two `DelegatingHandler` instances are registered automatically by `AddNdbHttpClient`:

### CorrelationIdHandler

Propagates `X-Correlation-ID` from the incoming request to all outgoing HTTP calls:

```
Incoming: POST /api/orders  →  X-Correlation-ID: req-01HWKR6GZJ...
Outgoing: GET /stock/{id}   →  X-Correlation-ID: req-01HWKR6GZJ...  (same)
Outgoing: POST /payments    →  X-Correlation-ID: req-01HWKR6GZJ...  (same)
```

This makes it possible to trace a single request across all downstream services in your logs.

### PiiScrubLoggingHandler

Masks sensitive fields in request/response logs:

```
Authorization: Bearer ey***
X-Api-Key: ***
password: ***
refresh_token: ***
```

Prevents secrets and personal data from appearing in log sinks.

---

## Resilience (Retry + Circuit Breaker)

Configured automatically via `Microsoft.Extensions.Http.Resilience`:

| Policy | Default |
|---|---|
| Retry | 3 attempts with exponential backoff (1 s, 2 s, 4 s) |
| Circuit Breaker | Breaks after 5 failures in 30 s window; stays open 60 s |
| Per-attempt timeout | 10 seconds |
| Total timeout | 30 seconds |

Override per client:

```csharp
builder.Services.AddNdbHttpClient<IPaymentApi, PaymentApi>(opt =>
{
    opt.BaseUrl          = config["Services:Payment:BaseUrl"]!;
    opt.MaxRetryAttempts = 1;      // payment idempotency — limit retries
    opt.TimeoutSeconds   = 90;     // allow longer for payment processing
});
```

---

## Session Expiry Event

When a token refresh fails (e.g. refresh token has also expired), `BaseApiService` raises `SessionExpired`. Subscribe to handle graceful logout or redirect:

```csharp
// In a Blazor or SPA setup:
var api = serviceProvider.GetRequiredService<IOrderServiceApi>() as BaseApiService;
if (api is not null)
{
    api.SessionExpired += (_, _) =>
        navigationManager.NavigateTo("/login?reason=session-expired", forceLoad: true);
}
```

---

## Source Files

```
src/Http/
├── BaseApiService.cs              ← Base class for typed HTTP clients
├── HttpResult.cs                  ← Result wrapper for HTTP responses
├── IHttpClientHelper.cs           ← Internal HTTP abstraction (testable)
├── IServiceApi.cs                 ← Marker interface for service API types
├── IAccessTokenProvider.cs        ← Supplies the current access token
├── MultipartRequest.cs            ← Builder for multipart/form-data uploads
├── RequestContentType.cs          ← Content-type enum
├── RequestOptions.cs              ← Per-request options (headers, timeout)
├── DependencyInjection.cs         ← AddNdbHttpClient() extension
├── Handlers/
│   ├── CorrelationIdHandler.cs    ← Propagates X-Correlation-ID
│   └── PiiScrubLoggingHandler.cs  ← Masks sensitive fields in logs
└── Resilience/
    ├── ITokenRefresher.cs         ← Defines how to refresh a token
    ├── ITokenStorage.cs           ← Stores the current token pair
    ├── TokenResponse.cs           ← Token pair model (access + refresh)
    └── SessionExpiredEventArgs.cs ← Raised when refresh fails
```

---

> Part of [NDB.Platform.Core](https://github.com/ndbco/NDB.Platform.Core) — free and open source under [GPL v3](../../LICENSE).
> Built by [PT. Navigate Digital Boundaries](https://ndb.co.id/)
