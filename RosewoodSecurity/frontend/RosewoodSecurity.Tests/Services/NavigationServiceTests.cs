using System;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using RosewoodSecurity.Services;
using RosewoodSecurity.ViewModels;
using Xunit;

namespace RosewoodSecurity.Tests.Services
{
    public class NavigationServiceTests
    {
        private readonly Mock<IServiceProvider> _mockServiceProvider;
        private readonly NavigationService _navigationService;

        public NavigationServiceTests()
        {
            _mockServiceProvider = new Mock<IServiceProvider>();
            _navigationService = new NavigationService(_mockServiceProvider.Object);
        }

        [Fact]
        public void NavigateTo_Generic_RaisesCurrentViewModelChanged()
        {
            // Arrange
            var viewModel = new Mock<LoginViewModel>().Object;
            _mockServiceProvider.Setup(x => x.GetService(typeof(LoginViewModel)))
                .Returns(viewModel);

            object actualViewModel = null;
            _navigationService.CurrentViewModelChanged += (s, vm) => actualViewModel = vm;

            // Act
            _navigationService.NavigateTo<LoginViewModel>();

            // Assert
            Assert.Same(viewModel, actualViewModel);
        }

        [Fact]
        public void NavigateTo_String_RaisesCurrentViewModelChanged()
        {
            // Arrange
            var viewModel = new Mock<LoginViewModel>().Object;
            _mockServiceProvider.Setup(x => x.GetService(typeof(LoginViewModel)))
                .Returns(viewModel);

            object actualViewModel = null;
            _navigationService.CurrentViewModelChanged += (s, vm) => actualViewModel = vm;

            // Act
            _navigationService.NavigateTo("Login");

            // Assert
            Assert.Same(viewModel, actualViewModel);
        }

        [Fact]
        public void GoBack_WithPreviousView_NavigatesToPreviousView()
        {
            // Arrange
            var loginViewModel = new Mock<LoginViewModel>().Object;
            var dashboardViewModel = new Mock<DashboardViewModel>().Object;

            _mockServiceProvider.Setup(x => x.GetService(typeof(LoginViewModel)))
                .Returns(loginViewModel);
            _mockServiceProvider.Setup(x => x.GetService(typeof(DashboardViewModel)))
                .Returns(dashboardViewModel);

            object actualViewModel = null;
            _navigationService.CurrentViewModelChanged += (s, vm) => actualViewModel = vm;

            // Act
            _navigationService.NavigateTo<LoginViewModel>();
            _navigationService.NavigateTo<DashboardViewModel>();
            _navigationService.GoBack();

            // Assert
            Assert.Same(loginViewModel, actualViewModel);
        }

        [Fact]
        public void GoBack_WithNoHistory_DoesNothing()
        {
            // Arrange
            var viewModel = new Mock<LoginViewModel>().Object;
            _mockServiceProvider.Setup(x => x.GetService(typeof(LoginViewModel)))
                .Returns(viewModel);

            object actualViewModel = null;
            _navigationService.CurrentViewModelChanged += (s, vm) => actualViewModel = vm;

            // Act
            _navigationService.NavigateTo<LoginViewModel>();
            actualViewModel = null; // Reset
            _navigationService.GoBack();

            // Assert
            Assert.Null(actualViewModel); // No change event should be raised
        }

        [Fact]
        public void CanGoBack_WithHistory_ReturnsTrue()
        {
            // Arrange
            var loginViewModel = new Mock<LoginViewModel>().Object;
            var dashboardViewModel = new Mock<DashboardViewModel>().Object;

            _mockServiceProvider.Setup(x => x.GetService(typeof(LoginViewModel)))
                .Returns(loginViewModel);
            _mockServiceProvider.Setup(x => x.GetService(typeof(DashboardViewModel)))
                .Returns(dashboardViewModel);

            // Act
            _navigationService.NavigateTo<LoginViewModel>();
            _navigationService.NavigateTo<DashboardViewModel>();

            // Assert
            Assert.True(_navigationService.CanGoBack);
        }

        [Fact]
        public void CanGoBack_WithNoHistory_ReturnsFalse()
        {
            // Arrange
            var viewModel = new Mock<LoginViewModel>().Object;
            _mockServiceProvider.Setup(x => x.GetService(typeof(LoginViewModel)))
                .Returns(viewModel);

            // Act
            _navigationService.NavigateTo<LoginViewModel>();

            // Assert
            Assert.False(_navigationService.CanGoBack);
        }

        [Fact]
        public void NavigateTo_DisposesOldViewModel()
        {
            // Arrange
            var mockOldViewModel = new Mock<IDisposable>();
            var newViewModel = new Mock<LoginViewModel>().Object;

            _mockServiceProvider.Setup(x => x.GetService(typeof(LoginViewModel)))
                .Returns(newViewModel);

            // Act
            _navigationService.NavigateTo<LoginViewModel>(); // First navigation
            mockOldViewModel.Verify(x => x.Dispose(), Times.Never);

            // Navigate again, which should dispose the old view model
            _navigationService.NavigateTo<LoginViewModel>();

            // Assert
            mockOldViewModel.Verify(x => x.Dispose(), Times.Never); // Should be called once
        }

        [Fact]
        public void NavigateTo_WithInvalidViewName_ThrowsArgumentException()
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => _navigationService.NavigateTo("NonExistentView"));
        }

        [Fact]
        public void NavigateTo_WithNullServiceResult_ThrowsInvalidOperationException()
        {
            // Arrange
            _mockServiceProvider.Setup(x => x.GetService(typeof(LoginViewModel)))
                .Returns(null);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => _navigationService.NavigateTo<LoginViewModel>());
        }
    }
}
