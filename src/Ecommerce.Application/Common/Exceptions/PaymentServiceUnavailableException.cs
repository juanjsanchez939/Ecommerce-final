namespace Ecommerce.Application.Common.Exceptions;

public class PaymentServiceUnavailableException : Exception
{
    public PaymentServiceUnavailableException(string message) : base(message) { }
    public PaymentServiceUnavailableException(string message, Exception innerException) : base(message, innerException) { }
}
