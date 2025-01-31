using Domain.Errors;
using Domain.Events;
using Domain.Primitives;
using Domain.Shared;
using Domain.ValueObjects;

namespace Domain.Entities;

public sealed class User : AggregateRoot, IAuditableEntity
{
    #region Constructors 
    
    private User(
        Guid id,
        Email email,
        string passwordHash,
        FullName fullName): base(id)
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
    
    public Result<string> EnableMfa(string password)
    {
        #region Checking MFA is not enabled
        
        if (IsMfaEnabled)
            return Result.Failure<string>(
                DomainErrors.User.MfaAlreadyEnabled);
        
        #endregion
        
        #region Generate Recovery Code
        
        var rawRecoveryCode = GenerateRecoveryCode();
        
        #endregion
        
        #region Update fields

        RecoveryCode = RecoveryCode.Create(rawRecoveryCode);
        IsMfaEnabled = true;
        
        #endregion

        #region Domain Events
        
        RaiseDomainEvent(new MfaEnabledDomainEvent(
            Guid.NewGuid(),
            Id,
            Email));
        
        #endregion

        return Result.Success(rawRecoveryCode);
    }
    
    public Result DisableMfa(string recoveryCode)
    {
        #region Checking MFA is enabled
        
        if (!IsMfaEnabled)
            return Result.Failure(
                DomainErrors.User.MfaNotEnabled);
        
        #endregion
        
        #region Checking Recovery Code

        if (!RecoveryCode.Verify(recoveryCode))
            return Result.Failure(
                DomainErrors.User.InvalidRecoveryCode);
        
        #endregion
        
        #region Update fields

        IsMfaEnabled = false;
        RecoveryCode = null;
        
        #endregion

        #region Domain Events
        
        RaiseDomainEvent(new MfaDisabledDomainEvent(
            Guid.NewGuid(),
            Id,
            Email));
        
        #endregion
        
        return Result.Success();
    }

    public bool CanLoginWithMfa(string recoveryCode)
    {
        return IsMfaEnabled && RecoveryCode?.Verify(recoveryCode) == true;
    }

    private string GenerateRecoveryCode()
    {
        return Guid.NewGuid().ToString("N")[..10]; // 10-char random code
    }
    
    #endregion
    
    #endregion
}