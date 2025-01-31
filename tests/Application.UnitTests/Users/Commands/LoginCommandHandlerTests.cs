using Application.Abstractions.Security;
using Application.Users.Commands.Login;
using Domain.Entities;
using Domain.Errors;
using Domain.Repositories;
using Domain.ValueObjects;
using FluentAssertions;
using Moq;

namespace Application.UnitTests.Users.Commands;

public class LoginCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IJwtProvider> _jwtProviderMock;
    private readonly Mock<IPasswordHasher> _passwordHasherMock;
    private readonly LoginCommandHandler _handler;

    public LoginCommandHandlerTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _jwtProviderMock = new Mock<IJwtProvider>();
        _passwordHasherMock = new Mock<IPasswordHasher>();
        _handler = new LoginCommandHandler(
            _userRepositoryMock.Object,
            _jwtProviderMock.Object,
            _passwordHasherMock.Object);
    }

    [Fact]
    public async Task Handle_Should_ReturnSuccess_When_CredentialsAreValid()
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

        _jwtProviderMock
            .Setup(x => x.Generate(It.IsAny<User>()))
            .Returns("jwtToken");

        var command = new LoginCommand(email, password);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("jwtToken");
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

        var command = new LoginCommand(email, password);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DomainErrors.User.InvalidCredentials);
    }

    [Fact]
    public async Task Handle_Should_ReturnFailure_When_MfaIsEnabled()
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

        var command = new LoginCommand(email, password);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DomainErrors.User.MfaLoginRequired);
    }
}