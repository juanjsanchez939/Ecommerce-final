using Ecommerce.Domain.Entities;
using Ecommerce.Domain.Enums;

namespace Ecommerce.Infrastructure.Persistence;

public static class DbInitializer
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        if (context.Users.Any())
            return;

        var adminUser = new User(
            "admin@test.com",
            BCrypt.Net.BCrypt.HashPassword("123456"),
            "Admin",
            "User",
            UserRole.Admin);

        context.Users.Add(adminUser);

        var adminCart = new Cart(adminUser.Id);
        context.Carts.Add(adminCart);

        var electronics = new Category("Electronics", "Electronic devices and accessories");
        var clothing = new Category("Clothing", "Men and women clothing");
        var books = new Category("Books", "Books and literature");

        context.Categories.AddRange(electronics, clothing, books);
        await context.SaveChangesAsync();

        var product1 = new Product(
            "Wireless Headphones",
            "Bluetooth 5.0 noise-cancelling headphones",
            new Domain.ValueObjects.Money(149.99m),
            50,
            "https://placehold.co/400?text=Headphones",
            electronics.Id);

        var product2 = new Product(
            "Laptop Pro 15",
            "15-inch laptop with 16GB RAM and 512GB SSD",
            new Domain.ValueObjects.Money(1299.99m),
            20,
            "https://placehold.co/400?text=Laptop",
            electronics.Id);

        var product3 = new Product(
            "Cotton T-Shirt",
            "Comfortable 100% cotton t-shirt",
            new Domain.ValueObjects.Money(29.99m),
            200,
            "https://placehold.co/400?text=TShirt",
            clothing.Id);

        var product4 = new Product(
            "Clean Code",
            "A handbook of agile software craftsmanship by Robert C. Martin",
            new Domain.ValueObjects.Money(39.99m),
            100,
            "https://placehold.co/400?text=CleanCode",
            books.Id);

        context.Products.AddRange(product1, product2, product3, product4);
        await context.SaveChangesAsync();
    }
}
