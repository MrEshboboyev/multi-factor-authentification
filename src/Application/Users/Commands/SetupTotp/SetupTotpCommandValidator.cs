using FluentValidation;

namespace Application.Users.Commands.SetupTotp;

internal sealed class SetupTotpCommandValidator : AbstractValidator<SetupTotpCommand>
{
    public SetupTotpCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty();
    }
}