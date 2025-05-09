using System;
using System.Threading.Tasks;
using MaterialDesignThemes.Wpf;
using Microsoft.Extensions.Configuration;
using Moq;
using RosewoodSecurity.Services;
using Xunit;

namespace RosewoodSecurity.Tests.Services
{
    public class AuthenticationServiceTests
    {
        private readonly Mock<IApiService> _mockApiService;
        private readonly Mock<ISettingsService> _mockSettingsService;
        private readonly Mock<ISnackbarMessageQueue> _mockMessageQueue;
        private readonly AuthenticationService _authService;

        public AuthenticationServiceTests()
        {
            _mockApiService = new Mock<IApiService>();
            _mockSettingsService = new Mock<ISettingsService>();
            _mockMessageQueue = new Mock<ISnackbarMessageQueue>();

            _authService = new AuthenticationService(
                _mockApiService.Object,
                _mockSettingsService.Object,
                _mockMessageQueue.Object);
        }

        [Fact]
        public async Task InitiateLoginAsync_WithValidCredentials_ReturnsTrue()
        {
            // Arrange
            var username = "testuser";
            var password = "testpass";
            var authResult = new AuthenticationResult
            {
                Success = true,
                User = new UserInfo
                {
                    Id = "1",
                    Username = username,
                    Role = "User"
                },
                Token = "test-token",
                RefreshToken = "test-refresh-token"
            };

            _mockApiService.Setup(x => x.LoginAsync(username, password))
                .ReturnsAsync(authResult);

            // Act
            var result = await _authService.InitiateLoginAsync(username, password);

            // Assert
            Assert.True(result);
            Assert.True(_authService.IsAuthenticated);
            Assert.NotNull(_authService.CurrentUser);
            Assert.Equal(username, _authService.CurrentUser.Username);
        }

        [Fact]
        public async Task InitiateLoginAsync_WithInvalidCredentials_ReturnsFalse()
        {
            // Arrange
            var username = "testuser";
            var password = "wrongpass";
            var authResult = new AuthenticationResult
            {
                Success = false,
                ErrorMessage = "Invalid credentials"
            };

            _mockApiService.Setup(x => x.LoginAsync(username, password))
                .ReturnsAsync(authResult);

            // Act
            var result = await _authService.InitiateLoginAsync(username, password);

            // Assert
            Assert.False(result);
            Assert.False(_authService.IsAuthenticated);
            Assert.Null(_authService.CurrentUser);
        }

        [Fact]
        public async Task ValidateTwoFactorAsync_WithValidCode_ReturnsTrue()
        {
            // Arrange
            var code = "123456";
            _mockApiService.Setup(x => x.ValidateTwoFactorAsync(code))
                .ReturnsAsync(true);

            // First simulate a login that requires 2FA
            var authResult = new AuthenticationResult
            {
                Success = true,
                RequiresTwoFactor = true,
                User = new UserInfo { Id = "1", Username = "testuser" }
            };

            _mockApiService.Setup(x => x.LoginAsync("testuser", "testpass"))
                .ReturnsAsync(authResult);

            await _authService.InitiateLoginAsync("testuser", "testpass");

            // Act
            var result = await _authService.ValidateTwoFactorAsync(code);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void Logout_ClearsAuthenticationState()
        {
            // Arrange
            var authResult = new AuthenticationResult
            {
                Success = true,
                User = new UserInfo { Id = "1", Username = "testuser" },
                Token = "test-token"
            };

            _mockApiService.Setup(x => x.LoginAsync("testuser", "testpass"))
                .ReturnsAsync(authResult);

            _authService.InitiateLoginAsync("testuser", "testpass").Wait();

            // Act
            _authService.Logout();

            // Assert
            Assert.False(_authService.IsAuthenticated);
            Assert.Null(_authService.CurrentUser);
            Assert.Null(_authService.AuthToken);
        }

        [Fact]
        public void HasPermission_WithValidPermission_ReturnsTrue()
        {
            // Arrange
            var authResult = new AuthenticationResult
            {
                Success = true,
                User = new UserInfo
                {
                    Id = "1",
                    Username = "testuser",
                    Permissions = new[] { "read:keys", "write:keys" }
                }
            };

            _mockApiService.Setup(x => x.LoginAsync("testuser", "testpass"))
                .ReturnsAsync(authResult);

            _authService.InitiateLoginAsync("testuser", "testpass").Wait();

            // Act
            var result = _authService.HasPermission("read:keys");

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void HasPermission_WithInvalidPermission_ReturnsFalse()
        {
            // Arrange
            var authResult = new AuthenticationResult
            {
                Success = true,
                User = new UserInfo
                {
                    Id = "1",
                    Username = "testuser",
                    Permissions = new[] { "read:keys" }
                }
            };

            _mockApiService.Setup(x => x.LoginAsync("testuser", "testpass"))
                .ReturnsAsync(authResult);

            _authService.InitiateLoginAsync("testuser", "testpass").Wait();

            // Act
            var result = _authService.HasPermission("write:keys");

            // Assert
            Assert.False(result);
        }
    }
}
