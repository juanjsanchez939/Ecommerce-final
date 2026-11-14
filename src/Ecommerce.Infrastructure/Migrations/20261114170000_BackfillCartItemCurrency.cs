using Ecommerce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ecommerce.Infrastructure.Migrations;

[DbContext(typeof(ApplicationDbContext))]
[Migration("20261114170000_BackfillCartItemCurrency")]
public class BackfillCartItemCurrency : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            UPDATE "CartItems"
            SET "Currency" = (
                SELECT "Products"."Currency"
                FROM "Products"
                WHERE "Products"."Id" = "CartItems"."ProductId"
            )
            WHERE TRIM("Currency") = ''
              AND EXISTS (
                  SELECT 1 FROM "Products"
                  WHERE "Products"."Id" = "CartItems"."ProductId"
                    AND TRIM("Products"."Currency") <> ''
              );

            CREATE TEMP TABLE "__CartCurrencyBackfillCheck" (
                "Resolved" INTEGER CONSTRAINT "Cart_item_currency_cannot_be_inferred_from_product" CHECK ("Resolved" = 1)
            );
            INSERT INTO "__CartCurrencyBackfillCheck" ("Resolved")
            SELECT CASE WHEN EXISTS (
                SELECT 1 FROM "CartItems" WHERE TRIM("Currency") = ''
            ) THEN 0 ELSE 1 END;
            DROP TABLE "__CartCurrencyBackfillCheck";
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // The original missing currencies cannot be distinguished from existing values.
    }
}
