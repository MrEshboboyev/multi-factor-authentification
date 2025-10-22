using Application.Users.Commands.DisableMfa;
using Application.Users.Commands.EnableMfa;
using Application.Users.Commands.GenerateBackupCodes;
using Application.Users.Commands.Login;
using Application.Users.Commands.LoginWithMfa;
using Application.Users.Commands.ManageTrustedDevice;
using Application.Users.Commands.MfaSetupWizard;
using Application.Users.Commands.RegisterUser;
using Application.Users.Commands.SetupTotp;
using Application.Users.Commands.ValidateBackupCode;
using Application.Users.Commands.ValidateTotp;
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
    
    #region Advanced MFA Endpoints
    
    [HttpPost("setup-totp")]
    public async Task<IActionResult> SetupTotp(
        [FromBody] SetupTotpRequest request,
        CancellationToken cancellationToken)
    {
        var command = new SetupTotpCommand(
            request.Email,
            request.Password);

        var qrCodeUrlResult = await Sender.Send(command, cancellationToken);

        return qrCodeUrlResult.IsFailure ? HandleFailure(qrCodeUrlResult) : Ok(qrCodeUrlResult.Value);
    }
    
    [HttpPost("validate-totp")]
    public async Task<IActionResult> ValidateTotp(
        [FromBody] ValidateTotpRequest request,
        CancellationToken cancellationToken)
    {
        var command = new ValidateTotpCommand(
            request.Email,
            request.Password,
            request.TotpCode);

        var tokenResult = await Sender.Send(command, cancellationToken);

        return tokenResult.IsFailure ? HandleFailure(tokenResult) : Ok(tokenResult.Value);
    }
    
    [HttpPost("generate-backup-codes")]
    public async Task<IActionResult> GenerateBackupCodes(
        [FromBody] GenerateBackupCodesRequest request,
        CancellationToken cancellationToken)
    {
        var command = new GenerateBackupCodesCommand(
            request.Email,
            request.Password);

        var backupCodesResult = await Sender.Send(command, cancellationToken);

        return backupCodesResult.IsFailure ? HandleFailure(backupCodesResult) : Ok(backupCodesResult.Value);
    }
    
    [HttpPost("validate-backup-code")]
    public async Task<IActionResult> ValidateBackupCode(
        [FromBody] ValidateBackupCodeRequest request,
        CancellationToken cancellationToken)
    {
        var command = new ValidateBackupCodeCommand(
            request.Email,
            request.Password,
            request.BackupCode);

        var tokenResult = await Sender.Send(command, cancellationToken);

        return tokenResult.IsFailure ? HandleFailure(tokenResult) : Ok(tokenResult.Value);
    }
    
    [HttpPost("mfa-setup-wizard")]
    public async Task<IActionResult> MfaSetupWizard(
        [FromBody] MfaSetupWizardRequest request,
        CancellationToken cancellationToken)
    {
        var command = new MfaSetupWizardCommand(
            request.Email,
            request.Password);

        var result = await Sender.Send(command, cancellationToken);

        return result.IsFailure ? HandleFailure(result) : Ok(result.Value);
    }
    
    [HttpPost("manage-trusted-device")]
    public async Task<IActionResult> ManageTrustedDevice(
        [FromBody] ManageTrustedDeviceRequest request,
        CancellationToken cancellationToken)
    {
        var command = new ManageTrustedDeviceCommand(
            request.Email,
            request.Password,
            request.DeviceId,
            request.TrustDevice);

        var result = await Sender.Send(command, cancellationToken);

        return result.IsFailure ? HandleFailure(result) : Ok();
    }

    #endregion
}