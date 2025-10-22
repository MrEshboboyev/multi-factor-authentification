namespace Presentation.Contracts.Users;

public sealed record ValidateTotpRequest(
    string Email,
    string Password,
    string TotpCode);