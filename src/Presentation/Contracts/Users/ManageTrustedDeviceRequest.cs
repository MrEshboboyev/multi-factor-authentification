namespace Presentation.Contracts.Users;

public sealed record ManageTrustedDeviceRequest(
    string Email,
    string Password,
    string DeviceId,
    bool TrustDevice);