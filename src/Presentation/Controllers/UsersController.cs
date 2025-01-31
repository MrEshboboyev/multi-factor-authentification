using Application.Users.Commands.DisableMfa;
using Application.Users.Commands.EnableMfa;
using Application.Users.Commands.Login;
using Application.Users.Commands.LoginWithMfa;
using Application.Users.Commands.RegisterUser;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Presentation.Abstractions;
using Presentation.Contracts.Users;

namespace Presentation.Controllers;

[Route("api/users")]
public sealed class UsersController(ISender sender) : ApiController(sender)
{
    #region Login and Registration

    [HttpPost("login")]
    public async Task<IActionResult> LoginUser(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        var command = new LoginCommand(
            request.Email,
            request.Password);

        var tokenResult = await Sender.Send(command, cancellationToken);

        return tokenResult.IsFailure ? HandleFailure(tokenResult) : Ok(tokenResult.Value);
    }

    [HttpPost("login-with-mfa")]
    public async Task<IActionResult> LoginWithMfa(
        [FromBody] LoginWithMfaRequest request,
        CancellationToken cancellationToken)
    {
        var command = new LoginWithMfaCommand(
            request.Email,
            request.Password,
            request.RecoveryCode);

        var tokenResult = await Sender.Send(command, cancellationToken);

        return tokenResult.IsFailure ? HandleFailure(tokenResult) : Ok(tokenResult.Value);
    }

    [HttpPost("register")]
    public async Task<IActionResult> RegisterUser(
        [FromBody] RegisterUserRequest request,
        CancellationToken cancellationToken)
    {
        var command = new RegisterUserCommand(
            request.Email,
            request.Password,
            request.FullName);

        var result = await Sender.Send(command, cancellationToken);

        return result.IsFailure ? HandleFailure(result) : Ok();
    }

    #endregion

    #region MFA Endpoints

    [HttpPost("enable-mfa")]
    public async Task<IActionResult> EnableMfa(
        [FromBody] EnableMfaRequest request,
        CancellationToken cancellationToken)
    {
        var command = new EnableMfaCommand(
            request.Email,
            request.Password);

        var recoveryCodeResult = await Sender.Send(command, cancellationToken);

        return recoveryCodeResult.IsFailure ? HandleFailure(recoveryCodeResult) : Ok(recoveryCodeResult.Value);
    }

    [HttpPost("disable-mfa")]
    public async Task<IActionResult> DisableMfa(
        [FromBody] DisableMfaRequest request,
        CancellationToken cancellationToken)
    {
        var command = new DisableMfaCommand(
            request.Email,
            request.Password,
            request.RecoveryCode);

        var result = await Sender.Send(command, cancellationToken);

        return result.IsFailure ? HandleFailure(result) : Ok();
    }

    #endregion
}