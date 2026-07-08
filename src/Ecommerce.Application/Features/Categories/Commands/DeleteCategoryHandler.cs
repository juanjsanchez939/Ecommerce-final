using Ecommerce.Application.Common.Exceptions;
using Ecommerce.Application.Common.Interfaces;
using Ecommerce.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Application.Features.Categories.Commands;

public class DeleteCategoryHandler : IRequestHandler<DeleteCategoryCommand>
{
    private readonly IApplicationDbContext _context;

    public DeleteCategoryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await _context.Categories
            .FindAsync(new object[] { request.Id }, cancellationToken);

        if (category is null)
            throw new NotFoundException(nameof(Category), request.Id);

        var hasProducts = await _context.Products
            .AnyAsync(p => p.CategoryId == request.Id, cancellationToken);

        if (hasProducts)
            throw new BadRequestException("Cannot delete category with existing products.");

        _context.Categories.Remove(category);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
