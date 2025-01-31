using Application.Abstractions.Security;
using Application.Users.Commands.EnableMfa;
using Domain.Entities;
using Domain.Errors;
using Domain.Repositories;
using Domain.ValueObjects;
using FluentAssertions;
using Moq;

namespace Application.UnitTests.Users.Commands;

public class EnableMfaCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IPasswordHasher> _passwordHasherMock;
    private readonly Mock<IMfaProvider> _mfaProviderMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly EnableMfaCommandHandler _handler;

    public EnableMfaCommandHandlerTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _passwordHasherMock = new Mock<IPasswordHasher>();
        _mfaProviderMock = new Mock<IMfaProvider>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _handler = new EnableMfaCommandHandler(
            _userRepositoryMock.Object,
            _passwordHasherMock.Object,
            _mfaProviderMock.Object,
            _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_Should_ReturnSuccess_When_MfaIsEnabled()
    {
        // Arrange
        var email = "user@example.com";
        var password = "password123";
        var user = User.Create(
            Guid.NewGuid(),
            Email.Create(email).Value,
            "hashedPassword",
            FullName.Create("John Doe").Value);

        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync(It.IsAny<Email>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _passwordHasherMock
            .Setup(x => x.Verify(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(true);

        _mfaProviderMock
            .Setup(x => x.GenerateRecoveryCode())
            .Returns("123456");

        var command = new EnableMfaCommand(email, password);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("123456");
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_ReturnFailure_When_CredentialsAreInvalid()
    {
        // Arrange
        var email = "user@example.com";
        var password = "password123";

        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync(It.IsAny<Email>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var command = new EnableMfaCommand(email, password);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DomainErrors.User.InvalidCredentials);
    }

    [Fact]
    public async Task Handle_Should_ReturnFailure_When_MfaIsAlreadyEnabled()
    {
        // Arrange
        var email = "user@example.com";
        var password = "password123";
        var user = User.Create(
            Guid.NewGuid(),
            Email.Create(email).Value,
            "hashedPassword",
            FullName.Create("John Doe").Value);

        user.EnableMfa(RecoveryCode.Create("123456").Value);

        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync(It.IsAny<Email>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _passwordHasherMock
            .Setup(x => x.Verify(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(true);

        var command = new EnableMfaCommand(email, password);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DomainErrors.User.MfaAlreadyEnabled);
    }
}