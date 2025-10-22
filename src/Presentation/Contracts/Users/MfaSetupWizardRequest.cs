namespace Presentation.Contracts.Users;

public sealed record MfaSetupWizardRequest(
    string Email,
    string Password);