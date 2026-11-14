using PaymentService.Domain.Entities;
using PaymentService.Domain.Enums;

namespace PaymentService.Tests;

public class PaymentRuleTests
{
    [Theory]
    [InlineData(1000, PaymentStatus.Approved)]
    [InlineData(99999.99, PaymentStatus.Approved)]
    [InlineData(100000, PaymentStatus.Rejected)]
    [InlineData(250000, PaymentStatus.Rejected)]
    public void Process_AppliesLimitOf100000(decimal amount, PaymentStatus expected)
    {
        var payment = Payment.Process(Guid.NewGuid(), amount);

        Assert.Equal(expected, payment.Status);
        Assert.False(string.IsNullOrEmpty(payment.TransactionId));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void Process_RejectsNonPositiveAmount(decimal amount)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Payment.Process(Guid.NewGuid(), amount));
    }
}
