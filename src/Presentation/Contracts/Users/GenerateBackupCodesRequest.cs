namespace Presentation.Contracts.Users;

public sealed record GenerateBackupCodesRequest(
    string Email,
    string Password);