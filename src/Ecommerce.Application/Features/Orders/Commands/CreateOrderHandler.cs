using Ecommerce.Application.Common.Exceptions;
using Ecommerce.Application.Common.Interfaces;
using Ecommerce.Domain.Entities;
using Ecommerce.Domain.Enums;
using Ecommerce.Domain.ValueObjects;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Application.Features.Orders.Commands;

public class CreateOrderHandler : IRequestHandler<CreateOrderCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IPaymentClient _paymentClient;

    public CreateOrderHandler(IApplicationDbContext context, ICurrentUserService currentUser, IPaymentClient paymentClient)
    {
        _context = context;
        _currentUser = currentUser;
        _paymentClient = paymentClient;
    }

    public async Task<Guid> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || string.IsNullOrEmpty(_currentUser.UserId))
            throw new BadRequestException("User not authenticated.");

        if (!Guid.TryParse(_currentUser.UserId, out var userId))
            throw new BadRequestException("Authenticated user id is invalid.");

        var cart = await _context.Carts
            .Include(c => c.CartItems)
            .FirstOrDefaultAsync(c => c.UserId == userId, cancellationToken);

        if (cart is null || !cart.CartItems.Any())
            throw new BadRequestException("Cart is empty.");

        var shippingAddress = new Address(
            request.Street,
            request.City,
            request.State,
            request.Country,
            request.ZipCode);

        var order = new Order(userId, shippingAddress);

        using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            foreach (var cartItem in cart.CartItems)
            {
                var product = await _context.Products
                    .FirstOrDefaultAsync(p => p.Id == cartItem.ProductId, cancellationToken);

                if (product is null)
                    throw new NotFoundException(nameof(Product), cartItem.ProductId);

                if (cartItem.Quantity <= 0)
                    throw new BadRequestException("Cart item quantity must be positive.");

                if (product.Stock < cartItem.Quantity)
                    throw new BadRequestException($"Insufficient stock for product: {product.Name}");

                product.ReduceStock(cartItem.Quantity);

                var orderItem = new OrderItem(
                    product.Id,
                    product.Name,
                    new Money(cartItem.UnitPrice),
                    cartItem.Quantity);

                order.AddItem(orderItem);
            }

            await _context.Orders.AddAsync(order, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }

        return await ChargeOrderAsync(order, cart, cancellationToken);
    }

    private async Task<Guid> ChargeOrderAsync(Order order, Ecommerce.Domain.Entities.Cart cart, CancellationToken cancellationToken)
    {
        // The payment call runs outside any open transaction; the order is already saved as Pending.
        PaymentResult payment;
        try
        {
            payment = await _paymentClient.ProcessPaymentAsync(order.Id, order.TotalAmount, cancellationToken);
        }
        catch (PaymentServiceUnavailableException ex)
        {
            throw new PaymentServiceUnavailableException(
                $"Payment result is unknown for order {order.Id}. The order remains Pending with stock reserved; do not retry before reconciliation.", ex);
        }

        if (!payment.Approved)
        {
            await ReleaseOrderAsync(order, OrderStatus.PaymentRejected, cancellationToken);
            throw new BadRequestException("Payment was rejected.");
        }

        order.UpdateStatus(OrderStatus.Paid);
        cart.Clear();
        await _context.SaveChangesAsync(cancellationToken);

        return order.Id;
    }

    private async Task ReleaseOrderAsync(Order order, OrderStatus status, CancellationToken cancellationToken)
    {
        using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            foreach (var item in order.OrderItems)
            {
                var product = await _context.Products
                    .FirstAsync(p => p.Id == item.ProductId, cancellationToken);

                product.IncreaseStock(item.Quantity);
            }

            order.UpdateStatus(status);
            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}
