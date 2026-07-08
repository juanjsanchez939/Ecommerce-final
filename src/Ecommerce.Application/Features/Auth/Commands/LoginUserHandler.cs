using Ecommerce.Application.Common.DTOs;
using Ecommerce.Application.Common.Exceptions;
using Ecommerce.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Application.Features.Auth.Commands;

public class LoginUserHandler : IRequestHandler<LoginUserCommand, AuthResultDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ITokenService _tokenService;

    public LoginUserHandler(IApplicationDbContext context, ITokenService tokenService)
    {
        _context = context;
        _tokenService = tokenService;
    }

    public async Task<AuthResultDto> Handle(LoginUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);

        if (user is null)
            throw new BadRequestException("Invalid email or password.");

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            throw new BadRequestException("Invalid email or password.");

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
