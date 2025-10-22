using Application.Abstractions.Messaging;

namespace Application.Users.Commands.MfaSetupWizard;

public sealed record MfaSetupWizardCommand(
    string Email,
    string Password
) : ICommand<MfaSetupWizardResult>;
