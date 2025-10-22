using Application.Abstractions.Messaging;

namespace Application.Users.Commands.ValidateBackupCode;

public sealed record ValidateBackupCodeCommand(
    string Email,
    string Password,
    string BackupCode) : ICommand<string>;