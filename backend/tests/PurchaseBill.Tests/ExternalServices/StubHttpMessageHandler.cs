using System.Net;
using System.Net.Http;

namespace PurchaseBill.Tests.ExternalServices;

/// <summary>Replays a fixed queue of responses (or throws), one per request, for testing an HttpClient consumer.</summary>
public class StubHttpMessageHandler : HttpMessageHandler
{
    private readonly Queue<Func<HttpResponseMessage>> _responses = new();

    public int RequestCount { get; private set; }

    public StubHttpMessageHandler Enqueue(HttpStatusCode statusCode, string content)
    {
        _responses.Enqueue(() => new HttpResponseMessage(statusCode) { Content = new StringContent(content) });
        return this;
    }

    public StubHttpMessageHandler EnqueueThrow<TException>(TException exception) where TException : Exception
    {
        _responses.Enqueue(() => throw exception);
        return this;
    }

    /// <summary>A 200 OK whose body throws when actually read, simulating a stream fault partway through the response.</summary>
    public StubHttpMessageHandler EnqueueOkWithUnreadableBody(Exception readException)
    {
        _responses.Enqueue(() => new HttpResponseMessage(HttpStatusCode.OK) { Content = new ThrowingContent(readException) });
        return this;
    }

    private class ThrowingContent(Exception exception) : HttpContent
    {
        protected override Task SerializeToStreamAsync(Stream stream, TransportContext? context) => throw exception;

        protected override bool TryComputeLength(out long length)
        {
            length = 0;
            return false;
        }
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        RequestCount++;
        if (_responses.Count == 0)
        {
            throw new InvalidOperationException("No more stubbed responses were queued.");
        }

        return Task.FromResult(_responses.Dequeue()());
    }
}
