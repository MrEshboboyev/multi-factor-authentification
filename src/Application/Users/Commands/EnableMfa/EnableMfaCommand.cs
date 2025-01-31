using Application.Abstractions.Messaging;

namespace Application.Users.Commands.EnableMfa;

public sealed record EnableMfaCommand(
    string Email,
    string Password) : ICommand<string>;