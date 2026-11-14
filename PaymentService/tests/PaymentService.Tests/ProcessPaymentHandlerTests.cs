using PaymentService.Application.Payments;
using PaymentService.Domain.Entities;

namespace PaymentService.Tests;

public class ProcessPaymentHandlerTests
{
    [Theory]
    [InlineData(99999, "Approved")]
    [InlineData(100000, "Rejected")]
    public async Task Handle_StoresPaymentAndReturnsItsOutcome(decimal amount, string expected)
    {
        var repository = new RecordingRepository();
        var orderId = Guid.NewGuid();
        var response = await new ProcessPaymentHandler(repository)
            .Handle(new ProcessPaymentRequest(orderId, amount), CancellationToken.None);

        Assert.NotNull(repository.Payment);
        Assert.Equal(orderId, repository.Payment.OrderId);
        Assert.Equal(expected, response.Status);
        Assert.Equal(repository.Payment.TransactionId, response.TransactionId);
    }

    private sealed class RecordingRepository : IPaymentRepository
    {
        public Payment? Payment { get; private set; }

        public Task AddAsync(Payment payment, CancellationToken cancellationToken = default)
        {
            Payment = payment;
            return Task.CompletedTask;
        }
    }
}
