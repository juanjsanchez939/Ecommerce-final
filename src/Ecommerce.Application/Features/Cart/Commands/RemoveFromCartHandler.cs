using Ecommerce.Application.Common.Exceptions;
using Ecommerce.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using DomainEntities = Ecommerce.Domain.Entities;

namespace Ecommerce.Application.Features.Cart.Commands;

public class RemoveFromCartHandler : IRequestHandler<RemoveFromCartCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public RemoveFromCartHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task Handle(RemoveFromCartCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || string.IsNullOrEmpty(_currentUser.UserId))
            throw new BadRequestException("User not authenticated.");

        if (!Guid.TryParse(_currentUser.UserId, out var userId))
            throw new BadRequestException("Authenticated user id is invalid.");

        var cart = await _context.Carts
            .Include(c => c.CartItems)
            .FirstOrDefaultAsync(c => c.UserId == userId, cancellationToken);

        if (cart is null)
            throw new NotFoundException(nameof(DomainEntities.Cart), userId);

        cart.RemoveItem(request.ProductId);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
