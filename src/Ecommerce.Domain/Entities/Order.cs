using Ecommerce.Domain.Enums;
using Ecommerce.Domain.ValueObjects;

namespace Ecommerce.Domain.Entities;

public class Order
{
    public Guid Id { get; private set; }
    public DateTime OrderDate { get; private set; }
    public OrderStatus Status { get; private set; }
    public Address ShippingAddress { get; private set; }
    public decimal TotalAmount { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    public Guid UserId { get; private set; }
    public User User { get; private set; }

    public ICollection<OrderItem> OrderItems { get; private set; } = new List<OrderItem>();

    private Order() { }

    public Order(Guid userId, Address shippingAddress)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        OrderDate = DateTime.UtcNow;
        Status = OrderStatus.Pending;
        ShippingAddress = shippingAddress;
        CreatedAt = DateTime.UtcNow;
    }

    public void AddItem(OrderItem item)
    {
        OrderItems.Add(item);
        RecalculateTotal();
    }

    public void UpdateStatus(OrderStatus newStatus)
    {
        Status = newStatus;
        UpdatedAt = DateTime.UtcNow;
    }

    private void RecalculateTotal()
    {
        TotalAmount = OrderItems.Sum(i => i.Subtotal);
    }
}
