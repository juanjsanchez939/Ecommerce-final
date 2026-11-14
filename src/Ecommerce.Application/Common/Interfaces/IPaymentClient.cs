namespace Ecommerce.Application.Common.Interfaces;

public record PaymentResult(bool Approved, string TransactionId);

public interface IPaymentClient
{
    Task<PaymentResult> ProcessPaymentAsync(Guid orderId, decimal amount, CancellationToken cancellationToken = default);
}
