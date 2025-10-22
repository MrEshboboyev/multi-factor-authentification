using Domain.Primitives;
using Domain.Shared;

namespace Domain.ValueObjects;

public sealed class TotpSecret : ValueObject
{
    #region Properties
    
    public string Value { get; }
    
    #endregion
    
    #region Constructors

    private TotpSecret(string value)
    {
        Value = value;
    }
    
    #endregion
    
    #region Factory methods

    public static Result<TotpSecret> Create(string secret)
    {
        // Validate the secret (should be base32 encoded)
        if (string.IsNullOrWhiteSpace(secret))
        {
            return Result.Failure<TotpSecret>(
                new Error("TotpSecret.Empty", "TOTP secret cannot be empty"));
        }

        return Result.Success(new TotpSecret(secret));
    }
    
    #endregion
    
    #region Overrides

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Value;
    }
    
    #endregion
}
