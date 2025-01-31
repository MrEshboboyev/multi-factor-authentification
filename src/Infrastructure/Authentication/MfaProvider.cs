using System.Security.Cryptography;
using Application.Abstractions.Security;

namespace Infrastructure.Authentication;

public class MfaProvider : IMfaProvider
{
    #region Private fields
    
    private const int RecoveryCodeLength = 8; // Length of the recovery code
    private const string AllowedCharacters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789"; // Allowed characters in the recovery code
    
    #endregion

    public string GenerateRecoveryCode()
    {
        // Create a byte array to hold the random bytes
        var randomBytes = new byte[RecoveryCodeLength];

        // Fill the array with cryptographically secure random bytes
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomBytes);
        }

        // Convert the random bytes to a recovery code using the allowed characters
        var recoveryCode = new char[RecoveryCodeLength];
        for (var i = 0; i < RecoveryCodeLength; i++)
        {
            // Use the random byte to index into the allowed characters
            recoveryCode[i] = AllowedCharacters[randomBytes[i] % AllowedCharacters.Length];
        }

        return new string(recoveryCode);
    }
}