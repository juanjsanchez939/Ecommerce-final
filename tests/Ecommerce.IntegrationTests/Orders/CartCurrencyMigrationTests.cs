using Ecommerce.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Ecommerce.IntegrationTests.Orders;

public class CartCurrencyMigrationTests
{
    private const string BeforeCurrency = "20260703202237_UpdateUserRole";
    private const string AfterCurrency = "20261114053431_AddCartItemCurrency";

    [Theory]
    [InlineData(BeforeCurrency)]
    [InlineData(AfterCurrency)]
    public async Task Upgrade_BackfillsMissingCurrencyAndPreservesExistingCurrency(string previousMigration)
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = CreateContext(connection);
        await db.GetService<IMigrator>().MigrateAsync(previousMigration);
        await InsertCartAndProduct(db);

        if (previousMigration == BeforeCurrency)
        {
            await db.Database.ExecuteSqlRawAsync("""
                INSERT INTO "CartItems" ("Id", "CartId", "ProductId", "ProductName", "Quantity", "UnitPrice")
                VALUES ('item1', 'cart1', 'product1', 'Product', 1, '10');
                """);
        }
        else
        {
            await db.Database.ExecuteSqlRawAsync("""
                INSERT INTO "CartItems" ("Id", "CartId", "ProductId", "ProductName", "Quantity", "UnitPrice", "Currency")
                VALUES ('item1', 'cart1', 'product1', 'Product', 1, '10', ''),
                       ('item2', 'cart1', 'product1', 'Product', 1, '10', 'GBP');
                """);
        }

        await db.Database.MigrateAsync();

        Assert.Equal("EUR", await ReadCurrency(connection, "item1"));
        if (previousMigration == AfterCurrency)
            Assert.Equal("GBP", await ReadCurrency(connection, "item2"));
    }

    [Fact]
    public async Task Upgrade_WithMissingProduct_RollsBackInsteadOfGuessingCurrency()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = CreateContext(connection);
        await db.GetService<IMigrator>().MigrateAsync(AfterCurrency);
        await InsertCartAndProduct(db);
        await db.Database.ExecuteSqlRawAsync("""
            INSERT INTO "CartItems" ("Id", "CartId", "ProductId", "ProductName", "Quantity", "UnitPrice", "Currency")
            VALUES ('item1', 'cart1', 'product1', 'Product', 1, '10', ''),
                   ('item2', 'cart1', 'missing', 'Missing', 1, '10', '');
            """);

        var exception = await Assert.ThrowsAsync<SqliteException>(() => db.Database.MigrateAsync());

        Assert.Contains("Cart_item_currency_cannot_be_inferred_from_product", exception.Message);
        Assert.Equal("", await ReadCurrency(connection, "item1"));
        Assert.Equal("", await ReadCurrency(connection, "item2"));
        Assert.DoesNotContain("20261114170000_BackfillCartItemCurrency", await db.Database.GetAppliedMigrationsAsync());
    }

    private static ApplicationDbContext CreateContext(SqliteConnection connection) =>
        new(new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlite(connection).Options);

    private static async Task InsertCartAndProduct(ApplicationDbContext db)
    {
        await db.Database.ExecuteSqlRawAsync("""
            INSERT INTO "Users" ("Id", "Email", "PasswordHash", "FirstName", "LastName", "Role", "CreatedAt")
            VALUES ('user1', 'test@example.test', 'hash', 'Test', 'User', 'Customer', '2026-01-01');
            INSERT INTO "Carts" ("Id", "UserId", "CreatedAt") VALUES ('cart1', 'user1', '2026-01-01');
            INSERT INTO "Categories" ("Id", "Name", "Description", "CreatedAt")
            VALUES ('category1', 'Category', 'Description', '2026-01-01');
            INSERT INTO "Products" ("Id", "Name", "Description", "Price", "Currency", "Stock", "ImageUrl", "IsActive", "CreatedAt", "CategoryId")
            VALUES ('product1', 'Product', 'Description', '10', 'EUR', 10, 'image', 1, '2026-01-01', 'category1');
            """);
    }

    private static async Task<string> ReadCurrency(SqliteConnection connection, string id)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT \"Currency\" FROM \"CartItems\" WHERE \"Id\" = $id";
        command.Parameters.AddWithValue("$id", id);
        return (string)(await command.ExecuteScalarAsync())!;
    }
}
