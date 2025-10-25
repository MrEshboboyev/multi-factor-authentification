using Application.Users.Commands.ValidateTotp;
using FluentAssertions;
using FluentValidation.TestHelper;

namespace Application.UnitTests.Users.Validators;

public class ValidateTotpCommandValidatorTests
{
    private readonly ValidateTotpCommandValidator _validator;

    public ValidateTotpCommandValidatorTests()
    {
        _validator = new ValidateTotpCommandValidator();
    }

    [Fact]
    public void Validate_Should_NotHaveValidationError_When_CommandIsValid()
    {
        // Arrange
        var command = new ValidateTotpCommand("user@example.com", "password123", "123456");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Email);
        result.ShouldNotHaveValidationErrorFor(x => x.Password);
        result.ShouldNotHaveValidationErrorFor(x => x.TotpCode);
    }

    [Fact]
    public void Validate_Should_HaveValidationError_When_EmailIsEmpty()
    {
        // Arrange
        var command = new ValidateTotpCommand("", "password123", "123456");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Validate_Should_HaveValidationError_When_EmailIsInvalid()
    {
        // Arrange
        var command = new ValidateTotpCommand("invalid-email", "password123", "123456");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Validate_Should_HaveValidationError_When_PasswordIsEmpty()
    {
        // Arrange
        var command = new ValidateTotpCommand("user@example.com", "", "123456");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Validate_Should_HaveValidationError_When_TotpCodeIsEmpty()
    {
        // Arrange
        var command = new ValidateTotpCommand("user@example.com", "password123", "");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.TotpCode);
    }

    [Fact]
    public void Validate_Should_HaveValidationError_When_TotpCodeHasInvalidLength()
    {
        // Arrange
        var command = new ValidateTotpCommand("user@example.com", "password123", "12345"); // 5 characters

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.TotpCode);
    }

    [Fact]
    public void Validate_Should_NotHaveValidationError_When_TotpCodeHasValidLength()
    {
        // Arrange
        var command = new ValidateTotpCommand("user@example.com", "password123", "123456"); // 6 characters

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.TotpCode);
    }
}