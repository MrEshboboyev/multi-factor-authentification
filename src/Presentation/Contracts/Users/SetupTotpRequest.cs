namespace Presentation.Contracts.Users;

public sealed record SetupTotpRequest(
    string Email,
    string Password);