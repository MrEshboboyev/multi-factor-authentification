using Application.Abstractions.Messaging;
using Application.Abstractions.Security;
using Domain.Errors;
using Domain.Repositories;
using Domain.Shared;
using Domain.ValueObjects;

namespace Application.Users.Commands.LoginWithMfa;

internal sealed class LoginWithMfaCommandHandler(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    IJwtProvider jwtProvider) : ICommandHandler<LoginWithMfaCommand, string>
{
    public async Task<Result<string>> Handle(
        LoginWithMfaCommand request,
        CancellationToken cancellationToken)
    {
        var (email, password, recoveryCode) = request;

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
        
        #region Recovery code validation

        var recoveryCodeResult = RecoveryCode.Create(recoveryCode);
        if (recoveryCodeResult.IsFailure)
        {
            return Result.Failure<string>(
                recoveryCodeResult.Error);
        }
        
        // Validate the recovery code
        if (!user.ValidateRecoveryCode(recoveryCodeResult.Value))
        {
            return Result.Failure<string>(
                DomainErrors.User.InvalidRecoveryCode);
        }
        
        #endregion

        #region Generate JWT token
        
        // Generate a JWT token for the authenticated user
        var token = jwtProvider.Generate(user);
        
        #endregion

        return Result.Success(token);
    }
}   