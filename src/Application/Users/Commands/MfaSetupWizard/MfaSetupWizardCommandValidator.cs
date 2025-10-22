using FluentValidation;

namespace Application.Users.Commands.MfaSetupWizard;

internal sealed class MfaSetupWizardCommandValidator : AbstractValidator<MfaSetupWizardCommand>
{
    public MfaSetupWizardCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty();
    }
}