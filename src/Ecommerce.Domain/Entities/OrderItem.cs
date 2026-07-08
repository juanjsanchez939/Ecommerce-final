using Ecommerce.Domain.ValueObjects;

namespace Ecommerce.Domain.Entities;

public class OrderItem
{
    public Guid Id { get; private set; }
    public Guid OrderId { get; private set; }
    public Guid ProductId { get; private set; }
    public string ProductName { get; private set; }
    public Money UnitPrice { get; private set; }
    public int Quantity { get; private set; }

    public Order Order { get; private set; }
    public Product Product { get; private set; }

    private OrderItem() { }

    public OrderItem(Guid productId, string productName, Money unitPrice, int quantity)
    {
        Id = Guid.NewGuid();
        ProductId = productId;
        ProductName = productName;
        UnitPrice = unitPrice;
        Quantity = quantity;
    }

    public decimal Subtotal => UnitPrice.Amount * Quantity;
}
