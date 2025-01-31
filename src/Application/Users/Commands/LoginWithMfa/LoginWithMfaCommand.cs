using Application.Abstractions.Messaging;

namespace Application.Users.Commands.LoginWithMfa;

public sealed record LoginWithMfaCommand(
    string Email,
    string Password,
    string RecoveryCode) : ICommand<string>;