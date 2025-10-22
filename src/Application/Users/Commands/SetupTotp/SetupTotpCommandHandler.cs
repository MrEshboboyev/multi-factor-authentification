using Application.Abstractions.Messaging;
using Application.Abstractions.Security;
using Domain.Errors;
using Domain.Repositories;
using Domain.Shared;
using Domain.ValueObjects;

namespace Application.Users.Commands.SetupTotp;

internal sealed class SetupTotpCommandHandler(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    IMfaProvider mfaProvider) : ICommandHandler<SetupTotpCommand, string>
{
    public async Task<Result<string>> Handle(
        SetupTotpCommand request,
        CancellationToken cancellationToken)
    {
        var (email, password) = request;

        #region Checking user exists by this email and credentials valid

        // Validate and create the Email value object
        var createEmailResult = Email.Create(email);
        if (createEmailResult.IsFailure)
        {
            return Result.Failure<string>(
                createEmailResult.Error);
        }
        
        // Retrieve the user by email
        var user = await userRepository.GetByEmailAsync(
            createEmailResult.Value,
            cancellationToken);
        
        // Verify if user exists and the password matches
        if (user is null || !passwordHasher.Verify(password, user.PasswordHash))
        {
            return Result.Failure<string>(
                DomainErrors.User.InvalidCredentials);
        }

        #endregion

        #region Checking Mfa for this user
        
        // Check if MFA is enabled
        if (!user.IsMfaEnabled)
        {
            return Result.Failure<string>(
                DomainErrors.User.MfaNotEnabled);
        }
        
        #endregion
        
        #region Generate TOTP secret

        var totpSecret = mfaProvider.GenerateTotpSecret();
        var totpSecretResult = TotpSecret.Create(totpSecret);
        if (totpSecretResult.IsFailure)
        {
            return Result.Failure<string>(
                totpSecretResult.Error);
        }
        
        var setResult = user.SetTotpSecret(totpSecretResult.Value);
        if (setResult.IsFailure)
        {
            return Result.Failure<string>(
                setResult.Error);
        }
        
        #endregion
        
        #region Generate QR code URL

        var qrCodeUrl = mfaProvider.GenerateQrCodeUrl("MFA App", user.Email.Value, totpSecret);
        
        #endregion

        return Result.Success(qrCodeUrl);
    }
}