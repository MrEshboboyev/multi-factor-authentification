using System.Security.Cryptography;
using Application.Abstractions.Security;
using OtpNet;

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
    
    public string GenerateTotpSecret()
    {
        // Generate a 20-byte (160-bit) secret key
        var secretKey = KeyGeneration.GenerateRandomKey(20);
        
        // Convert to base32 string
        return Base32Encoding.ToString(secretKey);
    }
    
    public bool ValidateTotpCode(string secret, string totpCode)
    {
        try
        {
            // Create a TOTP object with the secret
            var totp = new Totp(Base32Encoding.ToBytes(secret));
            
            // Verify the code (with a 1-step window for slight time differences)
            return totp.VerifyTotp(totpCode, out _, new VerificationWindow(1, 1));
        }
        catch
        {
            // If there's any error (invalid secret format, etc.), return false
            return false;
        }
    }
    
    public string GenerateQrCodeUrl(string issuer, string accountName, string secret)
    {
        return $"otpauth://totp/{issuer}:{accountName}?secret={secret}&issuer={issuer}";
    }
}