using Application.Abstractions.Messaging;
using Application.Abstractions.Security;
using Domain.Errors;
using Domain.Repositories;
using Domain.Shared;
using Domain.ValueObjects;

namespace Application.Users.Commands.MfaSetupWizard;

internal sealed class MfaSetupWizardCommandHandler(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    IMfaProvider mfaProvider,
    IUnitOfWork unitOfWork) : ICommandHandler<MfaSetupWizardCommand, MfaSetupWizardResult>
{
    public async Task<Result<MfaSetupWizardResult>> Handle(
        MfaSetupWizardCommand request,
        CancellationToken cancellationToken)
    {
        var (email, password) = request;

        #region Checking user exists by this email and credentials valid

        // Validate and create the Email value object
        var createEmailResult = Domain.ValueObjects.Email.Create(email);
        if (createEmailResult.IsFailure)
        {
            return Result.Failure<MfaSetupWizardResult>(
                createEmailResult.Error);
        }
        
        // Retrieve the user by email
        var user = await userRepository.GetByEmailAsync(
            createEmailResult.Value,
            cancellationToken);
        
        // Verify if user exists and the password matches
        if (user is null || !passwordHasher.Verify(password, user.PasswordHash))
        {
            return Result.Failure<MfaSetupWizardResult>(
                DomainErrors.User.InvalidCredentials);
        }

        #endregion

        #region Enable MFA if not already enabled

        string? recoveryCode = null;
        if (!user.IsMfaEnabled)
        {
            // Generate a recovery code
            recoveryCode = mfaProvider.GenerateRecoveryCode();
            
            var recoveryCodeResult = RecoveryCode.Create(recoveryCode);
            if (recoveryCodeResult.IsFailure)
            {
                return Result.Failure<MfaSetupWizardResult>(
                    recoveryCodeResult.Error);
            }
            
            // Enable MFA for the user
            var enableMfaResult = user.EnableMfa(recoveryCodeResult.Value);
            if (enableMfaResult.IsFailure)
            {
                return Result.Failure<MfaSetupWizardResult>(
                    enableMfaResult.Error);
            }
        }

        #endregion
        
        #region Generate TOTP secret

        var totpSecret = mfaProvider.GenerateTotpSecret();
        var totpSecretResult = TotpSecret.Create(totpSecret);
        if (totpSecretResult.IsFailure)
        {
            return Result.Failure<MfaSetupWizardResult>(
                totpSecretResult.Error);
        }
        
        var setResult = user.SetTotpSecret(totpSecretResult.Value);
        if (setResult.IsFailure)
        {
            return Result.Failure<MfaSetupWizardResult>(
                setResult.Error);
        }
        
        #endregion
        
        #region Generate backup codes

        var generateResult = user.GenerateBackupCodes();
        if (generateResult.IsFailure)
        {
            return Result.Failure<MfaSetupWizardResult>(
                generateResult.Error);
        }
        
        #endregion
        
        #region Generate QR code URL

        var qrCodeUrl = mfaProvider.GenerateQrCodeUrl("MFA App", user.Email.Value, totpSecret);
        
        #endregion
        
        #region Update database

        userRepository.Update(user);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        
        #endregion

        return Result.Success(new MfaSetupWizardResult
        {
            RecoveryCode = recoveryCode,
            TotpQrCodeUrl = qrCodeUrl,
            BackupCodes = user.BackupCodes.Select(bc => bc.Value),
            Message = "MFA has been successfully configured. Please save the recovery code and backup codes in a secure location."
        });
    }
}