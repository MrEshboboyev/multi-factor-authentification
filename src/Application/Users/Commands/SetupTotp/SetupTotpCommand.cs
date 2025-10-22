using Application.Abstractions.Messaging;

namespace Application.Users.Commands.SetupTotp;

public sealed record SetupTotpCommand(
    string Email,
    string Password) : ICommand<string>;