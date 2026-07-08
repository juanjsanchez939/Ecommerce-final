using Ecommerce.Application.Common.Exceptions;
using Ecommerce.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using DomainEntities = Ecommerce.Domain.Entities;

namespace Ecommerce.Application.Features.Cart.Commands;

public class AddToCartHandler : IRequestHandler<AddToCartCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public AddToCartHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task Handle(AddToCartCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || string.IsNullOrEmpty(_currentUser.UserId))
            throw new BadRequestException("User not authenticated.");

        var userId = Guid.Parse(_currentUser.UserId);

        var product = await _context.Products
            .FirstOrDefaultAsync(p => p.Id == request.ProductId, cancellationToken);

        if (product is null)
            throw new NotFoundException(nameof(DomainEntities.Product), request.ProductId);

        if (!product.IsActive)
            throw new BadRequestException("Product is not available.");

        if (product.Stock < request.Quantity)
            throw new BadRequestException("Insufficient stock.");

        var cart = await _context.Carts
            .Include(c => c.CartItems)
            .FirstOrDefaultAsync(c => c.UserId == userId, cancellationToken);

        if (cart is null)
        {
            cart = new DomainEntities.Cart(userId);
            await _context.Carts.AddAsync(cart, cancellationToken);
        }

        var cartItem = new DomainEntities.CartItem(product.Id, product.Name, product.Price.Amount, request.Quantity, product.Price.Currency);
        cart.AddItem(cartItem);

        await _context.SaveChangesAsync(cancellationToken);
    }
}
