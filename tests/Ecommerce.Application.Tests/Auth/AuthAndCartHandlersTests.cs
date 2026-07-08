using Ecommerce.Application.Common.Exceptions;
using Ecommerce.Application.Common.Interfaces;
using Ecommerce.Application.Features.Auth.Commands;
using Ecommerce.Application.Features.Cart.Commands;
using Ecommerce.Application.Features.Cart.Queries;
using Ecommerce.Domain.Entities;
using Ecommerce.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace Ecommerce.Application.Tests.Auth;

public class TestAuthDbContext : DbContext, IApplicationDbContext
{
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Cart> Carts => Set<Cart>();
    public DbSet<CartItem> CartItems => Set<CartItem>();

    public TestAuthDbContext(DbContextOptions<TestAuthDbContext> options) : base(options) { }

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

public class RegisterUserHandlerTests : IDisposable
{
    private readonly TestAuthDbContext _context;
    private readonly Mock<ITokenService> _tokenServiceMock;

    public RegisterUserHandlerTests()
    {
        var options = new DbContextOptionsBuilder<TestAuthDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new TestAuthDbContext(options);
        _tokenServiceMock = new Mock<ITokenService>();
        _tokenServiceMock.Setup(x => x.GenerateToken(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns("test-token");
    }

    public void Dispose() => _context.Dispose();

    [Fact]
    public async Task Handle_ShouldRegisterUserAndReturnToken()
    {
        var handler = new RegisterUserHandler(_context, _tokenServiceMock.Object);
        var command = new RegisterUserCommand
        {
            Email = "new@test.com",
            Password = "password123",
            FirstName = "John",
            LastName = "Doe"
        };

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("test-token", result.Token);
        Assert.Equal("new@test.com", result.Email);
        Assert.Equal("John Doe", result.FullName);

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == "new@test.com");
        Assert.NotNull(user);
    }

    [Fact]
    public async Task Handle_ShouldThrowBadRequestException_WhenEmailAlreadyExists()
    {
        var existingUser = new User("existing@test.com",
            BCrypt.Net.BCrypt.HashPassword("password"), "Existing", "User");
        await _context.Users.AddAsync(existingUser);
        await _context.SaveChangesAsync();

        var handler = new RegisterUserHandler(_context, _tokenServiceMock.Object);
        var command = new RegisterUserCommand
        {
            Email = "existing@test.com",
            Password = "password123",
            FirstName = "John",
            LastName = "Doe"
        };

        await Assert.ThrowsAsync<BadRequestException>(() => handler.Handle(command, CancellationToken.None));
    }
}

public class LoginUserHandlerTests : IDisposable
{
    private readonly TestAuthDbContext _context;
    private readonly Mock<ITokenService> _tokenServiceMock;

    public LoginUserHandlerTests()
    {
        var options = new DbContextOptionsBuilder<TestAuthDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new TestAuthDbContext(options);
        _tokenServiceMock = new Mock<ITokenService>();
        _tokenServiceMock.Setup(x => x.GenerateToken(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns("test-token");
    }

    public void Dispose() => _context.Dispose();

    private async Task SeedUserAsync(string email = "user@test.com", string password = "password123")
    {
        var user = new User(email, BCrypt.Net.BCrypt.HashPassword(password), "Test", "User");
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();
    }

    [Fact]
    public async Task Handle_ShouldLoginSuccessfully()
    {
        await SeedUserAsync();

        var handler = new LoginUserHandler(_context, _tokenServiceMock.Object);
        var command = new LoginUserCommand { Email = "user@test.com", Password = "password123" };

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("test-token", result.Token);
        Assert.Equal("user@test.com", result.Email);
    }

    [Fact]
    public async Task Handle_ShouldThrowBadRequestException_WhenEmailNotFound()
    {
        var handler = new LoginUserHandler(_context, _tokenServiceMock.Object);
        var command = new LoginUserCommand { Email = "nonexistent@test.com", Password = "password" };

        await Assert.ThrowsAsync<BadRequestException>(() => handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldThrowBadRequestException_WhenPasswordWrong()
    {
        await SeedUserAsync();

        var handler = new LoginUserHandler(_context, _tokenServiceMock.Object);
        var command = new LoginUserCommand { Email = "user@test.com", Password = "wrongpassword" };

        await Assert.ThrowsAsync<BadRequestException>(() => handler.Handle(command, CancellationToken.None));
    }
}

public class CartHandlersTests : IDisposable
{
    private readonly TestAuthDbContext _context;
    private readonly Mock<ICurrentUserService> _currentUserMock;
    private Guid _userId;

    public CartHandlersTests()
    {
        var options = new DbContextOptionsBuilder<TestAuthDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new TestAuthDbContext(options);
        _currentUserMock = new Mock<ICurrentUserService>();
        _userId = Guid.NewGuid();
        _currentUserMock.Setup(x => x.IsAuthenticated).Returns(true);
        _currentUserMock.Setup(x => x.UserId).Returns(_userId.ToString());
    }

    public void Dispose() => _context.Dispose();

    private async Task<Product> SeedProductAsync(string name = "Test Product", int stock = 50, decimal price = 99.99m)
    {
        var category = new Category("Test Category", "Test");
        await _context.Categories.AddAsync(category);
        await _context.SaveChangesAsync();

        var product = new Product(name, "A test product", new Money(price), stock, "https://img.jpg", category.Id);
        await _context.Products.AddAsync(product);
        await _context.SaveChangesAsync();
        return product;
    }

    [Fact]
    public async Task Handle_ShouldAddItemToCartSuccessfully()
    {
        var product = await SeedProductAsync();
        var handler = new AddToCartHandler(_context, _currentUserMock.Object);
        var command = new AddToCartCommand { ProductId = product.Id, Quantity = 2 };

        await handler.Handle(command, CancellationToken.None);

        var cart = await _context.Carts
            .Include(c => c.CartItems)
            .FirstOrDefaultAsync(c => c.UserId == _userId);

        Assert.NotNull(cart);
        Assert.Single(cart.CartItems);
        Assert.Equal(2, cart.CartItems.First().Quantity);
    }

    [Fact]
    public async Task Handle_ShouldThrowBadRequestException_WhenUserNotAuthenticated()
    {
        _currentUserMock.Setup(x => x.IsAuthenticated).Returns(false);
        _currentUserMock.Setup(x => x.UserId).Returns((string?)null);

        var handler = new AddToCartHandler(_context, _currentUserMock.Object);
        var command = new AddToCartCommand { ProductId = Guid.NewGuid(), Quantity = 1 };

        await Assert.ThrowsAsync<BadRequestException>(() => handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFoundException_WhenProductNotFound()
    {
        var handler = new AddToCartHandler(_context, _currentUserMock.Object);
        var command = new AddToCartCommand { ProductId = Guid.NewGuid(), Quantity = 1 };

        await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldAccumulateQuantity_WhenSameProductAddedTwice()
    {
        var product = await SeedProductAsync();
        var handler = new AddToCartHandler(_context, _currentUserMock.Object);

        await handler.Handle(new AddToCartCommand { ProductId = product.Id, Quantity = 2 }, CancellationToken.None);
        await handler.Handle(new AddToCartCommand { ProductId = product.Id, Quantity = 3 }, CancellationToken.None);

        var cart = await _context.Carts
            .Include(c => c.CartItems)
            .FirstOrDefaultAsync(c => c.UserId == _userId);

        Assert.Single(cart!.CartItems);
        Assert.Equal(5, cart.CartItems.First().Quantity);
    }

    [Fact]
    public async Task Handle_ShouldThrowBadRequestException_WhenInsufficientStock()
    {
        var product = await SeedProductAsync(stock: 1);
        var handler = new AddToCartHandler(_context, _currentUserMock.Object);
        var command = new AddToCartCommand { ProductId = product.Id, Quantity = 5 };

        await Assert.ThrowsAsync<BadRequestException>(() => handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldRemoveItemFromCart()
    {
        var product = await SeedProductAsync();
        var cart = new Cart(_userId);
        cart.AddItem(new CartItem(product.Id, product.Name, product.Price.Amount, 3));
        await _context.Carts.AddAsync(cart);
        await _context.SaveChangesAsync();

        var handler = new RemoveFromCartHandler(_context, _currentUserMock.Object);
        await handler.Handle(new RemoveFromCartCommand(product.Id), CancellationToken.None);

        var updatedCart = await _context.Carts
            .Include(c => c.CartItems)
            .FirstOrDefaultAsync(c => c.UserId == _userId);

        Assert.NotNull(updatedCart);
        Assert.Empty(updatedCart.CartItems);
    }

    [Fact]
    public async Task GetCart_ShouldReturnEmptyCart_WhenNotAuthenticated()
    {
        _currentUserMock.Setup(x => x.IsAuthenticated).Returns(false);
        _currentUserMock.Setup(x => x.UserId).Returns((string?)null);

        var handler = new GetCartHandler(_context, _currentUserMock.Object);
        var result = await handler.Handle(new GetCartQuery(), CancellationToken.None);

        Assert.NotNull(result);
        Assert.Empty(result.Items);
        Assert.Equal(0, result.Total);
    }

    [Fact]
    public async Task GetCart_ShouldReturnCartWithItems()
    {
        var product = await SeedProductAsync();
        var cart = new Cart(_userId);
        cart.AddItem(new CartItem(product.Id, product.Name, product.Price.Amount, 2));
        await _context.Carts.AddAsync(cart);
        await _context.SaveChangesAsync();

        var handler = new GetCartHandler(_context, _currentUserMock.Object);
        var result = await handler.Handle(new GetCartQuery(), CancellationToken.None);

        Assert.Single(result.Items);
        Assert.Equal(2, result.Items[0].Quantity);
        Assert.Equal(product.Price.Amount * 2, result.Total);
    }
}
