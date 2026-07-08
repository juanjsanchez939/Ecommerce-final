namespace Ecommerce.Domain.Entities;

public class CartItem
{
    public Guid Id { get; private set; }
    public Guid CartId { get; private set; }
    public Guid ProductId { get; private set; }
    public string ProductName { get; private set; }
    public int Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public string Currency { get; private set; }

    public Cart Cart { get; private set; }

    private CartItem() { }

    public CartItem(Guid productId, string productName, decimal unitPrice, int quantity = 1, string currency = "USD")
    {
        Id = Guid.NewGuid();
        ProductId = productId;
        ProductName = productName;
        UnitPrice = unitPrice;
        Currency = currency;
        Quantity = quantity;
    }

    public void UpdateQuantity(int quantity)
    {
        Quantity = quantity;
    }

    public decimal Subtotal => UnitPrice * Quantity;
}
