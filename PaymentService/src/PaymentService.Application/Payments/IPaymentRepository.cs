using PaymentService.Domain.Entities;

namespace PaymentService.Application.Payments;

public interface IPaymentRepository
{
    Task AddAsync(Payment payment, CancellationToken cancellationToken = default);
}
