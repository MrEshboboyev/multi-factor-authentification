namespace Application.Abstractions.Security;

public interface IMfaProvider
{
    string GenerateRecoveryCode();
    string GenerateTotpSecret();
    bool ValidateTotpCode(string secret, string totpCode);
    string GenerateQrCodeUrl(string issuer, string accountName, string secret);
}