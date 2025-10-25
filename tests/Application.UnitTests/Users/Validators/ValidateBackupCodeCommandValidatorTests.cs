using Application.Users.Commands.ValidateBackupCode;
using FluentAssertions;
using FluentValidation.TestHelper;

namespace Application.UnitTests.Users.Validators;

public class ValidateBackupCodeCommandValidatorTests
{
    private readonly ValidateBackupCodeCommandValidator _validator;

    public ValidateBackupCodeCommandValidatorTests()
    {
        _validator = new ValidateBackupCodeCommandValidator();
    }

    [Fact]
    public void Validate_Should_NotHaveValidationError_When_CommandIsValid()
    {
        // Arrange
        var command = new ValidateBackupCodeCommand("user@example.com", "password123", "ABCD-EFGH");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Email);
        result.ShouldNotHaveValidationErrorFor(x => x.Password);
        result.ShouldNotHaveValidationErrorFor(x => x.BackupCode);
    }

    [Fact]
    public void Validate_Should_HaveValidationError_When_EmailIsEmpty()
    {
        // Arrange
        var command = new ValidateBackupCodeCommand("", "password123", "ABCD-EFGH");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Validate_Should_HaveValidationError_When_EmailIsInvalid()
    {
        // Arrange
        var command = new ValidateBackupCodeCommand("invalid-email", "password123", "ABCD-EFGH");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Validate_Should_HaveValidationError_When_PasswordIsEmpty()
    {
        // Arrange
        var command = new ValidateBackupCodeCommand("user@example.com", "", "ABCD-EFGH");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Validate_Should_HaveValidationError_When_BackupCodeIsEmpty()
    {
        // Arrange
        var command = new ValidateBackupCodeCommand("user@example.com", "password123", "");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.BackupCode);
    }

    [Fact]
    public void Validate_Should_HaveValidationError_When_BackupCodeHasInvalidFormat()
    {
        // Arrange
        var command = new ValidateBackupCodeCommand("user@example.com", "password123", "invalid-format");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.BackupCode);
    }

    [Fact]
    public void Validate_Should_NotHaveValidationError_When_BackupCodeHasValidFormat()
    {
        // Arrange
        var command = new ValidateBackupCodeCommand("user@example.com", "password123", "ABCD-EFGH");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.BackupCode);
    }
}