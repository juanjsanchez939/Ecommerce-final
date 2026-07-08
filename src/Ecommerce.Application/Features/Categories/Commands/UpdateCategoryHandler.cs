using Ecommerce.Application.Common.Exceptions;
using Ecommerce.Application.Common.Interfaces;
using Ecommerce.Domain.Entities;
using MediatR;

namespace Ecommerce.Application.Features.Categories.Commands;

public class UpdateCategoryHandler : IRequestHandler<UpdateCategoryCommand>
{
    private readonly IApplicationDbContext _context;

    public UpdateCategoryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await _context.Categories
            .FindAsync(new object[] { request.Id }, cancellationToken);

        if (category is null)
            throw new NotFoundException(nameof(Category), request.Id);

        category.Update(request.Name, request.Description);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
