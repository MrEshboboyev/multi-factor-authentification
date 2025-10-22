using Application.Abstractions.Messaging;
using Application.Abstractions.Security;
using Domain.Errors;
using Domain.Repositories;
using Domain.Shared;

namespace Application.Users.Commands.ValidateTotp;

internal sealed class ValidateTotpCommandHandler(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    IMfaProvider mfaProvider,
    IJwtProvider jwtProvider,
    IUnitOfWork unitOfWork) : ICommandHandler<ValidateTotpCommand, string>
{
    public async Task<Result<string>> Handle(
        ValidateTotpCommand request,
        CancellationToken cancellationToken)
    {
        var (email, password, totpCode) = request;

        #region Checking user exists by this email and credentials valid

        // Validate and create the Email value object
        var createEmailResult = Domain.ValueObjects.Email.Create(email);
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
        
        // Check if MFA is locked
        if (user.IsMfaLocked())
        {
            return Result.Failure<string>(
                DomainErrors.User.MfaLocked);
        }
        
        #endregion
        
        #region Validate TOTP code

        if (user.TotpSecret == null || !mfaProvider.ValidateTotpCode(user.TotpSecret.Value, totpCode))
        {
            user.IncrementFailedMfaAttempts();
            userRepository.Update(user);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            
            return Result.Failure<string>(
                DomainErrors.User.InvalidCredentials);
        }
        
        #endregion
        
        #region Reset failed attempts and generate JWT token

        user.ResetFailedMfaAttempts();
        userRepository.Update(user);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        
        // Generate a JWT token for the authenticated user
        var token = jwtProvider.Generate(user);
        
        #endregion

        return Result.Success(token);
    }
}