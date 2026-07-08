using MediatR;

namespace Ecommerce.Application.Features.Categories.Queries;

public record GetCategoriesQuery : IRequest<List<CategoryDto>>;

public class CategoryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public int ProductCount { get; set; }
}
