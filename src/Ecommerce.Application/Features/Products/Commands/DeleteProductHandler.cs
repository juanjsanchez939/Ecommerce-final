using Ecommerce.Application.Common.Exceptions;
using Ecommerce.Application.Common.Interfaces;
using Ecommerce.Domain.Entities;
using MediatR;

namespace Ecommerce.Application.Features.Products.Commands;

public class DeleteProductHandler : IRequestHandler<DeleteProductCommand>
{
    private readonly IApplicationDbContext _context;

    public DeleteProductHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _context.Products
            .FindAsync(new object[] { request.Id }, cancellationToken);

        if (product is null)
            throw new NotFoundException(nameof(Product), request.Id);

        product.Deactivate();
        await _context.SaveChangesAsync(cancellationToken);
    }
}
