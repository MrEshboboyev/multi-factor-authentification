using Application.Abstractions.Security;
using Domain.ValueObjects;
using System.Security.Cryptography;

namespace Infrastructure.Authentication;

public class DeviceManager : IDeviceManager
{
    public string GenerateDeviceId()
    {
        // Generate a random device ID
        var bytes = new byte[16];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(bytes);
        }
        return Convert.ToBase64String(bytes);
    }

    public Device CreateDevice(string deviceId, string deviceName, string ipAddress)
    {
        var deviceResult = Device.Create(
            deviceId,
            deviceName,
            ipAddress,
            DateTime.UtcNow,
            DateTime.UtcNow,
            false);
            
        return deviceResult.IsSuccess ? deviceResult.Value : null;
    }

    public bool IsDeviceTrusted(string userId, string deviceId)
    {
        // In a real implementation, this would check a database or cache
        // For now, we'll return false to require MFA each time
        return false;
    }
}
