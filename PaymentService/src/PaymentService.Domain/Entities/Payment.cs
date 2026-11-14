using PaymentService.Domain.Enums;

namespace PaymentService.Domain.Entities;

public class Payment
{
    public const decimal ApprovalLimit = 100000m;

    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid OrderId { get; private set; }
    public decimal Amount { get; private set; }
    public PaymentStatus Status { get; private set; }
    public string TransactionId { get; private set; } = string.Empty;

    private Payment() { }

    public static Payment Process(Guid orderId, decimal amount)
    {
        if (orderId == Guid.Empty)
            throw new ArgumentOutOfRangeException(nameof(orderId), "OrderId must not be empty.");
        if (amount <= 0)
            throw new ArgumentOutOfRangeException(nameof(amount), "Amount must be greater than zero.");

        return new Payment
        {
            OrderId = orderId,
            Amount = amount,
            Status = amount < ApprovalLimit ? PaymentStatus.Approved : PaymentStatus.Rejected,
            TransactionId = Guid.NewGuid().ToString("N")
        };
    }
}
