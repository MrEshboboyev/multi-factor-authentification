using FluentValidation;

namespace Application.Users.Commands.ValidateBackupCode;

internal sealed class ValidateBackupCodeCommandValidator : AbstractValidator<ValidateBackupCodeCommand>
{
    public ValidateBackupCodeCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty();
        RuleFor(x => x.BackupCode).NotEmpty().Matches(@"^[A-Z0-9]{4}-[A-Z0-9]{4}$");
    }
}