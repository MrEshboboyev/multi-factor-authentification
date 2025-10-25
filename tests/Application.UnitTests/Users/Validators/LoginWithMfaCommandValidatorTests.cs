using Application.Users.Commands.LoginWithMfa;
using FluentAssertions;
using FluentValidation.TestHelper;

namespace Application.UnitTests.Users.Validators;

public class LoginWithMfaCommandValidatorTests
{
    private readonly LoginWithMfaCommandValidator _validator;

    public LoginWithMfaCommandValidatorTests()
    {
        _validator = new LoginWithMfaCommandValidator();
    }

    [Fact]
    public void Validate_Should_NotHaveValidationError_When_CommandIsValid()
    {
        // Arrange
        var command = new LoginWithMfaCommand("user@example.com", "password123", "123456");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Email);
        result.ShouldNotHaveValidationErrorFor(x => x.Password);
        result.ShouldNotHaveValidationErrorFor(x => x.RecoveryCode);
    }

    [Fact]
    public void Validate_Should_HaveValidationError_When_EmailIsEmpty()
    {
        // Arrange
        var command = new LoginWithMfaCommand("", "password123", "123456");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Validate_Should_HaveValidationError_When_EmailIsInvalid()
    {
        // Arrange
        var command = new LoginWithMfaCommand("invalid-email", "password123", "123456");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Validate_Should_HaveValidationError_When_PasswordIsEmpty()
    {
        // Arrange
        var command = new LoginWithMfaCommand("user@example.com", "", "123456");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Validate_Should_HaveValidationError_When_RecoveryCodeIsEmpty()
    {
        // Arrange
        var command = new LoginWithMfaCommand("user@example.com", "password123", "");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.RecoveryCode);
    }
}