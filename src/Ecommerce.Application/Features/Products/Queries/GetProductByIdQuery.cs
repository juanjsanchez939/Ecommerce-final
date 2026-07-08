using MediatR;

namespace Ecommerce.Application.Features.Products.Queries;

public record GetProductByIdQuery(Guid Id) : IRequest<ProductDetailDto>;

public class ProductDetailDto
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
    public DateTime? UpdatedAt { get; set; }
}
