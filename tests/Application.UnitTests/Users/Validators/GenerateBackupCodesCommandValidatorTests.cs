using Application.Users.Commands.GenerateBackupCodes;
using FluentAssertions;
using FluentValidation.TestHelper;

namespace Application.UnitTests.Users.Validators;

public class GenerateBackupCodesCommandValidatorTests
{
    private readonly GenerateBackupCodesCommandValidator _validator;

    public GenerateBackupCodesCommandValidatorTests()
    {
        _validator = new GenerateBackupCodesCommandValidator();
    }

    [Fact]
    public void Validate_Should_NotHaveValidationError_When_CommandIsValid()
    {
        // Arrange
        var command = new GenerateBackupCodesCommand("user@example.com", "password123");

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
        var command = new GenerateBackupCodesCommand("", "password123");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Validate_Should_HaveValidationError_When_EmailIsInvalid()
    {
        // Arrange
        var command = new GenerateBackupCodesCommand("invalid-email", "password123");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Validate_Should_HaveValidationError_When_PasswordIsEmpty()
    {
        // Arrange
        var command = new GenerateBackupCodesCommand("user@example.com", "");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }
}