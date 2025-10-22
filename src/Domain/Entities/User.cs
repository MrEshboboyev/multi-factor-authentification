using Domain.Errors;
using Domain.Events;
using Domain.Primitives;
using Domain.Shared;
using Domain.ValueObjects;
using System.Collections.Immutable;

namespace Domain.Entities;

public sealed class User : AggregateRoot, IAuditableEntity
{
    #region Constructors

    private User(
        Guid id,
        Email email,
        string passwordHash,
        FullName fullName) : base(id)
    {
        Email = email;
        PasswordHash = passwordHash;
        FullName = fullName;
    }

    #endregion

    #region Properties

    public string PasswordHash { get; set; }
    public FullName FullName { get; set; }
    public Email Email { get; set; }
    public DateTime CreatedOnUtc { get; set; }
    public DateTime? ModifiedOnUtc { get; set; }

    #region MFA related

    public bool IsMfaEnabled { get; private set; }
    public RecoveryCode? RecoveryCode { get; private set; }
    
    // New MFA properties
    public TotpSecret? TotpSecret { get; private set; }
    public ImmutableList<BackupCode> BackupCodes { get; private set; } = [];
    public ImmutableList<Device> TrustedDevices { get; private set; } = [];
    public int FailedMfaAttempts { get; private set; }
    public DateTime? MfaLockedUntil { get; private set; }

    #endregion

    #endregion

    #region Factory Methods

    /// <summary> 
    /// Creates a new user instance. 
    /// </summary>
    public static User Create(
        Guid id,
        Email email,
        string passwordHash,
        FullName fullName
    )
    {
        #region Create new User

        var user = new User(
            id,
            email,
            passwordHash,
            fullName);

        #endregion

        #region Domain Events

        user.RaiseDomainEvent(new UserRegisteredDomainEvent(
            Guid.NewGuid(),
            user.Id));

        #endregion

        return user;
    }

    #endregion

    #region Own Methods

    /// <summary> 
    /// Changes the user's name and raises a domain event if the name has changed. 
    /// </summary>
    public void ChangeName(
        FullName fullName)
    {
        #region Checking new values are equals old valus

        if (!FullName.Equals(fullName))
        {
            RaiseDomainEvent(new UserNameChangedDomainEvent(
                Guid.NewGuid(),
                Id));
        }

        #endregion

        #region Update fields

        FullName = fullName;

        #endregion
    }

    #region MFA related

    public Result EnableMfa(RecoveryCode recoveryCode)
    {
        #region Checking MFA is not enabled

        if (IsMfaEnabled)
            return Result.Failure<string>(
                DomainErrors.User.MfaAlreadyEnabled);

        #endregion

        #region Update fields

        RecoveryCode = recoveryCode;
        IsMfaEnabled = true;

        #endregion

        #region Domain Events

        RaiseDomainEvent(new MfaEnabledDomainEvent(
            Guid.NewGuid(),
            Id,
            Email));

        #endregion

        return Result.Success(recoveryCode);
    }

    public Result DisableMfa()
    {
        #region Checking MFA is enabled

        if (!IsMfaEnabled)
            return Result.Failure(
                DomainErrors.User.MfaNotEnabled);

        #endregion

        #region Update fields

        IsMfaEnabled = false;
        RecoveryCode = null;
        TotpSecret = null;
        BackupCodes = ImmutableList<BackupCode>.Empty;
        TrustedDevices = ImmutableList<Device>.Empty;
        FailedMfaAttempts = 0;
        MfaLockedUntil = null;

        #endregion

        #region Domain Events

        RaiseDomainEvent(new MfaDisabledDomainEvent(
            Guid.NewGuid(),
            Id,
            Email));

        #endregion

        return Result.Success();
    }

    public bool ValidateRecoveryCode(RecoveryCode recoveryCode)
        => RecoveryCode != null && RecoveryCode.Equals(recoveryCode);
        
    // New MFA methods
    
    public Result SetTotpSecret(TotpSecret totpSecret)
    {
        #region Checking MFA is enabled

        if (!IsMfaEnabled)
            return Result.Failure(
                DomainErrors.User.MfaNotEnabled);

        #endregion

        TotpSecret = totpSecret;
        return Result.Success();
    }
    
    public bool ValidateTotpCode(string totpCode)
    {
        if (TotpSecret == null || string.IsNullOrEmpty(totpCode))
            return false;
            
        // Implementation will be in the infrastructure layer
        return false;
    }
    
    public Result GenerateBackupCodes(int count = 10)
    {
        #region Checking MFA is enabled

        if (!IsMfaEnabled)
            return Result.Failure(
                DomainErrors.User.MfaNotEnabled);

        #endregion
        
        #region Generate backup codes

        var backupCodes = new List<BackupCode>();
        for (int i = 0; i < count; i++)
        {
            // Generate a backup code in format XXXX-XXXX
            var code = $"{GenerateRandomString(4)}-{GenerateRandomString(4)}";
            var backupCodeResult = BackupCode.Create(code);
            if (backupCodeResult.IsSuccess)
            {
                backupCodes.Add(backupCodeResult.Value);
            }
        }
        
        BackupCodes = ImmutableList.CreateRange(backupCodes);

        #endregion

        return Result.Success();
    }
    
    public bool ValidateBackupCode(BackupCode backupCode)
    {
        return BackupCodes.Contains(backupCode);
    }
    
    public Result UseBackupCode(BackupCode backupCode)
    {
        #region Checking MFA is enabled

        if (!IsMfaEnabled)
            return Result.Failure(
                DomainErrors.User.MfaNotEnabled);

        #endregion
        
        #region Validate backup code

        if (!ValidateBackupCode(backupCode))
        {
            IncrementFailedMfaAttempts();
            return Result.Failure(DomainErrors.User.InvalidBackupCode);
        }

        #endregion
        
        #region Remove used backup code

        BackupCodes = BackupCodes.Remove(backupCode);

        #endregion
        
        ResetFailedMfaAttempts();
        return Result.Success();
    }
    
    public Result AddTrustedDevice(Device device)
    {
        #region Checking MFA is enabled

        if (!IsMfaEnabled)
            return Result.Failure(
                DomainErrors.User.MfaNotEnabled);

        #endregion
        
        TrustedDevices = TrustedDevices.Add(device);
        return Result.Success();
    }
    
    public bool IsDeviceTrusted(Device device)
    {
        return TrustedDevices.Any(d => d.Id == device.Id && d.IsTrusted);
    }
    
    public Result TrustDevice(string deviceId)
    {
        #region Checking MFA is enabled

        if (!IsMfaEnabled)
            return Result.Failure(
                DomainErrors.User.MfaNotEnabled);

        #endregion
        
        var device = TrustedDevices.FirstOrDefault(d => d.Id == deviceId);
        if (device == null)
        {
            return Result.Failure(
                DomainErrors.User.DeviceNotFound);
        }
        
        var updatedDevice = device.SetTrusted(true);
        TrustedDevices = TrustedDevices.Replace(device, updatedDevice);
        return Result.Success();
    }
    
    public bool IsMfaLocked()
    {
        return MfaLockedUntil.HasValue && MfaLockedUntil > DateTime.UtcNow;
    }
    
    public void IncrementFailedMfaAttempts()
    {
        FailedMfaAttempts++;
        
        // Lock account after 5 failed attempts for 30 minutes
        if (FailedMfaAttempts >= 5)
        {
            MfaLockedUntil = DateTime.UtcNow.AddMinutes(30);
        }
    }
    
    public void ResetFailedMfaAttempts()
    {
        FailedMfaAttempts = 0;
        MfaLockedUntil = null;
    }
    
    #endregion

    #endregion
    
    #region Private Helpers
    
    private string GenerateRandomString(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var random = new Random();
        return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }
    
    #endregion
}
