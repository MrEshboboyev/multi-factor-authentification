using FluentValidation;

namespace Application.Users.Commands.ValidateTotp;

internal sealed class ValidateTotpCommandValidator : AbstractValidator<ValidateTotpCommand>
{
    public ValidateTotpCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty();
        RuleFor(x => x.TotpCode).NotEmpty().Length(6);
    }
}