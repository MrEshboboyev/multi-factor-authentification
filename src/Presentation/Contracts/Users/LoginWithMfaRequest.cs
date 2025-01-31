namespace Presentation.Contracts.Users;

public sealed record LoginWithMfaRequest(
    string Email,
    string Password,
    string RecoveryCode);