using Domain.Shared;

namespace Domain.Errors;

public static class DomainErrors
{
    #region User
    
    #region Entities
    
    public static class User
    {
        public static readonly Error EmailAlreadyInUse = new(
            "User.EmailAlreadyInUse",
            "The specified email is already in use");

        public static readonly Func<Guid, Error> NotFound = id => new Error(
            "User.NotFound",
            $"The user with the identifier {id} was not found.");

        public static readonly Error NotExist = new(
            "Users.NotExist",
            $"There is no users");

        public static readonly Error InvalidCredentials = new(
            "User.InvalidCredentials",
            "The provided credentials are invalid");
        
        public static readonly Error MfaAlreadyEnabled = new(
            "User.MfaAlreadyEnabled",
            "MFA is already enabled.");
        
        public static readonly Error MfaNotEnabled = new(
            "User.MfaNotEnabled",
            "MFA is not enabled.");
        
        public static readonly Error InvalidRecoveryCode = new(
            "User.InvalidRecoveryCode",
            "Invalid recovery code.");
            
        public static readonly Error InvalidBackupCode = new(
            "User.InvalidBackupCode",
            "Invalid backup code.");
            
        public static readonly Error DeviceNotFound = new(
            "User.DeviceNotFound",
            "Device not found.");
            
        public static readonly Error MfaLocked = new(
            "User.MfaLocked",
            "MFA is temporarily locked due to too many failed attempts.");
            
        public static readonly Error MfaLoginRequired = new(
            "User.MfaLoginRequired",
            "MFA is enabled. Please use the LoginWithMfa endpoint.");
    }

    #endregion
    
    #region Value Objects
    
    public static class Email
    {
        public static readonly Error Empty = new(
            "Email.Empty",
            "Email is empty");
        public static readonly Error InvalidFormat = new(
            "Email.InvalidFormat",
            "Email format is invalid");
    }

    public static class FullName
    {
        public static readonly Error Empty = new(
            "FirstName.Empty",
            "First name is empty");
        public static readonly Error TooLong = new(
            "LastName.TooLong",
            "FirstName name is too long");
    }
    
    #endregion
    
    #endregion
}