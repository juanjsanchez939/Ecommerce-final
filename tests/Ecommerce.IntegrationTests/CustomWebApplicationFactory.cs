using Ecommerce.Api;
using Ecommerce.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Ecommerce.IntegrationTests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _dbName = Guid.NewGuid().ToString();

    public CustomWebApplicationFactory()
    {
        // Program.Main reads JwtSettings before the factory's in-memory config is applied, so the test values go through env vars.
        Environment.SetEnvironmentVariable("JwtSettings__SecretKey", "Integration-Test-Key-Minimum-32-Chars!!");
        Environment.SetEnvironmentVariable("JwtSettings__Issuer", "Ecommerce.Api");
        Environment.SetEnvironmentVariable("JwtSettings__Audience", "Ecommerce.Client");
        Environment.SetEnvironmentVariable("JwtSettings__ExpirationMinutes", "60");
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration(config =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["JwtSettings:SecretKey"] = "Integration-Test-Key-Minimum-32-Chars!!",
                ["JwtSettings:Issuer"] = "Ecommerce.Api",
                ["JwtSettings:Audience"] = "Ecommerce.Client",
                ["JwtSettings:ExpirationMinutes"] = "60"
            });
        });

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<ApplicationDbContext>>();
            services.RemoveAll<ApplicationDbContext>();

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseInMemoryDatabase(_dbName));

            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            db.Database.EnsureCreated();
        });
    }
}
