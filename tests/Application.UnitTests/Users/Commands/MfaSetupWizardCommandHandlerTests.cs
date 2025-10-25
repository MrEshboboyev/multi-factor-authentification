using Application.Abstractions.Security;
using Application.Users.Commands.MfaSetupWizard;
using Domain.Entities;
using Domain.Errors;
using Domain.Repositories;
using Domain.Shared;
using Domain.ValueObjects;
using FluentAssertions;
using Moq;

namespace Application.UnitTests.Users.Commands;

public class MfaSetupWizardCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IPasswordHasher> _passwordHasherMock;
    private readonly Mock<IMfaProvider> _mfaProviderMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly MfaSetupWizardCommandHandler _handler;

    public MfaSetupWizardCommandHandlerTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _passwordHasherMock = new Mock<IPasswordHasher>();
        _mfaProviderMock = new Mock<IMfaProvider>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _handler = new MfaSetupWizardCommandHandler(
            _userRepositoryMock.Object,
            _passwordHasherMock.Object,
            _mfaProviderMock.Object,
            _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_Should_ReturnSuccess_When_MfaIsSetupForNewUser()
    {
        // Arrange
        var email = "user@example.com";
        var password = "password123";
        var command = new MfaSetupWizardCommand(email, password);

        var emailResult = Email.Create(email);
        var user = User.Create(Guid.NewGuid(), emailResult.Value, "hashedPassword", FullName.Create("John Doe").Value);
        // MFA is not enabled by default

        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync(It.IsAny<Email>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _passwordHasherMock
            .Setup(x => x.Verify(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(true);

        _mfaProviderMock
            .Setup(x => x.GenerateRecoveryCode())
            .Returns("ABC123");

        _mfaProviderMock
            .Setup(x => x.GenerateTotpSecret())
            .Returns("totpSecret123");

        _mfaProviderMock
            .Setup(x => x.GenerateQrCodeUrl(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns("qrCodeUrl");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.RecoveryCode.Should().NotBeNullOrEmpty();
        result.Value.TotpQrCodeUrl.Should().NotBeNullOrEmpty();
        result.Value.BackupCodes.Should().NotBeEmpty();
        result.Value.Message.Should().NotBeNullOrEmpty();
        _userRepositoryMock.Verify(x => x.Update(It.IsAny<User>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_ReturnSuccess_When_MfaIsAlreadyEnabled()
    {
        // Arrange
        var email = "user@example.com";
        var password = "password123";
        var command = new MfaSetupWizardCommand(email, password);

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

        _mfaProviderMock
            .Setup(x => x.GenerateTotpSecret())
            .Returns("totpSecret123");

        _mfaProviderMock
            .Setup(x => x.GenerateQrCodeUrl(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns("qrCodeUrl");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.RecoveryCode.Should().BeNull(); // No new recovery code when MFA already enabled
        result.Value.TotpQrCodeUrl.Should().NotBeNullOrEmpty();
        result.Value.BackupCodes.Should().NotBeEmpty();
        result.Value.Message.Should().NotBeNullOrEmpty();
        _userRepositoryMock.Verify(x => x.Update(It.IsAny<User>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_ReturnFailure_When_EmailIsInvalid()
    {
        // Arrange
        var email = "invalid-email";
        var password = "password123";
        var command = new MfaSetupWizardCommand(email, password);

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
        var command = new MfaSetupWizardCommand(email, password);

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
        var command = new MfaSetupWizardCommand(email, password);

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
    public async Task Handle_Should_ReturnFailure_When_RecoveryCodeGenerationFails()
    {
        // Arrange
        var email = "user@example.com";
        var password = "password123";
        var command = new MfaSetupWizardCommand(email, password);

        var emailResult = Email.Create(email);
        var user = User.Create(Guid.NewGuid(), emailResult.Value, "hashedPassword", FullName.Create("John Doe").Value);
        // MFA is not enabled by default

        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync(It.IsAny<Email>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _passwordHasherMock
            .Setup(x => x.Verify(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(true);

        _mfaProviderMock
            .Setup(x => x.GenerateRecoveryCode())
            .Returns(""); // Invalid recovery code

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_Should_ReturnFailure_When_TotpSecretGenerationFails()
    {
        // Arrange
        var email = "user@example.com";
        var password = "password123";
        var command = new MfaSetupWizardCommand(email, password);

        var emailResult = Email.Create(email);
        var user = User.Create(Guid.NewGuid(), emailResult.Value, "hashedPassword", FullName.Create("John Doe").Value);
        // MFA is not enabled by default

        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync(It.IsAny<Email>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _passwordHasherMock
            .Setup(x => x.Verify(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(true);

        _mfaProviderMock
            .Setup(x => x.GenerateRecoveryCode())
            .Returns("ABC123");

        _mfaProviderMock
            .Setup(x => x.GenerateTotpSecret())
            .Returns(""); // Invalid TOTP secret

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
    }
}