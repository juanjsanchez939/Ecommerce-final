using Ecommerce.Application.Common.DTOs;
using FluentValidation;
using MediatR;

namespace Ecommerce.Application.Features.Auth.Commands;

public record LoginUserCommand : IRequest<AuthResultDto>
{
    public string Email { get; init; }
    public string Password { get; init; }
}

public class LoginUserValidator : AbstractValidator<LoginUserCommand>
{
    public LoginUserValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty();
    }
}
