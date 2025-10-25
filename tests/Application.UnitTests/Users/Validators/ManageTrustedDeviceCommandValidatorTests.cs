using Application.Users.Commands.ManageTrustedDevice;
using FluentAssertions;
using FluentValidation.TestHelper;

namespace Application.UnitTests.Users.Validators;

public class ManageTrustedDeviceCommandValidatorTests
{
    private readonly ManageTrustedDeviceCommandValidator _validator;

    public ManageTrustedDeviceCommandValidatorTests()
    {
        _validator = new ManageTrustedDeviceCommandValidator();
    }

    [Fact]
    public void Validate_Should_NotHaveValidationError_When_CommandIsValid()
    {
        // Arrange
        var command = new ManageTrustedDeviceCommand("user@example.com", "password123", "device123", true);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Email);
        result.ShouldNotHaveValidationErrorFor(x => x.Password);
        result.ShouldNotHaveValidationErrorFor(x => x.DeviceId);
    }

    [Fact]
    public void Validate_Should_HaveValidationError_When_EmailIsEmpty()
    {
        // Arrange
        var command = new ManageTrustedDeviceCommand("", "password123", "device123", true);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Validate_Should_HaveValidationError_When_EmailIsInvalid()
    {
        // Arrange
        var command = new ManageTrustedDeviceCommand("invalid-email", "password123", "device123", true);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Validate_Should_HaveValidationError_When_PasswordIsEmpty()
    {
        // Arrange
        var command = new ManageTrustedDeviceCommand("user@example.com", "", "device123", true);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Validate_Should_HaveValidationError_When_DeviceIdIsEmpty()
    {
        // Arrange
        var command = new ManageTrustedDeviceCommand("user@example.com", "password123", "", true);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.DeviceId);
    }
}