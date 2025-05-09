using System;
using System.Threading.Tasks;
using Moq;
using RosewoodSecurity.Tests.TestHelpers;
using RosewoodSecurity.ViewModels;
using Xunit;

namespace RosewoodSecurity.Tests.ViewModels
{
    public class DashboardViewModelTests : TestBase
    {
        private readonly DashboardViewModel _viewModel;

        public DashboardViewModelTests()
        {
            _viewModel = new DashboardViewModel(
                MockApiService.Object,
                MockAuthenticationService.Object,
                MockNavigationService.Object,
                MockDialogService.Object,
                MockSettingsService.Object);

            // Setup default authenticated user
            MockAuthenticationService
                .Setup(x => x.IsAuthenticated)
                .Returns(true);

            MockAuthenticationService
                .Setup(x => x.CurrentUser)
                .Returns(TestData.CreateTestUser());
        }

        [Fact]
        public void Constructor_InitializesProperties()
        {
            // Assert
            Assert.NotNull(_viewModel.RefreshCommand);
            Assert.NotNull(_viewModel.NavigateToCheckInOutCommand);
            Assert.False(_viewModel.IsBusy);
            Assert.NotNull(_viewModel.Title);
        }

        [Fact]
        public async Task LoadData_PopulatesDashboardData()
        {
            // Arrange
            var testData = TestData.CreateTestDashboardData();
            MockApiService
                .Setup(x => x.GetDashboardDataAsync())
                .ReturnsAsync(testData);

            // Act
            await _viewModel.LoadDataAsync();

            // Assert
            Assert.Equal(testData.TotalKeysOut, _viewModel.TotalKeysOut);
            Assert.Equal(testData.TotalAccessCardsOut, _viewModel.TotalAccessCardsOut);
            Assert.Equal(testData.OverdueItemsCount, _viewModel.OverdueItemsCount);
            Assert.Equal(testData.ActiveUsersCount, _viewModel.ActiveUsersCount);
            Assert.Equal(testData.DepartmentUsage.Count, _viewModel.DepartmentUsage.Count);
            Assert.Equal(testData.RecentActivities.Count, _viewModel.RecentActivities.Count);
        }

        [Fact]
        public async Task LoadData_OnError_ShowsErrorDialog()
        {
            // Arrange
            MockApiService
                .Setup(x => x.GetDashboardDataAsync())
                .ThrowsAsync(new Exception("Test error"));

            // Act
            await _viewModel.LoadDataAsync();

            // Assert
            MockDialogService.Verify(x => x.ShowErrorAsync(
                It.IsAny<string>(),
                It.IsAny<string>()),
                Times.Once);
        }

        [Fact]
        public async Task RefreshCommand_UpdatesDashboardData()
        {
            // Arrange
            var testData = TestData.CreateTestDashboardData();
            MockApiService
                .Setup(x => x.GetDashboardDataAsync())
                .ReturnsAsync(testData);

            // Act
            await _viewModel.RefreshCommand.ExecuteAsync(null);

            // Assert
            Assert.Equal(testData.TotalKeysOut, _viewModel.TotalKeysOut);
            Assert.False(_viewModel.IsBusy);
            MockApiService.Verify(x => x.GetDashboardDataAsync(), Times.Once);
        }

        [Fact]
        public async Task NavigateToCheckInOutCommand_NavigatesToCheckInOutView()
        {
            // Act
            await _viewModel.NavigateToCheckInOutCommand.ExecuteAsync(null);

            // Assert
            MockNavigationService.Verify(x => x.NavigateTo("CheckInOut"), Times.Once);
        }

        [Fact]
        public void AutoRefresh_WhenEnabled_UpdatesDashboardPeriodically()
        {
            // Arrange
            MockSettingsService
                .Setup(x => x.EnableAutoRefresh)
                .Returns(true);

            MockSettingsService
                .Setup(x => x.AutoRefreshInterval)
                .Returns(30);

            // Act
            _viewModel.StartAutoRefresh();

            // Assert - Verify that the timer is started
            Assert.True(_viewModel.IsAutoRefreshEnabled);
        }

        [Fact]
        public void AutoRefresh_WhenDisabled_StopsUpdating()
        {
            // Arrange
            MockSettingsService
                .Setup(x => x.EnableAutoRefresh)
                .Returns(false);

            // Act
            _viewModel.StartAutoRefresh();

            // Assert
            Assert.False(_viewModel.IsAutoRefreshEnabled);
        }

        [Fact]
        public void Dispose_CleansUpResources()
        {
            // Act
            _viewModel.Dispose();

            // Assert - Verify that the timer is stopped
            Assert.False(_viewModel.IsAutoRefreshEnabled);
        }

        [Fact]
        public async Task LoadData_UpdatesLastRefreshTime()
        {
            // Arrange
            var testData = TestData.CreateTestDashboardData();
            MockApiService
                .Setup(x => x.GetDashboardDataAsync())
                .ReturnsAsync(testData);

            var beforeLoad = DateTime.Now;

            // Act
            await _viewModel.LoadDataAsync();

            // Assert
            Assert.True(_viewModel.LastRefreshTime >= beforeLoad);
        }

        [Fact]
        public void HasOverdueItems_ReturnsCorrectValue()
        {
            // Arrange
            _viewModel.OverdueItemsCount = 5;

            // Act & Assert
            Assert.True(_viewModel.HasOverdueItems);

            // Arrange
            _viewModel.OverdueItemsCount = 0;

            // Act & Assert
            Assert.False(_viewModel.HasOverdueItems);
        }

        [Fact]
        public void DepartmentUsagePercentage_CalculatesCorrectly()
        {
            // Arrange
            var testData = TestData.CreateTestDashboardData();
            MockApiService
                .Setup(x => x.GetDashboardDataAsync())
                .ReturnsAsync(testData);

            // Act
            _viewModel.LoadDataAsync().Wait();

            // Assert
            var totalPercentage = 0.0;
            foreach (var dept in _viewModel.DepartmentUsage)
            {
                totalPercentage += dept.UsagePercentage;
            }
            Assert.Equal(70.0, totalPercentage); // Based on test data (40% + 30%)
        }

        [Fact]
        public void UserHasAdminAccess_ChecksPermissionsCorrectly()
        {
            // Arrange - Admin user
            var adminUser = TestData.CreateTestUser(role: "Admin", permissions: new[] { "admin:access" });
            MockAuthenticationService
                .Setup(x => x.CurrentUser)
                .Returns(adminUser);

            // Act & Assert
            Assert.True(_viewModel.UserHasAdminAccess);

            // Arrange - Regular user
            var regularUser = TestData.CreateTestUser(role: "User", permissions: new[] { "read:keys" });
            MockAuthenticationService
                .Setup(x => x.CurrentUser)
                .Returns(regularUser);

            // Act & Assert
            Assert.False(_viewModel.UserHasAdminAccess);
        }
    }
}
