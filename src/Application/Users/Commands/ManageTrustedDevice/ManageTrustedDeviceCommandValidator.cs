using FluentValidation;

namespace Application.Users.Commands.ManageTrustedDevice;

internal sealed class ManageTrustedDeviceCommandValidator : AbstractValidator<ManageTrustedDeviceCommand>
{
    public ManageTrustedDeviceCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty();
        RuleFor(x => x.DeviceId).NotEmpty();
    }
}
