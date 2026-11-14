using Ecommerce.Domain.Entities;
using Ecommerce.Domain.Enums;
using Ecommerce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Ecommerce.IntegrationTests.Auth;

public class AdminSeedTests
{
    [Fact]
    public async Task Seed_CreatesAdminWhenCustomersAlreadyExist_WithoutChangingThem()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        await using var db = new ApplicationDbContext(options);
        var customer = new User("customer@example.test", "existing-hash", "A", "B");
        db.Users.Add(customer);
        await db.SaveChangesAsync();
        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["AdminSeed:Email"] = "admin@example.test",
            ["AdminSeed:Password"] = "TestOnlyPassword!"
        }).Build();

        await DbInitializer.SeedAsync(db, config);
        await DbInitializer.SeedAsync(db, config);

        Assert.Equal(2, await db.Users.CountAsync());
        Assert.Equal(UserRole.Customer, customer.Role);
        Assert.Equal("existing-hash", customer.PasswordHash);
        Assert.Single(await db.Users.Where(u => u.Role == UserRole.Admin).ToListAsync());
        Assert.Empty(await db.Products.ToListAsync());
    }

    [Fact]
    public async Task Seed_RejectsEmailAlreadyUsedByCustomer()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        await using var db = new ApplicationDbContext(options);
        db.Users.Add(new User("taken@example.test", "hash", "A", "B"));
        await db.SaveChangesAsync();
        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["AdminSeed:Email"] = "taken@example.test",
            ["AdminSeed:Password"] = "TestOnlyPassword!"
        }).Build();

        await Assert.ThrowsAsync<InvalidOperationException>(() => DbInitializer.SeedAsync(db, config));
        Assert.Single(await db.Users.ToListAsync());
    }
}
