namespace Presentation.Contracts.Users;

public sealed record EnableMfaRequest(
    string Email,
    string Password);