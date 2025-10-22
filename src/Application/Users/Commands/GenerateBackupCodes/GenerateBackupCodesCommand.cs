using Application.Abstractions.Messaging;

namespace Application.Users.Commands.GenerateBackupCodes;

public sealed record GenerateBackupCodesCommand(
    string Email,
    string Password
) : ICommand<IEnumerable<string>>;
