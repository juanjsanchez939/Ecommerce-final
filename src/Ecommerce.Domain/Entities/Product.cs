using Ecommerce.Domain.ValueObjects;

namespace Ecommerce.Domain.Entities;

public class Product
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public Money Price { get; private set; }
    public int Stock { get; private set; }
    public string ImageUrl { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    public Guid CategoryId { get; private set; }
    public Category Category { get; private set; }

    public ICollection<OrderItem> OrderItems { get; private set; } = new List<OrderItem>();

    private Product() { }

    public Product(string name, string description, Money price, int stock, string imageUrl, Guid categoryId)
    {
        Id = Guid.NewGuid();
        Name = name;
        Description = description;
        Price = price;
        Stock = stock;
        ImageUrl = imageUrl;
        IsActive = true;
        CategoryId = categoryId;
        CreatedAt = DateTime.UtcNow;
    }

    public void Update(string name, string description, Money price, int stock, string imageUrl, Guid categoryId)
    {
        Name = name;
        Description = description;
        Price = price;
        Stock = stock;
        ImageUrl = imageUrl;
        CategoryId = categoryId;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ReduceStock(int quantity)
    {
        if (quantity > Stock)
            throw new InvalidOperationException("Insufficient stock.");

        Stock -= quantity;
    }

    public void IncreaseStock(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be positive.", nameof(quantity));

        Stock += quantity;
    }
}
