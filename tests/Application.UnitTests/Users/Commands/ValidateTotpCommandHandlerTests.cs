using Application.Abstractions.Security;
using Application.Users.Commands.ValidateTotp;
using Domain.Entities;
using Domain.Errors;
using Domain.Repositories;
using Domain.Shared;
using Domain.ValueObjects;
using FluentAssertions;
using Moq;

namespace Application.UnitTests.Users.Commands;

public class ValidateTotpCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IPasswordHasher> _passwordHasherMock;
    private readonly Mock<IMfaProvider> _mfaProviderMock;
    private readonly Mock<IJwtProvider> _jwtProviderMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly ValidateTotpCommandHandler _handler;

    public ValidateTotpCommandHandlerTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _passwordHasherMock = new Mock<IPasswordHasher>();
        _mfaProviderMock = new Mock<IMfaProvider>();
        _jwtProviderMock = new Mock<IJwtProvider>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _handler = new ValidateTotpCommandHandler(
            _userRepositoryMock.Object,
            _passwordHasherMock.Object,
            _mfaProviderMock.Object,
            _jwtProviderMock.Object,
            _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_Should_ReturnSuccess_When_TotpCodeIsValid()
    {
        // Arrange
        var email = "user@example.com";
        var password = "password123";
        var totpCode = "123456";
        var command = new ValidateTotpCommand(email, password, totpCode);

        var emailResult = Email.Create(email);
        var user = User.Create(Guid.NewGuid(), emailResult.Value, "hashedPassword", FullName.Create("John Doe").Value);
        
        // Enable MFA and set TOTP secret
        var recoveryCode = RecoveryCode.Create("ABC123").Value;
        user.EnableMfa(recoveryCode);
        var totpSecret = TotpSecret.Create("totpSecret123").Value;
        user.SetTotpSecret(totpSecret);

        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync(It.IsAny<Email>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _passwordHasherMock
            .Setup(x => x.Verify(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(true);

        _mfaProviderMock
            .Setup(x => x.ValidateTotpCode(It.IsAny<string>(), It.IsAny<string>()))
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
        var totpCode = "123456";
        var command = new ValidateTotpCommand(email, password, totpCode);

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
        var totpCode = "123456";
        var command = new ValidateTotpCommand(email, password, totpCode);

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
        var totpCode = "123456";
        var command = new ValidateTotpCommand(email, password, totpCode);

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
        var totpCode = "123456";
        var command = new ValidateTotpCommand(email, password, totpCode);

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
        var totpCode = "123456";
        var command = new ValidateTotpCommand(email, password, totpCode);

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
    public async Task Handle_Should_ReturnFailure_When_TotpCodeIsInvalid()
    {
        // Arrange
        var email = "user@example.com";
        var password = "password123";
        var totpCode = "123456";
        var command = new ValidateTotpCommand(email, password, totpCode);

        var emailResult = Email.Create(email);
        var user = User.Create(Guid.NewGuid(), emailResult.Value, "hashedPassword", FullName.Create("John Doe").Value);
        
        // Enable MFA and set TOTP secret
        var recoveryCode = RecoveryCode.Create("ABC123").Value;
        user.EnableMfa(recoveryCode);
        var totpSecret = TotpSecret.Create("totpSecret123").Value;
        user.SetTotpSecret(totpSecret);

        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync(It.IsAny<Email>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _passwordHasherMock
            .Setup(x => x.Verify(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(true);

        _mfaProviderMock
            .Setup(x => x.ValidateTotpCode(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(false); // Invalid TOTP code

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DomainErrors.User.InvalidCredentials);
        _userRepositoryMock.Verify(x => x.Update(It.IsAny<User>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_ReturnFailure_When_TotpSecretIsNull()
    {
        // Arrange
        var email = "user@example.com";
        var password = "password123";
        var totpCode = "123456";
        var command = new ValidateTotpCommand(email, password, totpCode);

        var emailResult = Email.Create(email);
        var user = User.Create(Guid.NewGuid(), emailResult.Value, "hashedPassword", FullName.Create("John Doe").Value);
        
        // Enable MFA but don't set TOTP secret
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
        result.Error.Should().Be(DomainErrors.User.InvalidCredentials);
        _userRepositoryMock.Verify(x => x.Update(It.IsAny<User>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}