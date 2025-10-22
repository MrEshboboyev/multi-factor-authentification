namespace Presentation.Contracts.Users;

public sealed record ValidateBackupCodeRequest(
    string Email,
    string Password,
    string BackupCode);