using Domain.Primitives;
using Domain.Shared;

namespace Domain.ValueObjects;

public sealed class BackupCode : ValueObject
{
    #region Properties
    
    public string Value { get; }
    
    #endregion
    
    #region Constructors

    private BackupCode(string value)
    {
        Value = value;
    }
    
    #endregion
    
    #region Factory methods

    public static Result<BackupCode> Create(string code)
    {
        // Validate the backup code
        if (string.IsNullOrWhiteSpace(code))
        {
            return Result.Failure<BackupCode>(
                new Error("BackupCode.Empty", "Backup code cannot be empty"));
        }

        // Backup codes should be in format XXXX-XXXX
        if (!System.Text.RegularExpressions.Regex.IsMatch(code, @"^[A-Z0-9]{4}-[A-Z0-9]{4}$"))
        {
            return Result.Failure<BackupCode>(
                new Error("BackupCode.InvalidFormat", "Backup code must be in format XXXX-XXXX"));
        }

        return Result.Success(new BackupCode(code));
    }
    
    #endregion
    
    #region Overrides

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Value;
    }
    
    #endregion
}
