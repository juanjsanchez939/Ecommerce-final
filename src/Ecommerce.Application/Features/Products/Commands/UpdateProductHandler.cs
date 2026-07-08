using Ecommerce.Application.Common.Exceptions;
using Ecommerce.Application.Common.Interfaces;
using Ecommerce.Domain.Entities;
using Ecommerce.Domain.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Ecommerce.Application.Features.Products.Commands;

public class UpdateProductHandler : IRequestHandler<UpdateProductCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<UpdateProductHandler> _logger;

    public UpdateProductHandler(IApplicationDbContext context, ILogger<UpdateProductHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _context.Products
            .FindAsync(new object[] { request.Id }, cancellationToken);

        if (product is null)
            throw new NotFoundException(nameof(Product), request.Id);

        var categoryExists = await _context.Categories
            .FindAsync(new object[] { request.CategoryId }, cancellationToken);

        if (categoryExists is null)
            throw new NotFoundException(nameof(Category), request.CategoryId);

        product.Update(
            request.Name,
            request.Description,
            new Money(request.Price),
            request.Stock,
            request.ImageUrl,
            request.CategoryId);

        await _context.SaveChangesAsync(cancellationToken);
    }
}
