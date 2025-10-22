using Domain.ValueObjects;

namespace Application.Abstractions.Security;

public interface IDeviceManager
{
    string GenerateDeviceId();
    Device CreateDevice(string deviceId, string deviceName, string ipAddress);
    bool IsDeviceTrusted(string userId, string deviceId);
}
