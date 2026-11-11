using Ecommerce.Application.Features.Orders.Queries;
using Ecommerce.Application.Features.Products.Queries;

namespace Ecommerce.Application.Tests;

public class PaginationValidatorTests
{
    [Fact]
    public void ProductPagination_ShouldRejectOutOfRangeValues()
    {
        var result = new GetProductsValidator().Validate(new GetProductsQuery
        {
            PageNumber = 0,
            PageSize = 101
        });

        Assert.False(result.IsValid);
    }

    [Fact]
    public void OrderPagination_ShouldAcceptConfiguredBounds()
    {
        var result = new GetOrdersValidator().Validate(new GetOrdersQuery
        {
            PageNumber = 10_000,
            PageSize = 100
        });

        Assert.True(result.IsValid);
    }
}
