using System;
using System.Threading.Tasks;
using Moq;
using RosewoodSecurity.Tests.TestHelpers;
using RosewoodSecurity.ViewModels;
using Xunit;

namespace RosewoodSecurity.Tests.ViewModels
{
    public class LoginViewModelTests : TestBase
    {
        private readonly LoginViewModel _viewModel;

        public LoginViewModelTests()
        {
            _viewModel = new LoginViewModel(
                MockAuthenticationService.Object,
                MockNavigationService.Object,
                MockDialogService.Object);
        }

        [Fact]
        public void Constructor_InitializesProperties()
        {
            // Assert
            Assert.NotNull(_viewModel.LoginCommand);
            Assert.False(_viewModel.IsBusy);
            Assert.False(_viewModel.IsLoggedIn);
            Assert.False(_viewModel.RequiresTwoFactor);
            Assert.Equal(string.Empty, _viewModel.Username);
            Assert.Equal(string.Empty, _viewModel.Password);
            Assert.Equal(string.Empty, _viewModel.TwoFactorCode);
        }

        [Fact]
        public async Task LoginCommand_WithValidCredentials_NavigatesToDashboard()
        {
            // Arrange
            _viewModel.Username = "testuser";
            _viewModel.Password = "testpass";

            MockAuthenticationService
                .Setup(x => x.InitiateLoginAsync("testuser", "testpass"))
                .ReturnsAsync(true);

            MockAuthenticationService
                .Setup(x => x.IsAuthenticated)
                .Returns(true);

            // Act
            await _viewModel.LoginCommand.ExecuteAsync(null);

            // Assert
            MockNavigationService.Verify(x => x.NavigateTo("Dashboard"), Times.Once);
            Assert.True(_viewModel.IsLoggedIn);
            Assert.False(_viewModel.IsBusy);
        }

        [Fact]
        public async Task LoginCommand_WithInvalidCredentials_ShowsError()
        {
            // Arrange
            _viewModel.Username = "testuser";
            _viewModel.Password = "wrongpass";

            MockAuthenticationService
                .Setup(x => x.InitiateLoginAsync("testuser", "wrongpass"))
                .ReturnsAsync(false);

            // Act
            await _viewModel.LoginCommand.ExecuteAsync(null);

            // Assert
            MockDialogService.Verify(x => x.ShowErrorAsync(
                It.IsAny<string>(),
                It.IsAny<string>()),
                Times.Once);
            Assert.False(_viewModel.IsLoggedIn);
            Assert.False(_viewModel.IsBusy);
        }

        [Fact]
        public async Task LoginCommand_WithTwoFactorRequired_ShowsTwoFactorInput()
        {
            // Arrange
            _viewModel.Username = "testuser";
            _viewModel.Password = "testpass";

            MockAuthenticationService
                .Setup(x => x.InitiateLoginAsync("testuser", "testpass"))
                .ReturnsAsync(true);

            MockAuthenticationService
                .Setup(x => x.IsAuthenticated)
                .Returns(false);

            // Act
            await _viewModel.LoginCommand.ExecuteAsync(null);

            // Assert
            Assert.True(_viewModel.RequiresTwoFactor);
            Assert.False(_viewModel.IsLoggedIn);
            Assert.False(_viewModel.IsBusy);
        }

        [Fact]
        public async Task ValidateTwoFactorCommand_WithValidCode_NavigatesToDashboard()
        {
            // Arrange
            _viewModel.TwoFactorCode = "123456";

            MockAuthenticationService
                .Setup(x => x.ValidateTwoFactorAsync("123456"))
                .ReturnsAsync(true);

            MockAuthenticationService
                .Setup(x => x.IsAuthenticated)
                .Returns(true);

            // Act
            await _viewModel.ValidateTwoFactorCommand.ExecuteAsync(null);

            // Assert
            MockNavigationService.Verify(x => x.NavigateTo("Dashboard"), Times.Once);
            Assert.True(_viewModel.IsLoggedIn);
            Assert.False(_viewModel.IsBusy);
            Assert.False(_viewModel.RequiresTwoFactor);
        }

        [Fact]
        public async Task ValidateTwoFactorCommand_WithInvalidCode_ShowsError()
        {
            // Arrange
            _viewModel.TwoFactorCode = "000000";

            MockAuthenticationService
                .Setup(x => x.ValidateTwoFactorAsync("000000"))
                .ReturnsAsync(false);

            // Act
            await _viewModel.ValidateTwoFactorCommand.ExecuteAsync(null);

            // Assert
            MockDialogService.Verify(x => x.ShowErrorAsync(
                It.IsAny<string>(),
                It.IsAny<string>()),
                Times.Once);
            Assert.False(_viewModel.IsLoggedIn);
            Assert.False(_viewModel.IsBusy);
            Assert.True(_viewModel.RequiresTwoFactor);
        }

        [Fact]
        public void CanExecuteLoginCommand_WithEmptyCredentials_ReturnsFalse()
        {
            // Arrange
            _viewModel.Username = string.Empty;
            _viewModel.Password = string.Empty;

            // Act
            var canExecute = _viewModel.LoginCommand.CanExecute(null);

            // Assert
            Assert.False(canExecute);
        }

        [Fact]
        public void CanExecuteLoginCommand_WithValidCredentials_ReturnsTrue()
        {
            // Arrange
            _viewModel.Username = "testuser";
            _viewModel.Password = "testpass";

            // Act
            var canExecute = _viewModel.LoginCommand.CanExecute(null);

            // Assert
            Assert.True(canExecute);
        }

        [Fact]
        public void CanExecuteValidateTwoFactorCommand_WithEmptyCode_ReturnsFalse()
        {
            // Arrange
            _viewModel.TwoFactorCode = string.Empty;

            // Act
            var canExecute = _viewModel.ValidateTwoFactorCommand.CanExecute(null);

            // Assert
            Assert.False(canExecute);
        }

        [Fact]
        public void CanExecuteValidateTwoFactorCommand_WithValidCode_ReturnsTrue()
        {
            // Arrange
            _viewModel.TwoFactorCode = "123456";

            // Act
            var canExecute = _viewModel.ValidateTwoFactorCommand.CanExecute(null);

            // Assert
            Assert.True(canExecute);
        }

        [Fact]
        public void PropertyChanged_RaisesCanExecuteChanged()
        {
            // Arrange
            var loginCommandCanExecuteChanged = false;
            var validateCommandCanExecuteChanged = false;

            _viewModel.LoginCommand.CanExecuteChanged += (s, e) => loginCommandCanExecuteChanged = true;
            _viewModel.ValidateTwoFactorCommand.CanExecuteChanged += (s, e) => validateCommandCanExecuteChanged = true;

            // Act
            _viewModel.Username = "testuser";
            _viewModel.Password = "testpass";
            _viewModel.TwoFactorCode = "123456";

            // Assert
            Assert.True(loginCommandCanExecuteChanged);
            Assert.True(validateCommandCanExecuteChanged);
        }
    }
}
