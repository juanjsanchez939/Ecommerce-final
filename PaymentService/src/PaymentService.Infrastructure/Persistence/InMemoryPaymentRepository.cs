using PaymentService.Application.Payments;
using PaymentService.Domain.Entities;

namespace PaymentService.Infrastructure.Persistence;

public class InMemoryPaymentRepository : IPaymentRepository
{
    private readonly List<Payment> _payments = [];

    public Task AddAsync(Payment payment, CancellationToken cancellationToken = default)
    {
        lock (_payments)
        {
            _payments.Add(payment);
        }
        return Task.CompletedTask;
    }
}
