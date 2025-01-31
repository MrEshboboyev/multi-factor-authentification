using System.Security.Cryptography;
using System.Text;
using Domain.Primitives;
using Domain.Shared;

namespace Domain.ValueObjects;

public sealed class RecoveryCode : ValueObject
{
    #region Properties
    
    public string Value { get; }
    
    #endregion
    
    #region Constructors

    private RecoveryCode(string value)
    {
        Value = value;
    }
    
    #endregion
    
    #region Factory methods

    public static Result<RecoveryCode> Create(string rawCode)
    {
        return Result.Success(new RecoveryCode(rawCode));
    }
    
    #endregion
    
    #region Overrides

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Value;
    }
    
    #endregion
}