using FluentValidation;

namespace Application.Users.Commands.DisableMfa;

internal sealed class DisableMfaCommandValidator : AbstractValidator<DisableMfaCommand>
{
    public DisableMfaCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty();
        RuleFor(x => x.RecoveryCode).NotEmpty();
    }
}