using System.Net.Http.Json;
using Ecommerce.Application.Common.Exceptions;
using Ecommerce.Application.Common.Interfaces;

namespace Ecommerce.Infrastructure.Services;

public class PaymentClient : IPaymentClient
{
    private readonly HttpClient _httpClient;

    public PaymentClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<PaymentResult> ProcessPaymentAsync(Guid orderId, decimal amount, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync(
                "api/payments/process",
                new { orderId, amount },
                cancellationToken);

            response.EnsureSuccessStatusCode();

            var body = await response.Content.ReadFromJsonAsync<PaymentResponseBody>(cancellationToken)
                ?? throw new PaymentServiceUnavailableException("PaymentService returned an empty response.");

            return new PaymentResult(body.Status == "Approved", body.TransactionId);
        }
        catch (HttpRequestException ex)
        {
            throw new PaymentServiceUnavailableException("PaymentService is unavailable.", ex);
        }
        catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            throw new PaymentServiceUnavailableException("PaymentService did not respond in time.", ex);
        }
    }

    private sealed record PaymentResponseBody(string Status, string TransactionId);
}
