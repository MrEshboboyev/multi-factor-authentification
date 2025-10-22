using FluentValidation;

namespace Application.Users.Commands.GenerateBackupCodes;

internal sealed class GenerateBackupCodesCommandValidator : AbstractValidator<GenerateBackupCodesCommand>
{
    public GenerateBackupCodesCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty();
    }
}
