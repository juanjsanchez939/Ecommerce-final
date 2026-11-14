namespace PaymentService.Application.Payments;

using MediatR;

public record ProcessPaymentRequest(Guid OrderId, decimal Amount) : IRequest<ProcessPaymentResponse>;

public record ProcessPaymentResponse(string Status, string TransactionId);
