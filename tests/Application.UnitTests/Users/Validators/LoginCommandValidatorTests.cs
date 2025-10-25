using Application.Users.Commands.Login;
using FluentAssertions;
using FluentValidation.TestHelper;

namespace Application.UnitTests.Users.Validators;

public class LoginCommandValidatorTests
{
    private readonly LoginCommandValidator _validator;

    public LoginCommandValidatorTests()
    {
        _validator = new LoginCommandValidator();
    }

    [Fact]
    public void Validate_Should_NotHaveValidationError_When_CommandIsValid()
    {
        // Arrange
        var command = new LoginCommand("user@example.com", "password123");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Email);
        result.ShouldNotHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Validate_Should_HaveValidationError_When_EmailIsEmpty()
    {
        // Arrange
        var command = new LoginCommand("", "password123");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Validate_Should_HaveValidationError_When_EmailIsInvalid()
    {
        // Arrange
        var command = new LoginCommand("invalid-email", "password123");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Validate_Should_HaveValidationError_When_PasswordIsEmpty()
    {
        // Arrange
        var command = new LoginCommand("user@example.com", "");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Validate_Should_HaveValidationError_When_PasswordIsNull()
    {
        // Arrange
        var command = new LoginCommand("user@example.com", null);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }
}