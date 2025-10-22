using Application.Abstractions.Messaging;

namespace Application.Users.Commands.ManageTrustedDevice;

public sealed record ManageTrustedDeviceCommand(
    string Email,
    string Password,
    string DeviceId,
    bool TrustDevice
) : ICommand;
