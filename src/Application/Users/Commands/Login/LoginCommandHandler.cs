using Application.Abstractions.Messaging;
using Application.Abstractions.Security;
using Domain.Errors;
using Domain.Repositories;
using Domain.Shared;
using Domain.ValueObjects;

namespace Application.Users.Commands.Login;

internal sealed class LoginCommandHandler(
    IUserRepository userRepository,
    IJwtProvider jwtProvider,
    IPasswordHasher passwordHasher) : ICommandHandler<LoginCommand, string>
{
    public async Task<Result<string>> Handle(LoginCommand request,
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

        #region Checking Mfa enabled
        
        if (user.IsMfaEnabled)
        {
            return Result.Failure<string>(
                DomainErrors.User.MfaLoginRequired);
        }
        
        #endregion
        
        #region Generate token

        // Generate a JWT token for the authenticated user
        var token = jwtProvider.Generate(user);

        #endregion

        return Result.Success(token);
    }
}