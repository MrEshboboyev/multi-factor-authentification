using System.Security.Cryptography;
using System.Text;
using Domain.Primitives;

namespace Domain.ValueObjects;

public sealed class RecoveryCode : ValueObject
{
    #region Private fields
    
    private readonly string _hashedValue;
    
    #endregion
    
    #region Constructors

    private RecoveryCode(string hashedValue, bool isHashed)
    {
        _hashedValue = hashedValue;
    }
    
    #endregion
    
    #region Factory methods

    public static RecoveryCode Create(string rawCode)
    {
        return new RecoveryCode(Hash(rawCode), true);
    }
    
    #endregion
    
    #region Public methods

    public bool Verify(string rawCode)
    {
        return Hash(rawCode) == _hashedValue;
    }
    
    #endregion
    
    #region Private methods

    private static string Hash(string input)
    {
        return Convert.ToBase64String(
            SHA256.HashData(Encoding.UTF8.GetBytes(input)));
    }
    
    #endregion
    
    #region Overrrides

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return _hashedValue;
    }
    
    #endregion
}