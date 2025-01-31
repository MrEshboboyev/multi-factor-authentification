using Application.Abstractions.Security;
using Application.Users.Commands.DisableMfa;
using Domain.Entities;
using Domain.Errors;
using Domain.Repositories;
using Domain.ValueObjects;
using FluentAssertions;
using Moq;

namespace Application.UnitTests.Users.Commands;

public class DisableMfaCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IPasswordHasher> _passwordHasherMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly DisableMfaCommandHandler _handler;

    public DisableMfaCommandHandlerTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _passwordHasherMock = new Mock<IPasswordHasher>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _handler = new DisableMfaCommandHandler(
            _userRepositoryMock.Object,
            _passwordHasherMock.Object,
            _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_Should_ReturnSuccess_When_MfaIsDisabled()
    {
        // Arrange
        var email = "user@example.com";
        var password = "password123";
        var recoveryCode = "123456";
        var user = User.Create(
            Guid.NewGuid(),
            Email.Create(email).Value,
            "hashedPassword",
            FullName.Create("John Doe").Value);

        user.EnableMfa(RecoveryCode.Create(recoveryCode).Value);

        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync(It.IsAny<Email>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _passwordHasherMock
            .Setup(x => x.Verify(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(true);

        var command = new DisableMfaCommand(email, password, recoveryCode);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_ReturnFailure_When_CredentialsAreInvalid()
    {
        // Arrange
        var email = "user@example.com";
        var password = "password123";
        var recoveryCode = "123456";

        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync(It.IsAny<Email>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var command = new DisableMfaCommand(email, password, recoveryCode);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DomainErrors.User.InvalidCredentials);
    }

    [Fact]
    public async Task Handle_Should_ReturnFailure_When_RecoveryCodeIsInvalid()
    {
        // Arrange
        var email = "user@example.com";
        var password = "password123";
        var recoveryCode = "123456";
        var user = User.Create(
            Guid.NewGuid(),
            Email.Create(email).Value,
            "hashedPassword",
            FullName.Create("John Doe").Value);

        user.EnableMfa(RecoveryCode.Create("654321").Value);

        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync(It.IsAny<Email>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _passwordHasherMock
            .Setup(x => x.Verify(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(true);

        var command = new DisableMfaCommand(email, password, recoveryCode);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DomainErrors.User.InvalidRecoveryCode);
    }
}