using System.Net;
using Ecommerce.Application.Common.Exceptions;
using Ecommerce.Infrastructure.Services;

namespace Ecommerce.IntegrationTests.Orders;

public class PaymentClientTests
{
    [Theory]
    [InlineData("{\"status\":\"Unknown\",\"transactionId\":\"tx\"}")]
    [InlineData("{\"status\":\"Approved\",\"transactionId\":\"\"}")]
    [InlineData("not json")]
    public async Task InvalidResponse_IsNotTreatedAsRejection(string response)
    {
        using var http = new HttpClient(new StubHandler(response)) { BaseAddress = new Uri("http://localhost/") };
        var client = new PaymentClient(http);

        await Assert.ThrowsAsync<PaymentServiceUnavailableException>(
            () => client.ProcessPaymentAsync(Guid.NewGuid(), 100));
    }

    [Fact]
    public async Task Timeout_ReportsUnknownPaymentResult()
    {
        using var http = new HttpClient(new TimeoutHandler()) { BaseAddress = new Uri("http://localhost/") };
        var client = new PaymentClient(http);

        var error = await Assert.ThrowsAsync<PaymentServiceUnavailableException>(
            () => client.ProcessPaymentAsync(Guid.NewGuid(), 100));
        Assert.Contains("time", error.Message);
    }

    private sealed class StubHandler(string response) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(response) });
    }

    private sealed class TimeoutHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
            throw new TaskCanceledException("Simulated timeout");
    }
}
