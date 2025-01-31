using FluentValidation;

namespace Application.Users.Commands.LoginWithMfa;

internal sealed class LoginWithMfaCommandValidator : AbstractValidator<LoginWithMfaCommand>
{
    public LoginWithMfaCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty();
        RuleFor(x => x.RecoveryCode).NotEmpty();
    }
}