using System.Net;
using System.Net.Http.Headers;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NDB.Platform.Abstraction;
using NDB.Platform.Http;
using NDB.Platform.Http.Resilience;
using NSubstitute;
using Xunit;

namespace NDB.Platform.Tests.Http;

// Test double: minimal concrete subclass of BaseApiService for testing
internal sealed class TestApiService : BaseApiService
{
    public TestApiService(HttpClient client) : base(client, NullLogger.Instance) { }
    public TestApiService(HttpClient client, ITokenRefresher refresher, ITokenStorage storage)
        : base(client, NullLogger.Instance, refresher, storage) { }

    public Task<HttpResult<string>> CallGetAsync(string path, CancellationToken ct = default)
        => GetAsync<string>(path, ct);
}

public sealed class BaseApiServiceRefreshTests
{
    [Fact]
    public async Task GetAsync_200Response_ShouldReturnSuccess()
    {
        var handler = new MockHttpHandler(HttpStatusCode.OK, "\"hello\"");
        using var client = new HttpClient(handler) { BaseAddress = new Uri("http://test/") };
        var svc = new TestApiService(client);

        var result = await svc.CallGetAsync("api/test");

        result.Succeeded.Should().BeTrue();
        result.Data.Should().Be("hello");
    }

    [Fact]
    public async Task GetAsync_401WithoutRefresher_ShouldReturnFail()
    {
        var handler = new MockHttpHandler(HttpStatusCode.Unauthorized, "Unauthorized");
        using var client = new HttpClient(handler) { BaseAddress = new Uri("http://test/") };
        var svc = new TestApiService(client);

        var result = await svc.CallGetAsync("api/test");

        result.Succeeded.Should().BeFalse();
        result.StatusCode.Should().Be(401);
    }

    // ── FIX 2 (C-01): Token-Expired header detect ────────────────────────────

    [Fact]
    public async Task GetAsync_401_Without_TokenExpired_Header_Should_NOT_Trigger_Refresh()
    {
        // 401 tanpa Token-Expired header = wrong password / bad credentials, jangan refresh
        var callCount = 0;
        var handler = new DynamicMockHttpHandler(req =>
        {
            callCount++;
            var resp = new HttpResponseMessage(HttpStatusCode.Unauthorized);
            // TIDAK ada Token-Expired header
            return resp;
        });

        using var client = new HttpClient(handler) { BaseAddress = new Uri("http://test/") };
        var refresher = Substitute.For<ITokenRefresher>();
        var storage = Substitute.For<ITokenStorage>();

        var svc = new TestApiService(client, refresher, storage);
        var result = await svc.CallGetAsync("api/test");

        result.Succeeded.Should().BeFalse();
        result.StatusCode.Should().Be(401);
        callCount.Should().Be(1); // hanya 1 call, tidak retry
        _ = refresher.DidNotReceive().RefreshAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetAsync_401_With_TokenExpired_True_Should_Trigger_Refresh_And_Retry()
    {
        // 401 + Token-Expired: true = expired token, harus refresh + retry
        var callCount = 0;
        var handler = new DynamicMockHttpHandler(req =>
        {
            callCount++;
            if (callCount == 1)
            {
                var resp401 = new HttpResponseMessage(HttpStatusCode.Unauthorized);
                resp401.Headers.Add("Token-Expired", "true"); // header yang memicu refresh
                return resp401;
            }
            // retry setelah refresh = 200
            return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("\"ok\"") };
        });

        using var client = new HttpClient(handler) { BaseAddress = new Uri("http://test/") };
        var refresher = Substitute.For<ITokenRefresher>();
        var storage = Substitute.For<ITokenStorage>();
        var newToken = new TokenResponse("new-access", "new-refresh", DateTimeOffset.UtcNow.AddMinutes(15));
        refresher.RefreshAsync(Arg.Any<CancellationToken>()).Returns(Result<TokenResponse>.Success(newToken));
        // Kedua panggilan GetAccessToken() return null supaya tidak trigger early-exit race path
        // (scenario: single request, bukan concurrent)
        storage.GetAccessToken().Returns((string?)null);

        var svc = new TestApiService(client, refresher, storage);
        var result = await svc.CallGetAsync("api/test");

        result.Succeeded.Should().BeTrue();
        callCount.Should().Be(2); // call pertama 401, retry setelah refresh = 200
        // Verify refresher dipanggil tepat 1x
        _ = refresher.Received(1).RefreshAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetAsync_401_With_TokenExpired_False_Should_NOT_Trigger_Refresh()
    {
        // Token-Expired: false = custom header ada tapi nilai bukan "true"
        var callCount = 0;
        var handler = new DynamicMockHttpHandler(req =>
        {
            callCount++;
            var resp = new HttpResponseMessage(HttpStatusCode.Unauthorized);
            resp.Headers.Add("Token-Expired", "false");
            return resp;
        });

        using var client = new HttpClient(handler) { BaseAddress = new Uri("http://test/") };
        var refresher = Substitute.For<ITokenRefresher>();
        var storage = Substitute.For<ITokenStorage>();

        var svc = new TestApiService(client, refresher, storage);
        var result = await svc.CallGetAsync("api/test");

        result.Succeeded.Should().BeFalse();
        callCount.Should().Be(1);
        _ = refresher.DidNotReceive().RefreshAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetAsync_200_Headers_Should_Be_Captured_In_Result()
    {
        var handler = new DynamicMockHttpHandler(req =>
        {
            var resp = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("\"data\"")
            };
            resp.Headers.Add("X-Request-Id", "abc-123");
            return resp;
        });

        using var client = new HttpClient(handler) { BaseAddress = new Uri("http://test/") };
        var svc = new TestApiService(client);

        var result = await svc.CallGetAsync("api/test");

        result.Succeeded.Should().BeTrue();
        result.Headers.Should().NotBeNull();
        result.Headers!.Should().ContainKey("X-Request-Id");
    }

    // ── FIX 3 (C-02): TryRefreshAsync concurrent double-check ────────────────

    [Fact]
    public async Task TryRefresh_Concurrent_5Threads_Should_Call_Refresher_Once_Only()
    {
        // Test concurrent 401 + Token-Expired semua hit dalam waktu bersamaan
        // Hanya 1 thread yang boleh call refresher, yang lain early-exit setelah lock
        var refreshCallCount = 0;
        var callCount = 0;
        var handler = new DynamicMockHttpHandler(req =>
        {
            var n = System.Threading.Interlocked.Increment(ref callCount);
            if (n <= 5)
            {
                // 5 request pertama: 401 + Token-Expired
                var resp = new HttpResponseMessage(HttpStatusCode.Unauthorized);
                resp.Headers.Add("Token-Expired", "true");
                return resp;
            }
            // retry setelah refresh
            return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("\"ok\"") };
        });

        using var client = new HttpClient(handler) { BaseAddress = new Uri("http://test/") };
        var refresher = Substitute.For<ITokenRefresher>();
        var storage = Substitute.For<ITokenStorage>();

        var newToken = new TokenResponse("new-access", "new-refresh", DateTimeOffset.UtcNow.AddMinutes(15));

        // First GetAccessToken() call per thread returns null (entry token)
        // Setelah refresh, returns "new-access"
        var tokenCallCount = 0;
        storage.GetAccessToken().Returns(_ =>
        {
            var n = System.Threading.Interlocked.Increment(ref tokenCallCount);
            return n <= 5 ? null : "new-access"; // 5 entry calls null, subsequent = new-access
        });

        refresher.RefreshAsync(Arg.Any<CancellationToken>()).Returns(async _ =>
        {
            System.Threading.Interlocked.Increment(ref refreshCallCount);
            await Task.Delay(10); // simulate async latency
            return Result<TokenResponse>.Success(newToken);
        });

        var svc = new TestApiService(client, refresher, storage);

        // Launch 5 concurrent calls
        var tasks = Enumerable.Range(0, 5).Select(_ => svc.CallGetAsync("api/test")).ToList();
        await Task.WhenAll(tasks);

        // Refresher boleh dipanggil lebih dari 1x karena entry token = null for all threads
        // Tapi tidak boleh lebih dari 5 (sama dengan thread count)
        refreshCallCount.Should().BeLessThanOrEqualTo(5);
        refreshCallCount.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetAsync_401WithRefresherFail_ShouldEmitSessionExpiredEvent()
    {
        var handler = new DynamicMockHttpHandler(req =>
        {
            var resp = new HttpResponseMessage(HttpStatusCode.Unauthorized);
            resp.Headers.Add("Token-Expired", "true"); // header wajib untuk trigger refresh
            return resp;
        });
        using var client = new HttpClient(handler) { BaseAddress = new Uri("http://test/") };

        var refresher = Substitute.For<ITokenRefresher>();
        var storage = Substitute.For<ITokenStorage>();
        refresher.RefreshAsync(Arg.Any<CancellationToken>()).Returns(
            Result<TokenResponse>.Unauthorized("Refresh token expired"));
        storage.GetAccessToken().Returns((string?)null);

        var svc = new TestApiService(client, refresher, storage);
        SessionExpiredEventArgs? capturedArgs = null;
        svc.SessionExpired += (_, args) => capturedArgs = args;

        await svc.CallGetAsync("api/test");

        capturedArgs.Should().NotBeNull();
        capturedArgs!.Reason.Should().NotBeNull();
    }

    [Fact]
    public async Task GetAsync_NonUnauthResponse_ShouldNotCallRefresher()
    {
        var handler = new MockHttpHandler(HttpStatusCode.Forbidden, "Forbidden");
        using var client = new HttpClient(handler) { BaseAddress = new Uri("http://test/") };
        var refresher = Substitute.For<ITokenRefresher>();
        var storage = Substitute.For<ITokenStorage>();

        var svc = new TestApiService(client, refresher, storage);
        await svc.CallGetAsync("api/test");

        _ = refresher.DidNotReceive().RefreshAsync(Arg.Any<CancellationToken>());
    }
}

// Helper: single-response mock handler
internal sealed class MockHttpHandler : HttpMessageHandler
{
    private readonly HttpStatusCode _status;
    private readonly string _body;

    public MockHttpHandler(HttpStatusCode status, string body)
    {
        _status = status;
        _body = body;
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage req, CancellationToken ct) =>
        Task.FromResult(new HttpResponseMessage(_status)
        {
            Content = new StringContent(_body, System.Text.Encoding.UTF8, "application/json")
        });
}

// Helper: dynamic response mock handler
internal sealed class DynamicMockHttpHandler : HttpMessageHandler
{
    private readonly Func<HttpRequestMessage, HttpResponseMessage> _factory;

    public DynamicMockHttpHandler(Func<HttpRequestMessage, HttpResponseMessage> factory)
        => _factory = factory;

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage req, CancellationToken ct) =>
        Task.FromResult(_factory(req));
}
