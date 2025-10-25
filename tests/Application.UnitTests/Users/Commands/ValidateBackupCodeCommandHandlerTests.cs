using Application.Abstractions.Security;
using Application.Users.Commands.ValidateBackupCode;
using Domain.Entities;
using Domain.Errors;
using Domain.Repositories;
using Domain.Shared;
using Domain.ValueObjects;
using FluentAssertions;
using Moq;

namespace Application.UnitTests.Users.Commands;

public class ValidateBackupCodeCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IPasswordHasher> _passwordHasherMock;
    private readonly Mock<IJwtProvider> _jwtProviderMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly ValidateBackupCodeCommandHandler _handler;

    public ValidateBackupCodeCommandHandlerTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _passwordHasherMock = new Mock<IPasswordHasher>();
        _jwtProviderMock = new Mock<IJwtProvider>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _handler = new ValidateBackupCodeCommandHandler(
            _userRepositoryMock.Object,
            _passwordHasherMock.Object,
            _jwtProviderMock.Object,
            _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_Should_ReturnSuccess_When_BackupCodeIsValid()
    {
        // Arrange
        var email = "user@example.com";
        var password = "password123";
        var backupCode = "ABCD-EFGH";
        var command = new ValidateBackupCodeCommand(email, password, backupCode);

        var emailResult = Email.Create(email);
        var user = User.Create(Guid.NewGuid(), emailResult.Value, "hashedPassword", FullName.Create("John Doe").Value);
        
        // Enable MFA and generate backup codes
        var recoveryCode = RecoveryCode.Create("ABC123").Value;
        user.EnableMfa(recoveryCode);
        user.GenerateBackupCodes();

        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync(It.IsAny<Email>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _passwordHasherMock
            .Setup(x => x.Verify(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(true);

        _jwtProviderMock
            .Setup(x => x.Generate(It.IsAny<User>()))
            .Returns("jwtToken");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("jwtToken");
        _userRepositoryMock.Verify(x => x.Update(It.IsAny<User>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_ReturnFailure_When_EmailIsInvalid()
    {
        // Arrange
        var email = "invalid-email";
        var password = "password123";
        var backupCode = "ABCD-EFGH";
        var command = new ValidateBackupCodeCommand(email, password, backupCode);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Email.InvalidFormat");
    }

    [Fact]
    public async Task Handle_Should_ReturnFailure_When_UserNotFound()
    {
        // Arrange
        var email = "user@example.com";
        var password = "password123";
        var backupCode = "ABCD-EFGH";
        var command = new ValidateBackupCodeCommand(email, password, backupCode);

        var emailResult = Email.Create(email);

        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync(It.IsAny<Email>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DomainErrors.User.InvalidCredentials);
    }

    [Fact]
    public async Task Handle_Should_ReturnFailure_When_PasswordIsInvalid()
    {
        // Arrange
        var email = "user@example.com";
        var password = "password123";
        var backupCode = "ABCD-EFGH";
        var command = new ValidateBackupCodeCommand(email, password, backupCode);

        var emailResult = Email.Create(email);
        var user = User.Create(Guid.NewGuid(), emailResult.Value, "hashedPassword", FullName.Create("John Doe").Value);

        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync(It.IsAny<Email>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _passwordHasherMock
            .Setup(x => x.Verify(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DomainErrors.User.InvalidCredentials);
    }

    [Fact]
    public async Task Handle_Should_ReturnFailure_When_MfaIsNotEnabled()
    {
        // Arrange
        var email = "user@example.com";
        var password = "password123";
        var backupCode = "ABCD-EFGH";
        var command = new ValidateBackupCodeCommand(email, password, backupCode);

        var emailResult = Email.Create(email);
        var user = User.Create(Guid.NewGuid(), emailResult.Value, "hashedPassword", FullName.Create("John Doe").Value);
        // MFA is not enabled by default

        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync(It.IsAny<Email>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _passwordHasherMock
            .Setup(x => x.Verify(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DomainErrors.User.MfaNotEnabled);
    }

    [Fact]
    public async Task Handle_Should_ReturnFailure_When_MfaIsLocked()
    {
        // Arrange
        var email = "user@example.com";
        var password = "password123";
        var backupCode = "ABCD-EFGH";
        var command = new ValidateBackupCodeCommand(email, password, backupCode);

        var emailResult = Email.Create(email);
        var user = User.Create(Guid.NewGuid(), emailResult.Value, "hashedPassword", FullName.Create("John Doe").Value);
        
        // Enable MFA
        var recoveryCode = RecoveryCode.Create("ABC123").Value;
        user.EnableMfa(recoveryCode);
        
        // Lock MFA by setting a future lock time
        typeof(User).GetProperty("MfaLockedUntil")?.SetValue(user, DateTime.UtcNow.AddMinutes(30));

        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync(It.IsAny<Email>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _passwordHasherMock
            .Setup(x => x.Verify(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DomainErrors.User.MfaLocked);
    }

    [Fact]
    public async Task Handle_Should_ReturnFailure_When_BackupCodeIsInvalidFormat()
    {
        // Arrange
        var email = "user@example.com";
        var password = "password123";
        var backupCode = "invalid-code"; // Invalid format
        var command = new ValidateBackupCodeCommand(email, password, backupCode);

        var emailResult = Email.Create(email);
        var user = User.Create(Guid.NewGuid(), emailResult.Value, "hashedPassword", FullName.Create("John Doe").Value);
        
        // Enable MFA
        var recoveryCode = RecoveryCode.Create("ABC123").Value;
        user.EnableMfa(recoveryCode);

        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync(It.IsAny<Email>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _passwordHasherMock
            .Setup(x => x.Verify(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        _userRepositoryMock.Verify(x => x.Update(It.IsAny<User>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_ReturnFailure_When_BackupCodeIsInvalid()
    {
        // Arrange
        var email = "user@example.com";
        var password = "password123";
        var backupCode = "ABCD-EFGH"; // Valid format but not in user's backup codes
        var command = new ValidateBackupCodeCommand(email, password, backupCode);

        var emailResult = Email.Create(email);
        var user = User.Create(Guid.NewGuid(), emailResult.Value, "hashedPassword", FullName.Create("John Doe").Value);
        
        // Enable MFA but don't add the backup code
        var recoveryCode = RecoveryCode.Create("ABC123").Value;
        user.EnableMfa(recoveryCode);
        user.GenerateBackupCodes();

        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync(It.IsAny<Email>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _passwordHasherMock
            .Setup(x => x.Verify(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DomainErrors.User.InvalidBackupCode);
        _userRepositoryMock.Verify(x => x.Update(It.IsAny<User>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}