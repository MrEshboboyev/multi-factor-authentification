namespace Application.Abstractions.Security;

public interface IMfaProvider
{
    string GenerateRecoveryCode();
}