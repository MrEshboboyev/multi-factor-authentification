using Domain.Primitives;
using Domain.Shared;

namespace Domain.ValueObjects;

public sealed class Device : ValueObject
{
    #region Properties
    
    public string Id { get; }
    public string Name { get; }
    public string IpAddress { get; }
    public DateTime FirstLogin { get; }
    public DateTime LastLogin { get; }
    public bool IsTrusted { get; }
    
    #endregion
    
    #region Constructors

    private Device(
        string id, 
        string name, 
        string ipAddress, 
        DateTime firstLogin, 
        DateTime lastLogin, 
        bool isTrusted)
    {
        Id = id;
        Name = name;
        IpAddress = ipAddress;
        FirstLogin = firstLogin;
        LastLogin = lastLogin;
        IsTrusted = isTrusted;
    }
    
    #endregion
    
    #region Factory methods

    public static Result<Device> Create(
        string id,
        string name,
        string ipAddress,
        DateTime firstLogin,
        DateTime lastLogin,
        bool isTrusted)
    {
        // Validate the device information
        if (string.IsNullOrWhiteSpace(id))
        {
            return Result.Failure<Device>(
                new Error("Device.IdEmpty", "Device ID cannot be empty"));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Failure<Device>(
                new Error("Device.NameEmpty", "Device name cannot be empty"));
        }

        if (string.IsNullOrWhiteSpace(ipAddress))
        {
            return Result.Failure<Device>(
                new Error("Device.IpAddressEmpty", "Device IP address cannot be empty"));
        }

        return Result.Success(new Device(id, name, ipAddress, firstLogin, lastLogin, isTrusted));
    }
    
    #endregion
    
    #region Public Methods
    
    public Device UpdateLastLogin(DateTime lastLogin)
    {
        return new Device(Id, Name, IpAddress, FirstLogin, lastLogin, IsTrusted);
    }
    
    public Device SetTrusted(bool isTrusted)
    {
        return new Device(Id, Name, IpAddress, FirstLogin, LastLogin, isTrusted);
    }
    
    #endregion
    
    #region Overrides

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Id;
        yield return Name;
        yield return IpAddress;
        yield return FirstLogin;
        yield return LastLogin;
        yield return IsTrusted;
    }
    
    #endregion
}
