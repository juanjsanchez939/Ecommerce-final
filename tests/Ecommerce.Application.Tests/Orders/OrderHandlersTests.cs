using Ecommerce.Application.Common.Exceptions;
using Ecommerce.Application.Common.Interfaces;
using Ecommerce.Application.Features.Orders.Commands;
using Ecommerce.Application.Features.Orders.Queries;
using Ecommerce.Domain.Entities;
using Ecommerce.Domain.Enums;
using Ecommerce.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Moq;

namespace Ecommerce.Application.Tests.Orders;

public class OrderHandlersTests
{
    private static TestDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        return new TestDbContext(options);
    }

    private static Mock<ICurrentUserService> CreateCurrentUserMock(Guid userId, bool isAuthenticated = true)
    {
        var mock = new Mock<ICurrentUserService>();
        mock.Setup(m => m.IsAuthenticated).Returns(isAuthenticated);
        mock.Setup(m => m.UserId).Returns(userId.ToString());
        mock.Setup(m => m.IsAdmin).Returns(false);
        return mock;
    }

    private static void SeedProductAndCart(TestDbContext context, Guid userId, Guid productId, string productName, decimal unitPrice, int productStock, int cartQuantity)
    {
        var category = new Category("Test Category", "Description");
        var product = new Product(productName, "Description", new Money(unitPrice), productStock, "url", category.Id);
        var user = new User("test@test.com", "hash", "Test", "User");

        var cart = new Cart(userId);
        var cartItem = new CartItem(product.Id, productName, unitPrice, cartQuantity);
        cart.AddItem(cartItem);

        context.Categories.Add(category);
        context.Products.Add(product);
        context.Users.Add(user);
        context.Carts.Add(cart);

        context.SaveChanges();
    }

    [Fact]
    public async Task Handle_ShouldCreateOrderSuccessfully_WhenCartHasItems()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var context = CreateContext();
        var mockUser = CreateCurrentUserMock(userId);

        var category = new Category("Test Category", "Description");
        var product = new Product("Test Product", "Description", new Money(10m), 10, "url", category.Id);
        var user = new User("test@test.com", "hash", "Test", "User");

        var cart = new Cart(userId);
        var cartItem = new CartItem(product.Id, product.Name, 10m, 2);
        cart.AddItem(cartItem);

        context.Categories.Add(category);
        context.Products.Add(product);
        context.Users.Add(user);
        context.Carts.Add(cart);
        await context.SaveChangesAsync();

        var handler = new CreateOrderHandler(context, mockUser.Object);
        var command = new CreateOrderCommand
        {
            Street = "123 Main St",
            City = "Test City",
            State = "TS",
            Country = "Testland",
            ZipCode = "12345"
        };

        // Act
        var orderId = await handler.Handle(command, CancellationToken.None);

        // Assert
        var order = await context.Orders
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.Id == orderId);
        Assert.NotNull(order);
        Assert.Equal(userId, order.UserId);
        Assert.Equal(OrderStatus.Pending, order.Status);
        Assert.Single(order.OrderItems);
        Assert.Equal(product.Id, order.OrderItems.First().ProductId);
        Assert.Equal(2, order.OrderItems.First().Quantity);
    }

    [Fact]
    public async Task Handle_ShouldThrowBadRequestException_WhenUserNotAuthenticated()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var context = CreateContext();
        var mockUser = CreateCurrentUserMock(userId, isAuthenticated: false);

        var handler = new CreateOrderHandler(context, mockUser.Object);
        var command = new CreateOrderCommand
        {
            Street = "123 Main St",
            City = "Test City",
            State = "TS",
            Country = "Testland",
            ZipCode = "12345"
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BadRequestException>(
            () => handler.Handle(command, CancellationToken.None));
        Assert.Equal("User not authenticated.", exception.Message);
    }

    [Fact]
    public async Task GetOrders_ShouldRejectUnauthenticatedUser()
    {
        var context = CreateContext();
        var mockUser = CreateCurrentUserMock(Guid.NewGuid(), isAuthenticated: false);
        var handler = new GetOrdersHandler(context, mockUser.Object);

        var exception = await Assert.ThrowsAsync<BadRequestException>(() =>
            handler.Handle(new GetOrdersQuery(), CancellationToken.None));

        Assert.Equal("User not authenticated.", exception.Message);
    }

    [Fact]
    public async Task GetOrders_ShouldOnlyReturnCurrentUsersOrders()
    {
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var context = CreateContext();
        var mockUser = CreateCurrentUserMock(userId);
        var address = new Address("1 Main St", "City", "State", "Country", "12345");

        var ownOrder = new Order(userId, address);
        ownOrder.AddItem(new OrderItem(Guid.NewGuid(), "Own item", new Money(10m), 1));
        var otherOrder = new Order(otherUserId, address);
        otherOrder.AddItem(new OrderItem(Guid.NewGuid(), "Other item", new Money(20m), 1));
        context.Orders.AddRange(ownOrder, otherOrder);
        await context.SaveChangesAsync();

        var handler = new GetOrdersHandler(context, mockUser.Object);
        var result = await handler.Handle(new GetOrdersQuery(), CancellationToken.None);

        var order = Assert.Single(result.Items);
        Assert.Equal(ownOrder.Id, order.Id);
    }

    [Fact]
    public async Task Handle_ShouldThrowBadRequestException_WhenCartIsEmpty()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var context = CreateContext();
        var mockUser = CreateCurrentUserMock(userId);

        var user = new User("test@test.com", "hash", "Test", "User");
        var cart = new Cart(userId);

        context.Users.Add(user);
        context.Carts.Add(cart);
        await context.SaveChangesAsync();

        var handler = new CreateOrderHandler(context, mockUser.Object);
        var command = new CreateOrderCommand
        {
            Street = "123 Main St",
            City = "Test City",
            State = "TS",
            Country = "Testland",
            ZipCode = "12345"
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BadRequestException>(
            () => handler.Handle(command, CancellationToken.None));
        Assert.Equal("Cart is empty.", exception.Message);
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFoundException_WhenProductInCartDoesNotExist()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var context = CreateContext();
        var mockUser = CreateCurrentUserMock(userId);

        var nonExistentProductId = Guid.NewGuid();
        var user = new User("test@test.com", "hash", "Test", "User");

        var cart = new Cart(userId);
        var cartItem = new CartItem(nonExistentProductId, "Ghost Product", 10m, 2);
        cart.AddItem(cartItem);

        context.Users.Add(user);
        context.Carts.Add(cart);
        await context.SaveChangesAsync();

        var handler = new CreateOrderHandler(context, mockUser.Object);
        var command = new CreateOrderCommand
        {
            Street = "123 Main St",
            City = "Test City",
            State = "TS",
            Country = "Testland",
            ZipCode = "12345"
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => handler.Handle(command, CancellationToken.None));
        Assert.Contains(nonExistentProductId.ToString(), exception.Message);
    }

    [Fact]
    public async Task Handle_ShouldThrowBadRequestException_WhenInsufficientStock()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var context = CreateContext();
        var mockUser = CreateCurrentUserMock(userId);

        var category = new Category("Test Category", "Description");
        var product = new Product("Test Product", "Description", new Money(10m), 1, "url", category.Id);
        var user = new User("test@test.com", "hash", "Test", "User");

        var cart = new Cart(userId);
        var cartItem = new CartItem(product.Id, product.Name, 10m, 5);
        cart.AddItem(cartItem);

        context.Categories.Add(category);
        context.Products.Add(product);
        context.Users.Add(user);
        context.Carts.Add(cart);
        await context.SaveChangesAsync();

        var handler = new CreateOrderHandler(context, mockUser.Object);
        var command = new CreateOrderCommand
        {
            Street = "123 Main St",
            City = "Test City",
            State = "TS",
            Country = "Testland",
            ZipCode = "12345"
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BadRequestException>(
            () => handler.Handle(command, CancellationToken.None));
        Assert.Contains("Insufficient stock", exception.Message);
    }

    [Fact]
    public async Task Handle_ShouldClearCart_AfterOrderCreation()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var context = CreateContext();
        var mockUser = CreateCurrentUserMock(userId);

        var category = new Category("Test Category", "Description");
        var product = new Product("Test Product", "Description", new Money(10m), 10, "url", category.Id);
        var user = new User("test@test.com", "hash", "Test", "User");

        var cart = new Cart(userId);
        var cartItem = new CartItem(product.Id, product.Name, 10m, 2);
        cart.AddItem(cartItem);

        context.Categories.Add(category);
        context.Products.Add(product);
        context.Users.Add(user);
        context.Carts.Add(cart);
        await context.SaveChangesAsync();

        var handler = new CreateOrderHandler(context, mockUser.Object);
        var command = new CreateOrderCommand
        {
            Street = "123 Main St",
            City = "Test City",
            State = "TS",
            Country = "Testland",
            ZipCode = "12345"
        };

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var cartAfter = await context.Carts
            .Include(c => c.CartItems)
            .FirstOrDefaultAsync(c => c.UserId == userId);
        Assert.NotNull(cartAfter);
        Assert.Empty(cartAfter.CartItems);
    }

    [Fact]
    public async Task Handle_ShouldReduceProductStock_AfterOrderCreation()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var context = CreateContext();
        var mockUser = CreateCurrentUserMock(userId);

        var category = new Category("Test Category", "Description");
        var product = new Product("Test Product", "Description", new Money(10m), 10, "url", category.Id);
        var user = new User("test@test.com", "hash", "Test", "User");

        var cart = new Cart(userId);
        var cartItem = new CartItem(product.Id, product.Name, 10m, 3);
        cart.AddItem(cartItem);

        context.Categories.Add(category);
        context.Products.Add(product);
        context.Users.Add(user);
        context.Carts.Add(cart);
        await context.SaveChangesAsync();

        var originalStock = product.Stock;

        var handler = new CreateOrderHandler(context, mockUser.Object);
        var command = new CreateOrderCommand
        {
            Street = "123 Main St",
            City = "Test City",
            State = "TS",
            Country = "Testland",
            ZipCode = "12345"
        };

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var productAfter = await context.Products.FindAsync(product.Id);
        Assert.NotNull(productAfter);
        Assert.Equal(originalStock - 3, productAfter.Stock);
    }

    public class TestDbContext : DbContext, IApplicationDbContext
    {
        public DbSet<Product> Products => Set<Product>();
        public DbSet<Category> Categories => Set<Category>();
        public DbSet<Domain.Entities.Order> Orders => Set<Domain.Entities.Order>();
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
            builder.Entity<Domain.Entities.Order>().OwnsOne(o => o.ShippingAddress);
            builder.Entity<Domain.Entities.Order>().HasMany(o => o.OrderItems).WithOne(oi => oi.Order).HasForeignKey(oi => oi.OrderId);
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
}
