using System.Net.Http.Json;
using System.Text.Json;
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
            using var response = await _httpClient.PostAsJsonAsync(
                "api/payments/process",
                new PaymentRequestBody(orderId, amount),
                cancellationToken);

            response.EnsureSuccessStatusCode();

            var body = await response.Content.ReadFromJsonAsync<PaymentResponseBody>(cancellationToken)
                ?? throw new PaymentServiceUnavailableException("PaymentService returned an empty response.");

            if ((body.Status != "Approved" && body.Status != "Rejected") || string.IsNullOrWhiteSpace(body.TransactionId))
                throw new PaymentServiceUnavailableException("PaymentService returned an invalid response.");

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
        catch (JsonException ex)
        {
            throw new PaymentServiceUnavailableException("PaymentService returned an invalid response.", ex);
        }
    }

    private sealed record PaymentRequestBody(Guid OrderId, decimal Amount);
    private sealed record PaymentResponseBody(string Status, string TransactionId);
}
