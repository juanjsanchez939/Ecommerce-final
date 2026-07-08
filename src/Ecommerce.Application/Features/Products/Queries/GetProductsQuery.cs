using Ecommerce.Application.Common.DTOs;
using MediatR;

namespace Ecommerce.Application.Features.Products.Queries;

public record GetProductsQuery : IRequest<PagedResult<ProductDto>>
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public string? SearchTerm { get; init; }
    public Guid? CategoryId { get; init; }
    public bool? IsActive { get; init; }
}

public class ProductDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public string ImageUrl { get; set; }
    public bool IsActive { get; set; }
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; }
    public DateTime CreatedAt { get; set; }
}
