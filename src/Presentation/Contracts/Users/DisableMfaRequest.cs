namespace Presentation.Contracts.Users;

public sealed record DisableMfaRequest(
    string Email,
    string Password,
    string RecoveryCode);