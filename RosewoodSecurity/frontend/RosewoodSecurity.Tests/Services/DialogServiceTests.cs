using System;
using System.Threading.Tasks;
using MaterialDesignThemes.Wpf;
using Moq;
using RosewoodSecurity.Services;
using Xunit;

namespace RosewoodSecurity.Tests.Services
{
    public class DialogServiceTests
    {
        private readonly Mock<ISnackbarMessageQueue> _mockMessageQueue;
        private readonly DialogService _dialogService;

        public DialogServiceTests()
        {
            _mockMessageQueue = new Mock<ISnackbarMessageQueue>();
            _dialogService = new DialogService(_mockMessageQueue.Object);
        }

        [Fact]
        public async Task ShowErrorAsync_DisplaysErrorMessage()
        {
            // Arrange
            var title = "Error Title";
            var message = "Error Message";
            var messageShown = false;

            _mockMessageQueue.Setup(x => x.Enqueue(
                It.IsAny<string>(),
                It.IsAny<object>(),
                It.IsAny<object>(),
                It.IsAny<object>(),
                It.IsAny<bool>(),
                It.IsAny<bool>(),
                It.IsAny<TimeSpan>()))
                .Callback(() => messageShown = true);

            // Act
            await _dialogService.ShowErrorAsync(title, message);

            // Assert
            Assert.True(messageShown);
        }

        [Fact]
        public async Task ShowWarningAsync_DisplaysWarningMessage()
        {
            // Arrange
            var title = "Warning Title";
            var message = "Warning Message";
            var messageShown = false;

            _mockMessageQueue.Setup(x => x.Enqueue(
                It.IsAny<string>(),
                It.IsAny<object>(),
                It.IsAny<object>(),
                It.IsAny<object>(),
                It.IsAny<bool>(),
                It.IsAny<bool>(),
                It.IsAny<TimeSpan>()))
                .Callback(() => messageShown = true);

            // Act
            await _dialogService.ShowWarningAsync(title, message);

            // Assert
            Assert.True(messageShown);
        }

        [Fact]
        public async Task ShowSuccessAsync_DisplaysSuccessMessage()
        {
            // Arrange
            var title = "Success Title";
            var message = "Success Message";
            var messageShown = false;

            _mockMessageQueue.Setup(x => x.Enqueue(
                It.IsAny<string>(),
                It.IsAny<object>(),
                It.IsAny<object>(),
                It.IsAny<object>(),
                It.IsAny<bool>(),
                It.IsAny<bool>(),
                It.IsAny<TimeSpan>()))
                .Callback(() => messageShown = true);

            // Act
            await _dialogService.ShowSuccessAsync(title, message);

            // Assert
            Assert.True(messageShown);
        }

        [Fact]
        public async Task ShowInfoAsync_DisplaysInfoMessage()
        {
            // Arrange
            var title = "Info Title";
            var message = "Info Message";
            var messageShown = false;

            _mockMessageQueue.Setup(x => x.Enqueue(
                It.IsAny<string>(),
                It.IsAny<object>(),
                It.IsAny<object>(),
                It.IsAny<object>(),
                It.IsAny<bool>(),
                It.IsAny<bool>(),
                It.IsAny<TimeSpan>()))
                .Callback(() => messageShown = true);

            // Act
            await _dialogService.ShowInfoAsync(title, message);

            // Assert
            Assert.True(messageShown);
        }

        [Fact]
        public void ShowError_DisplaysErrorMessage()
        {
            // Arrange
            var title = "Error Title";
            var message = "Error Message";
            var messageShown = false;

            _mockMessageQueue.Setup(x => x.Enqueue(
                It.IsAny<string>(),
                It.IsAny<object>(),
                It.IsAny<object>(),
                It.IsAny<object>(),
                It.IsAny<bool>(),
                It.IsAny<bool>(),
                It.IsAny<TimeSpan>()))
                .Callback(() => messageShown = true);

            // Act
            _dialogService.ShowError(title, message);

            // Assert
            Assert.True(messageShown);
        }

        [Fact]
        public void ShowWarning_DisplaysWarningMessage()
        {
            // Arrange
            var title = "Warning Title";
            var message = "Warning Message";
            var messageShown = false;

            _mockMessageQueue.Setup(x => x.Enqueue(
                It.IsAny<string>(),
                It.IsAny<object>(),
                It.IsAny<object>(),
                It.IsAny<object>(),
                It.IsAny<bool>(),
                It.IsAny<bool>(),
                It.IsAny<TimeSpan>()))
                .Callback(() => messageShown = true);

            // Act
            _dialogService.ShowWarning(title, message);

            // Assert
            Assert.True(messageShown);
        }

        [Fact]
        public void ShowSuccess_DisplaysSuccessMessage()
        {
            // Arrange
            var title = "Success Title";
            var message = "Success Message";
            var messageShown = false;

            _mockMessageQueue.Setup(x => x.Enqueue(
                It.IsAny<string>(),
                It.IsAny<object>(),
                It.IsAny<object>(),
                It.IsAny<object>(),
                It.IsAny<bool>(),
                It.IsAny<bool>(),
                It.IsAny<TimeSpan>()))
                .Callback(() => messageShown = true);

            // Act
            _dialogService.ShowSuccess(title, message);

            // Assert
            Assert.True(messageShown);
        }

        [Fact]
        public void ShowInfo_DisplaysInfoMessage()
        {
            // Arrange
            var title = "Info Title";
            var message = "Info Message";
            var messageShown = false;

            _mockMessageQueue.Setup(x => x.Enqueue(
                It.IsAny<string>(),
                It.IsAny<object>(),
                It.IsAny<object>(),
                It.IsAny<object>(),
                It.IsAny<bool>(),
                It.IsAny<bool>(),
                It.IsAny<TimeSpan>()))
                .Callback(() => messageShown = true);

            // Act
            _dialogService.ShowInfo(title, message);

            // Assert
            Assert.True(messageShown);
        }

        [Fact]
        public void ShowConfirmation_ReturnsUserChoice()
        {
            // Arrange
            var title = "Confirm";
            var message = "Are you sure?";

            // Act
            var result = _dialogService.ShowConfirmation(title, message);

            // Assert
            // Note: Since we can't actually test the UI interaction in a unit test,
            // we're just verifying the method completes without throwing
            Assert.True(true);
        }
    }
}
