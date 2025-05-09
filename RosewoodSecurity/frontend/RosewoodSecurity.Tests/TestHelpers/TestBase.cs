using System;
using MaterialDesignThemes.Wpf;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using RosewoodSecurity.Services;

namespace RosewoodSecurity.Tests.TestHelpers
{
    public abstract class TestBase : IDisposable
    {
        protected readonly IServiceProvider ServiceProvider;
        protected readonly Mock<IConfiguration> MockConfiguration;
        protected readonly Mock<ISettingsService> MockSettingsService;
        protected readonly Mock<IDialogService> MockDialogService;
        protected readonly Mock<INavigationService> MockNavigationService;
        protected readonly Mock<IAuthenticationService> MockAuthenticationService;
        protected readonly Mock<IApiService> MockApiService;
        protected readonly Mock<IThemeService> MockThemeService;
        protected readonly Mock<ISnackbarMessageQueue> MockMessageQueue;

        protected TestBase()
        {
            MockConfiguration = new Mock<IConfiguration>();
            MockSettingsService = new Mock<ISettingsService>();
            MockDialogService = new Mock<IDialogService>();
            MockNavigationService = new Mock<INavigationService>();
            MockAuthenticationService = new Mock<IAuthenticationService>();
            MockApiService = new Mock<IApiService>();
            MockThemeService = new Mock<IThemeService>();
            MockMessageQueue = new Mock<ISnackbarMessageQueue>();

            var services = new ServiceCollection();

            // Register mocks
            services.AddSingleton(MockConfiguration.Object);
            services.AddSingleton(MockSettingsService.Object);
            services.AddSingleton(MockDialogService.Object);
            services.AddSingleton(MockNavigationService.Object);
            services.AddSingleton(MockAuthenticationService.Object);
            services.AddSingleton(MockApiService.Object);
            services.AddSingleton(MockThemeService.Object);
            services.AddSingleton(MockMessageQueue.Object);

            ServiceProvider = services.BuildServiceProvider();

            SetupDefaultMocks();
        }

        protected virtual void SetupDefaultMocks()
        {
            // Setup default configuration values
            MockConfiguration.Setup(x => x["Api:BaseUrl"])
                .Returns("http://localhost:5000");

            // Setup default settings
            MockSettingsService.Setup(x => x.ApiBaseUrl)
                .Returns("http://localhost:5000");
            MockSettingsService.Setup(x => x.ApiTimeout)
                .Returns(30);

            // Setup default authentication state
            MockAuthenticationService.Setup(x => x.IsAuthenticated)
                .Returns(false);
            MockAuthenticationService.Setup(x => x.CurrentUser)
                .Returns((UserInfo)null);

            // Setup default theme
            MockThemeService.Setup(x => x.IsDarkTheme)
                .Returns(false);
        }

        protected T GetRequiredService<T>() where T : class
        {
            return ServiceProvider.GetRequiredService<T>();
        }

        protected Mock<T> GetMock<T>() where T : class
        {
            if (typeof(T) == typeof(IConfiguration)) return MockConfiguration as Mock<T>;
            if (typeof(T) == typeof(ISettingsService)) return MockSettingsService as Mock<T>;
            if (typeof(T) == typeof(IDialogService)) return MockDialogService as Mock<T>;
            if (typeof(T) == typeof(INavigationService)) return MockNavigationService as Mock<T>;
            if (typeof(T) == typeof(IAuthenticationService)) return MockAuthenticationService as Mock<T>;
            if (typeof(T) == typeof(IApiService)) return MockApiService as Mock<T>;
            if (typeof(T) == typeof(IThemeService)) return MockThemeService as Mock<T>;
            if (typeof(T) == typeof(ISnackbarMessageQueue)) return MockMessageQueue as Mock<T>;

            throw new ArgumentException($"Mock not found for type {typeof(T).Name}");
        }

        protected void VerifyMockCalled<T>(Action<Mock<T>> verifyAction) where T : class
        {
            var mock = GetMock<T>();
            verifyAction(mock);
        }

        public virtual void Dispose()
        {
            if (ServiceProvider is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }

    public static class TestData
    {
        public static UserInfo CreateTestUser(
            string id = "1",
            string username = "testuser",
            string role = "User",
            string[] permissions = null)
        {
            return new UserInfo
            {
                Id = id,
                Username = username,
                Role = role,
                Permissions = permissions ?? new[] { "read:keys", "write:keys" },
                IsActive = true,
                LastLogin = DateTime.UtcNow
            };
        }

        public static AuthenticationResult CreateSuccessfulAuthResult(UserInfo user = null)
        {
            return new AuthenticationResult
            {
                Success = true,
                User = user ?? CreateTestUser(),
                Token = "test-token",
                RefreshToken = "test-refresh-token",
                TokenExpiration = DateTime.UtcNow.AddHours(1)
            };
        }

        public static AuthenticationResult CreateFailedAuthResult(string errorMessage = "Invalid credentials")
        {
            return new AuthenticationResult
            {
                Success = false,
                ErrorMessage = errorMessage
            };
        }

        public static DashboardData CreateTestDashboardData()
        {
            return new DashboardData
            {
                TotalKeysOut = 5,
                TotalAccessCardsOut = 3,
                OverdueItemsCount = 1,
                ActiveUsersCount = 10,
                DepartmentUsage = new List<DepartmentUsage>
                {
                    new DepartmentUsage { DepartmentName = "IT", UsagePercentage = 40 },
                    new DepartmentUsage { DepartmentName = "HR", UsagePercentage = 30 }
                },
                RecentActivities = new List<RecentActivity>
                {
                    new RecentActivity
                    {
                        Description = "Key checkout",
                        Timestamp = DateTime.UtcNow,
                        Type = "Checkout"
                    }
                }
            };
        }
    }
}
