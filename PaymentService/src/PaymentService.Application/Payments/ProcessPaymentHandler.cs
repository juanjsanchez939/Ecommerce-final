using PaymentService.Domain.Entities;
using MediatR;

namespace PaymentService.Application.Payments;

public class ProcessPaymentHandler(IPaymentRepository repository) : IRequestHandler<ProcessPaymentRequest, ProcessPaymentResponse>
{
    public async Task<ProcessPaymentResponse> Handle(ProcessPaymentRequest request, CancellationToken cancellationToken)
    {
        var payment = Payment.Process(request.OrderId, request.Amount);
        await repository.AddAsync(payment, cancellationToken);

        return new ProcessPaymentResponse(payment.Status.ToString(), payment.TransactionId);
    }
}
