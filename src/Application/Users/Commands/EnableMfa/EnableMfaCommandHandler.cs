using Application.Abstractions.Messaging;
using Application.Abstractions.Security;
using Domain.Errors;
using Domain.Repositories;
using Domain.Shared;
using Domain.ValueObjects;

namespace Application.Users.Commands.EnableMfa;

internal sealed class EnableMfaCommandHandler(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    IMfaProvider mfaProvider,
    IUnitOfWork unitOfWork) : ICommandHandler<EnableMfaCommand, string>
{
    public async Task<Result<string>> Handle(
        EnableMfaCommand request,
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

        #region Generate recovery code and create
        
        // Generate a recovery code
        var recoveryCode = mfaProvider.GenerateRecoveryCode();
        
        var recoveryCodeResult = RecoveryCode.Create(recoveryCode);
        if (recoveryCodeResult.IsFailure)
        {
            return Result.Failure<string>(
                recoveryCodeResult.Error);
        }
        
        #endregion
        
        #region EnableMfa for this user

        // Enable MFA for the user
        var enableMfaResult = user.EnableMfa(recoveryCodeResult.Value);
        if (enableMfaResult.IsFailure)
        {
            return Result.Failure<string>(
                enableMfaResult.Error);
        }
        
        #endregion
        
        #region Update database

        // Update the user in the repository
        userRepository.Update(user);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        
        #endregion

        return Result.Success(recoveryCode);
    }
}