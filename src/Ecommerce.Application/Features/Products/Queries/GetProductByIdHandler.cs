using Ecommerce.Application.Common.Exceptions;
using Ecommerce.Application.Common.Interfaces;
using Ecommerce.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Application.Features.Products.Queries;

public class GetProductByIdHandler : IRequestHandler<GetProductByIdQuery, ProductDetailDto>
{
    private readonly IApplicationDbContext _context;

    public GetProductByIdHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ProductDetailDto> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        var product = await _context.Products
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (product is null)
            throw new NotFoundException(nameof(Product), request.Id);

        return new ProductDetailDto
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price.Amount,
            Stock = product.Stock,
            ImageUrl = product.ImageUrl,
            IsActive = product.IsActive,
            CategoryId = product.CategoryId,
            CategoryName = product.Category.Name,
            CreatedAt = product.CreatedAt,
            UpdatedAt = product.UpdatedAt
        };
    }
}
