using Ecommerce.Application.Common.Exceptions;
using Ecommerce.Application.Common.Interfaces;
using Ecommerce.Application.Features.Products.Commands;
using Ecommerce.Application.Features.Products.Queries;
using Ecommerce.Domain.Entities;
using Ecommerce.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace Ecommerce.Application.Tests.Products;

public class TestDbContext : DbContext, IApplicationDbContext
{
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Cart> Carts => Set<Cart>();
    public DbSet<CartItem> CartItems => Set<CartItem>();

    public TestDbContext(DbContextOptions<TestDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<Product>().OwnsOne(p => p.Price);
        builder.Entity<Product>().HasOne(p => p.Category).WithMany(c => c.Products).HasForeignKey(p => p.CategoryId);
        builder.Entity<Category>().HasKey(c => c.Id);
        builder.Entity<Order>().OwnsOne(o => o.ShippingAddress);
        builder.Entity<Order>().HasMany(o => o.OrderItems).WithOne(oi => oi.Order).HasForeignKey(oi => oi.OrderId);
        builder.Entity<OrderItem>().OwnsOne(oi => oi.UnitPrice);
        builder.Entity<OrderItem>().HasOne(oi => oi.Product).WithMany(p => p.OrderItems).HasForeignKey(oi => oi.ProductId);
        builder.Entity<User>().HasKey(u => u.Id);
        builder.Entity<Cart>().HasKey(c => c.Id);
        builder.Entity<Cart>().HasOne(c => c.User).WithOne(u => u.Cart).HasForeignKey<Cart>(c => c.UserId);
        builder.Entity<Cart>().HasMany(c => c.CartItems).WithOne(ci => ci.Cart).HasForeignKey(ci => ci.CartId);
        builder.Entity<CartItem>().HasKey(ci => ci.Id);
        builder.Ignore<Address>();
        builder.Ignore<Money>();
    }
}

public class ProductHandlersTests : IDisposable
{
    private readonly TestDbContext _context;
    private readonly ILogger<CreateProductHandler> _createLogger;
    private readonly ILogger<UpdateProductHandler> _updateLogger;

    public ProductHandlersTests()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new TestDbContext(options);
        _createLogger = Mock.Of<ILogger<CreateProductHandler>>();
        _updateLogger = Mock.Of<ILogger<UpdateProductHandler>>();
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    private async Task<Category> SeedCategoryAsync(string name = "Electronics", string description = "Electronic devices")
    {
        var category = new Category(name, description);
        await _context.Categories.AddAsync(category);
        await _context.SaveChangesAsync();
        return category;
    }

    private async Task<Product> SeedProductAsync(Category category, string name = "Laptop", decimal price = 999.99m)
    {
        var product = new Product(name, "A test product", new Money(price), 10, "https://example.com/img.jpg", category.Id);
        await _context.Products.AddAsync(product);
        await _context.SaveChangesAsync();
        return product;
    }

    private async Task<(Product, Category)> SeedProductWithCategoryAsync()
    {
        var category = await SeedCategoryAsync();
        var product = await SeedProductAsync(category);
        return (product, category);
    }

    // ==================== CreateProductHandler ====================

    [Fact]
    public async Task CreateProductHandler_ShouldCreateProduct_WhenCategoryExists()
    {
        var category = await SeedCategoryAsync();

        var handler = new CreateProductHandler(_context, _createLogger);
        var command = new CreateProductCommand
        {
            Name = "Laptop",
            Description = "A powerful laptop",
            Price = 999.99m,
            Stock = 10,
            ImageUrl = "https://example.com/laptop.jpg",
            CategoryId = category.Id
        };

        var productId = await handler.Handle(command, CancellationToken.None);

        Assert.NotEqual(Guid.Empty, productId);
        var product = await _context.Products.FindAsync(productId);
        Assert.NotNull(product);
        Assert.Equal("Laptop", product.Name);
        Assert.Equal("A powerful laptop", product.Description);
        Assert.Equal(999.99m, product.Price.Amount);
        Assert.Equal(10, product.Stock);
        Assert.Equal("https://example.com/laptop.jpg", product.ImageUrl);
        Assert.Equal(category.Id, product.CategoryId);
        Assert.True(product.IsActive);
    }

    [Fact]
    public async Task CreateProductHandler_ShouldThrowNotFoundException_WhenCategoryDoesNotExist()
    {
        var handler = new CreateProductHandler(_context, _createLogger);
        var command = new CreateProductCommand
        {
            Name = "Laptop",
            Description = "A powerful laptop",
            Price = 999.99m,
            Stock = 10,
            ImageUrl = "https://example.com/laptop.jpg",
            CategoryId = Guid.NewGuid()
        };

        var ex = await Assert.ThrowsAsync<NotFoundException>(
            () => handler.Handle(command, CancellationToken.None));

        Assert.Contains("Category", ex.Message);
    }

    // ==================== GetProductsHandler ====================

    [Fact]
    public async Task GetProductsHandler_ShouldReturnPagedResults()
    {
        var category = await SeedCategoryAsync();
        for (int i = 1; i <= 25; i++)
        {
            var product = new Product($"Product {i}", $"Description {i}", new Money(10m + i), 10, "url", category.Id);
            await _context.Products.AddAsync(product);
        }
        await _context.SaveChangesAsync();

        var handler = new GetProductsHandler(_context);
        var query = new GetProductsQuery { PageNumber = 1, PageSize = 10 };

        var result = await handler.Handle(query, CancellationToken.None);

        Assert.Equal(10, result.Items.Count);
        Assert.Equal(25, result.TotalCount);
        Assert.Equal(3, result.TotalPages);
        Assert.Equal(1, result.CurrentPage);
        Assert.True(result.HasNextPage);
        Assert.False(result.HasPreviousPage);
    }

    [Fact]
    public async Task GetProductsHandler_ShouldReturnSecondPage()
    {
        var category = await SeedCategoryAsync();
        for (int i = 1; i <= 25; i++)
        {
            var product = new Product($"Product {i}", $"Description {i}", new Money(10m + i), 10, "url", category.Id);
            await _context.Products.AddAsync(product);
        }
        await _context.SaveChangesAsync();

        var handler = new GetProductsHandler(_context);
        var query = new GetProductsQuery { PageNumber = 2, PageSize = 10 };

        var result = await handler.Handle(query, CancellationToken.None);

        Assert.Equal(10, result.Items.Count);
        Assert.Equal(25, result.TotalCount);
        Assert.True(result.HasPreviousPage);
        Assert.True(result.HasNextPage);
    }

    [Fact]
    public async Task GetProductsHandler_ShouldFilterBySearchTerm()
    {
        var category = await SeedCategoryAsync();
        var laptop = new Product("Laptop Pro", "High-end laptop", new Money(1500m), 5, "url", category.Id);
        var mouse = new Product("Wireless Mouse", "Ergonomic mouse", new Money(50m), 20, "url", category.Id);
        var keyboard = new Product("Mechanical Keyboard", "Gaming keyboard", new Money(120m), 15, "url", category.Id);
        await _context.Products.AddRangeAsync(laptop, mouse, keyboard);
        await _context.SaveChangesAsync();

        var handler = new GetProductsHandler(_context);
        var query = new GetProductsQuery { SearchTerm = "LAPTOP" };

        var result = await handler.Handle(query, CancellationToken.None);

        Assert.Single(result.Items);
        Assert.Equal("Laptop Pro", result.Items[0].Name);
    }

    [Fact]
    public async Task GetProductsHandler_ShouldFilterByCategory()
    {
        var electronics = await SeedCategoryAsync("Electronics", "Electronic devices");
        var clothing = new Category("Clothing", "Apparel and accessories");
        await _context.Categories.AddAsync(clothing);
        await _context.SaveChangesAsync();

        var laptop = new Product("Laptop", "A laptop", new Money(1000m), 10, "url", electronics.Id);
        var tshirt = new Product("T-Shirt", "Cotton t-shirt", new Money(25m), 50, "url", clothing.Id);
        var phone = new Product("Smartphone", "A smartphone", new Money(800m), 20, "url", electronics.Id);
        await _context.Products.AddRangeAsync(laptop, tshirt, phone);
        await _context.SaveChangesAsync();

        var handler = new GetProductsHandler(_context);
        var query = new GetProductsQuery { CategoryId = electronics.Id };

        var result = await handler.Handle(query, CancellationToken.None);

        Assert.Equal(2, result.Items.Count);
        Assert.All(result.Items, p => Assert.Equal(electronics.Id, p.CategoryId));
    }

    // ==================== GetProductByIdHandler ====================

    [Fact]
    public async Task GetProductByIdHandler_ShouldReturnProduct_WhenExists()
    {
        var (product, category) = await SeedProductWithCategoryAsync();

        var handler = new GetProductByIdHandler(_context);
        var query = new GetProductByIdQuery(product.Id);

        var result = await handler.Handle(query, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(product.Id, result.Id);
        Assert.Equal(product.Name, result.Name);
        Assert.Equal(product.Description, result.Description);
        Assert.Equal(product.Price.Amount, result.Price);
        Assert.Equal(product.Stock, result.Stock);
        Assert.Equal(product.ImageUrl, result.ImageUrl);
        Assert.Equal(product.IsActive, result.IsActive);
        Assert.Equal(product.CategoryId, result.CategoryId);
        Assert.Equal(category.Name, result.CategoryName);
    }

    [Fact]
    public async Task GetProductByIdHandler_ShouldThrowNotFoundException_WhenNotFound()
    {
        var handler = new GetProductByIdHandler(_context);
        var query = new GetProductByIdQuery(Guid.NewGuid());

        var ex = await Assert.ThrowsAsync<NotFoundException>(
            () => handler.Handle(query, CancellationToken.None));

        Assert.Contains("Product", ex.Message);
    }

    // ==================== UpdateProductHandler ====================

    [Fact]
    public async Task UpdateProductHandler_ShouldUpdateSuccessfully()
    {
        var (product, category) = await SeedProductWithCategoryAsync();

        var handler = new UpdateProductHandler(_context, _updateLogger);
        var command = new UpdateProductCommand
        {
            Id = product.Id,
            Name = "Updated Laptop",
            Description = "Updated description",
            Price = 1299.99m,
            Stock = 5,
            ImageUrl = "https://example.com/updated.jpg",
            CategoryId = category.Id
        };

        await handler.Handle(command, CancellationToken.None);

        var updated = await _context.Products.FindAsync(new object[] { product.Id }, CancellationToken.None);
        Assert.NotNull(updated);
        Assert.Equal("Updated Laptop", updated.Name);
        Assert.Equal("Updated description", updated.Description);
        Assert.Equal(1299.99m, updated.Price.Amount);
        Assert.Equal(5, updated.Stock);
        Assert.Equal("https://example.com/updated.jpg", updated.ImageUrl);
        Assert.Equal(category.Id, updated.CategoryId);
        Assert.NotNull(updated.UpdatedAt);
    }

    [Fact]
    public async Task UpdateProductHandler_ShouldThrowNotFoundException_WhenProductNotFound()
    {
        var handler = new UpdateProductHandler(_context, _updateLogger);
        var command = new UpdateProductCommand
        {
            Id = Guid.NewGuid(),
            Name = "Nonexistent",
            Description = "Does not exist",
            Price = 100m,
            Stock = 1,
            ImageUrl = "url",
            CategoryId = Guid.NewGuid()
        };

        var ex = await Assert.ThrowsAsync<NotFoundException>(
            () => handler.Handle(command, CancellationToken.None));

        Assert.Contains("Product", ex.Message);
    }

    [Fact]
    public async Task UpdateProductHandler_ShouldThrowNotFoundException_WhenCategoryDoesNotExist()
    {
        var (product, _) = await SeedProductWithCategoryAsync();

        var handler = new UpdateProductHandler(_context, _updateLogger);
        var command = new UpdateProductCommand
        {
            Id = product.Id,
            Name = "Updated Product",
            Description = "Updated description",
            Price = 100m,
            Stock = 1,
            ImageUrl = "url",
            CategoryId = Guid.NewGuid()
        };

        var ex = await Assert.ThrowsAsync<NotFoundException>(
            () => handler.Handle(command, CancellationToken.None));

        Assert.Contains("Category", ex.Message);
    }

    // ==================== DeleteProductHandler ====================

    [Fact]
    public async Task DeleteProductHandler_ShouldDeactivateProduct()
    {
        var (product, _) = await SeedProductWithCategoryAsync();

        var handler = new DeleteProductHandler(_context);
        var command = new DeleteProductCommand(product.Id);

        await handler.Handle(command, CancellationToken.None);

        var deactivated = await _context.Products.FindAsync(new object[] { product.Id }, CancellationToken.None);
        Assert.NotNull(deactivated);
        Assert.False(deactivated.IsActive);
        Assert.NotNull(deactivated.UpdatedAt);
    }

    [Fact]
    public async Task DeleteProductHandler_ShouldThrowNotFoundException_WhenNotFound()
    {
        var handler = new DeleteProductHandler(_context);
        var command = new DeleteProductCommand(Guid.NewGuid());

        var ex = await Assert.ThrowsAsync<NotFoundException>(
            () => handler.Handle(command, CancellationToken.None));

        Assert.Contains("Product", ex.Message);
    }
}
