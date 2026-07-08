using Ecommerce.Application.Common.DTOs;
using Ecommerce.Application.Common.Exceptions;
using Ecommerce.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using DomainEntities = Ecommerce.Domain.Entities;

namespace Ecommerce.Application.Features.Auth.Commands;

public class RegisterUserHandler : IRequestHandler<RegisterUserCommand, AuthResultDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ITokenService _tokenService;

    public RegisterUserHandler(IApplicationDbContext context, ITokenService tokenService)
    {
        _context = context;
        _tokenService = tokenService;
    }

    public async Task<AuthResultDto> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        var existingUser = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);

        if (existingUser is not null)
            throw new BadRequestException("Email already registered.");

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

        var user = new DomainEntities.User(request.Email, passwordHash, request.FirstName, request.LastName);

        await _context.Users.AddAsync(user, cancellationToken);

        var cart = new DomainEntities.Cart(user.Id);
        await _context.Carts.AddAsync(cart, cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);

        var token = _tokenService.GenerateToken(user.Id.ToString(), user.Email, user.Role.ToString());

        return new AuthResultDto
        {
            Token = token,
            Email = user.Email,
            FullName = $"{user.FirstName} {user.LastName}",
            Role = user.Role.ToString()
        };
    }
}
