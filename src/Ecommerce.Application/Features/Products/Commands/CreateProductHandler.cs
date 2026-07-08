using Ecommerce.Application.Common.Exceptions;
using Ecommerce.Application.Common.Interfaces;
using Ecommerce.Domain.Entities;
using Ecommerce.Domain.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Ecommerce.Application.Features.Products.Commands;

public class CreateProductHandler : IRequestHandler<CreateProductCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<CreateProductHandler> _logger;

    public CreateProductHandler(IApplicationDbContext context, ILogger<CreateProductHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Guid> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var categoryExists = await _context.Categories
            .FindAsync(new object[] { request.CategoryId }, cancellationToken);

        if (categoryExists is null)
            throw new NotFoundException(nameof(Category), request.CategoryId);

        var product = new Product(
            request.Name,
            request.Description,
            new Money(request.Price),
            request.Stock,
            request.ImageUrl,
            request.CategoryId);

        await _context.Products.AddAsync(product, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return product.Id;
    }
}
