using System.Net;
using System.Net.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using PurchaseBill.Application.Exceptions;
using PurchaseBill.Infrastructure.ExternalServices;
using Xunit;

namespace PurchaseBill.Tests.ExternalServices;

public class EnhanzerAuthClientTests
{
    private const string SuccessBody =
        """{"Status_Code":200,"Message":"ok","Response_Body":[{"Email":"info@enhanzer.com","User_Locations":[{"Location_Code":"LOC-1","Location_Name":"Head Office"}]}]}""";

    private static EnhanzerAuthClient BuildClient(StubHttpMessageHandler handler)
    {
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://ez-staging-api.example/") };
        var options = Options.Create(new EnhanzerOptions { BaseUrl = httpClient.BaseAddress.ToString(), DeviceId = "D001" });
        return new EnhanzerAuthClient(httpClient, options, NullLogger<EnhanzerAuthClient>.Instance);
    }

    [Fact]
    public async Task GetLoginDataAsync_OnFirstTrySuccess_ReturnsEnvelopeWithoutRetrying()
    {
        var handler = new StubHttpMessageHandler().Enqueue(HttpStatusCode.OK, SuccessBody);
        var client = BuildClient(handler);

        var envelope = await client.GetLoginDataAsync("info@enhanzer.com", "Welcome#5");

        Assert.Equal(200, envelope.StatusCode);
        Assert.Single(envelope.ResponseBody!);
        Assert.Equal(1, handler.RequestCount);
    }

    [Fact]
    public async Task GetLoginDataAsync_WhenFirstResponseBodyIsUnparseable_RetriesAndReturnsTheSecondEnvelope()
    {
        // Reproduces the reported bug: a 200 whose body is truncated/invalid JSON even though
        // the login actually succeeded on Enhanzer's side - this must not be reported as
        // "authentication failed" if a retry can read a good response.
        var handler = new StubHttpMessageHandler()
            .Enqueue(HttpStatusCode.OK, "{\"Status_Code\":200,\"Response_Bo") // truncated
            .Enqueue(HttpStatusCode.OK, SuccessBody);
        var client = BuildClient(handler);

        var envelope = await client.GetLoginDataAsync("info@enhanzer.com", "Welcome#5");

        Assert.Equal(200, envelope.StatusCode);
        Assert.Equal(2, handler.RequestCount);
    }

    [Fact]
    public async Task GetLoginDataAsync_WhenEveryAttemptIsUnparseable_ThrowsAnHonestRetryMessage_NotAuthFailure()
    {
        var handler = new StubHttpMessageHandler()
            .Enqueue(HttpStatusCode.OK, "not json at all")
            .Enqueue(HttpStatusCode.OK, "not json at all");
        var client = BuildClient(handler);

        var ex = await Assert.ThrowsAsync<AuthenticationFailedException>(
            () => client.GetLoginDataAsync("info@enhanzer.com", "Welcome#5"));

        Assert.DoesNotContain("nvalid", ex.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("try again", ex.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(2, handler.RequestCount);
    }

    [Fact]
    public async Task GetLoginDataAsync_WhenEmptyBodyOnEveryAttempt_ThrowsAfterRetrying()
    {
        var handler = new StubHttpMessageHandler()
            .Enqueue(HttpStatusCode.OK, "null")
            .Enqueue(HttpStatusCode.OK, "null");
        var client = BuildClient(handler);

        await Assert.ThrowsAsync<AuthenticationFailedException>(() => client.GetLoginDataAsync("info@enhanzer.com", "Welcome#5"));
        Assert.Equal(2, handler.RequestCount);
    }

    [Fact]
    public async Task GetLoginDataAsync_OnNetworkFailureThenSuccess_RetriesAndSucceeds()
    {
        var handler = new StubHttpMessageHandler()
            .EnqueueThrow(new HttpRequestException("connection reset"))
            .Enqueue(HttpStatusCode.OK, SuccessBody);
        var client = BuildClient(handler);

        var envelope = await client.GetLoginDataAsync("info@enhanzer.com", "Welcome#5");

        Assert.Equal(200, envelope.StatusCode);
        Assert.Equal(2, handler.RequestCount);
    }

    [Fact]
    public async Task GetLoginDataAsync_WhenNonSuccessHttpStatus_ThrowsImmediatelyWithoutRetrying()
    {
        var handler = new StubHttpMessageHandler().Enqueue(HttpStatusCode.InternalServerError, "oops");
        var client = BuildClient(handler);

        await Assert.ThrowsAsync<AuthenticationFailedException>(() => client.GetLoginDataAsync("info@enhanzer.com", "Welcome#5"));
        Assert.Equal(1, handler.RequestCount);
    }
}
