using FluentValidation;

namespace Application.Users.Commands.EnableMfa;

internal class EnableMfaCommandValidator : AbstractValidator<EnableMfaCommand>
{
    public EnableMfaCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email is required.")
            .EmailAddress()
            .WithMessage("Invalid email format.");

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("Password is required.")
            .NotNull()
            .WithMessage("Password cannot be null.");
    }
}