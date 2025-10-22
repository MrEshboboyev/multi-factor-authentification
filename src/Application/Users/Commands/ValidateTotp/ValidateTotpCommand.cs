using Application.Abstractions.Messaging;

namespace Application.Users.Commands.ValidateTotp;

public sealed record ValidateTotpCommand(
    string Email,
    string Password,
    string TotpCode) : ICommand<string>;