namespace Ecommerce.Domain.Entities;

public class Cart
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    public User User { get; private set; }
    public ICollection<CartItem> CartItems { get; private set; } = new List<CartItem>();

    private Cart() { }

    public Cart(Guid userId)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        CreatedAt = DateTime.UtcNow;
    }

    public void AddItem(CartItem item)
    {
        var existingItem = CartItems.FirstOrDefault(ci => ci.ProductId == item.ProductId);

        if (existingItem is not null)
        {
            existingItem.UpdateQuantity(existingItem.Quantity + item.Quantity);
        }
        else
        {
            CartItems.Add(item);
        }

        UpdatedAt = DateTime.UtcNow;
    }

    public void RemoveItem(Guid productId)
    {
        var item = CartItems.FirstOrDefault(ci => ci.ProductId == productId);
        if (item is not null)
        {
            CartItems.Remove(item);
            UpdatedAt = DateTime.UtcNow;
        }
    }

    public decimal Total => CartItems.Sum(ci => ci.Subtotal);

    public void Clear()
    {
        CartItems.Clear();
        UpdatedAt = DateTime.UtcNow;
    }
}
