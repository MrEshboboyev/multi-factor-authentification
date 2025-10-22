using Application.Abstractions.Messaging;
using Application.Abstractions.Security;
using Domain.Errors;
using Domain.Repositories;
using Domain.Shared;

namespace Application.Users.Commands.GenerateBackupCodes;

internal sealed class GenerateBackupCodesCommandHandler(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    IUnitOfWork unitOfWork
) : ICommandHandler<GenerateBackupCodesCommand, IEnumerable<string>>
{
    public async Task<Result<IEnumerable<string>>> Handle(
        GenerateBackupCodesCommand request,
        CancellationToken cancellationToken)
    {
        var (email, password) = request;

        #region Checking user exists by this email and credentials valid

        // Validate and create the Email value object
        var createEmailResult = Domain.ValueObjects.Email.Create(email);
        if (createEmailResult.IsFailure)
        {
            return Result.Failure<IEnumerable<string>>(
                createEmailResult.Error);
        }
        
        // Retrieve the user by email
        var user = await userRepository.GetByEmailAsync(
            createEmailResult.Value,
            cancellationToken);
        
        // Verify if user exists and the password matches
        if (user is null || !passwordHasher.Verify(password, user.PasswordHash))
        {
            return Result.Failure<IEnumerable<string>>(
                DomainErrors.User.InvalidCredentials);
        }

        #endregion

        #region Checking Mfa for this user
        
        // Check if MFA is enabled
        if (!user.IsMfaEnabled)
        {
            return Result.Failure<IEnumerable<string>>(
                DomainErrors.User.MfaNotEnabled);
        }
        
        #endregion
        
        #region Generate backup codes

        var generateResult = user.GenerateBackupCodes();
        if (generateResult.IsFailure)
        {
            return Result.Failure<IEnumerable<string>>(
                generateResult.Error);
        }
        
        #endregion
        
        #region Update database

        userRepository.Update(user);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        
        #endregion

        return Result.Success(user.BackupCodes.Select(bc => bc.Value));
    }
}
