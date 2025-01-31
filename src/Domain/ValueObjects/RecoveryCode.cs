using System.Security.Cryptography;
using System.Text;
using Domain.Primitives;
using Domain.Shared;

namespace Domain.ValueObjects;

public sealed class RecoveryCode : ValueObject
{
    #region Private fields
    
    private readonly string _value;
    
    #endregion
    
    #region Constructors

    private RecoveryCode(string value)
    {
        _value = value;
    }
    
    #endregion
    
    #region Factory methods

    public static Result<RecoveryCode> Create(string rawCode)
    {
        return Result.Success(new RecoveryCode(rawCode));
    }
    
    #endregion
    
    #region Overrrides

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return _value;
    }
    
    #endregion
}