using Application.Abstractions.Messaging;

namespace Application.Users.Commands.DisableMfa;

public sealed record DisableMfaCommand(
    string Email,
    string Password,
    string RecoveryCode) : ICommand;