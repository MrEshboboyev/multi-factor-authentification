using Application.Abstractions.Messaging;
using Application.Abstractions.Security;
using Domain.Errors;
using Domain.Repositories;
using Domain.Shared;
using Domain.ValueObjects;

namespace Application.Users.Commands.DisableMfa;

internal sealed class DisableMfaCommandHandler(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    IUnitOfWork unitOfWork) : ICommandHandler<DisableMfaCommand>
{
    public async Task<Result> Handle(
        DisableMfaCommand request,
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

        #region Validate recovery code
        
        var recoveryCodeResult = RecoveryCode.Create(recoveryCode);
        if (recoveryCodeResult.IsFailure)
        {
            return Result.Failure<string>(
                recoveryCodeResult.Error);
        }
        
        // Validate the recovery code
        if (!user.ValidateRecoveryCode(recoveryCodeResult.Value))
        {
            return Result.Failure(
                DomainErrors.User.InvalidRecoveryCode);
        }
        
        #endregion

        #region Disable Mfa
        
        // Disable MFA for the user
        var disableMfaResult = user.DisableMfa();
        if (disableMfaResult.IsFailure)
        {
            return Result.Failure(
                disableMfaResult.Error);
        }
        
        #endregion

        #region Update database
        
        // Update the user in the repository
        userRepository.Update(user);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        
        #endregion

        return Result.Success();
    }
}