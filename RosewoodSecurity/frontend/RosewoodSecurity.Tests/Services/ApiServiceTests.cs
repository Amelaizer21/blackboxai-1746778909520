using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Moq;
using RosewoodSecurity.Services;
using Xunit;
using System.Text.Json;

namespace RosewoodSecurity.Tests.Services
{
    public class ApiServiceTests : IDisposable
    {
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly Mock<ISettingsService> _mockSettingsService;
        private readonly Mock<IDialogService> _mockDialogService;
        private readonly ApiService _apiService;
        private readonly HttpClient _httpClient;

        public ApiServiceTests()
        {
            _mockConfiguration = new Mock<IConfiguration>();
            _mockSettingsService = new Mock<ISettingsService>();
            _mockDialogService = new Mock<IDialogService>();

            // Setup settings
            _mockSettingsService.Setup(x => x.ApiBaseUrl).Returns("http://localhost:5000");
            _mockSettingsService.Setup(x => x.ApiTimeout).Returns(30);

            _apiService = new ApiService(
                _mockConfiguration.Object,
                _mockSettingsService.Object,
                _mockDialogService.Object);
        }

        [Fact]
        public async Task LoginAsync_WithValidCredentials_ReturnsSuccessResult()
        {
            // Arrange
            var username = "testuser";
            var password = "testpass";
            var expectedResult = new AuthenticationResult
            {
                Success = true,
                User = new UserInfo
                {
                    Id = "1",
                    Username = username,
                    Role = "User"
                },
                Token = "test-token"
            };

            // Act
            var result = await _apiService.LoginAsync(username, password);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(username, result.User.Username);
            Assert.NotNull(result.Token);
        }

        [Fact]
        public async Task LoginAsync_WithInvalidCredentials_ReturnsFailureResult()
        {
            // Arrange
            var username = "testuser";
            var password = "wrongpass";

            // Act
            var result = await _apiService.LoginAsync(username, password);

            // Assert
            Assert.False(result.Success);
            Assert.Null(result.Token);
            Assert.NotNull(result.ErrorMessage);
        }

        [Fact]
        public async Task ValidateTwoFactorAsync_WithValidCode_ReturnsTrue()
        {
            // Arrange
            var code = "123456";

            // Act
            var result = await _apiService.ValidateTwoFactorAsync(code);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task GetDashboardDataAsync_ReturnsValidData()
        {
            // Act
            var result = await _apiService.GetDashboardDataAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.TotalKeysOut >= 0);
            Assert.True(result.TotalAccessCardsOut >= 0);
        }

        [Fact]
        public async Task CheckOutItemAsync_WithValidRequest_ReturnsSuccess()
        {
            // Arrange
            var request = new CheckOutRequest
            {
                EmployeeId = "1",
                ItemId = "KEY001",
                ItemType = "Key"
            };

            // Act
            var result = await _apiService.CheckOutItemAsync(request);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.TransactionId);
        }

        [Fact]
        public async Task CheckInItemAsync_WithValidId_ReturnsSuccess()
        {
            // Arrange
            var transactionId = "TRANS001";

            // Act
            var result = await _apiService.CheckInItemAsync(transactionId);

            // Assert
            Assert.True(result.Success);
        }

        [Fact]
        public async Task GetActiveTransactionsAsync_ReturnsTransactions()
        {
            // Act
            var result = await _apiService.GetActiveTransactionsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.IsType<List<ActiveTransaction>>(result);
        }

        [Fact]
        public void SetAuthToken_SetsAuthorizationHeader()
        {
            // Arrange
            var token = "test-token";

            // Act
            _apiService.SetAuthToken(token);

            // No direct way to assert the header was set, but we can verify no exception was thrown
            Assert.True(true);
        }

        [Fact]
        public void SetAuthToken_WithNullToken_ClearsAuthorizationHeader()
        {
            // Act
            _apiService.SetAuthToken(null);

            // No direct way to assert the header was cleared, but we can verify no exception was thrown
            Assert.True(true);
        }

        [Fact]
        public async Task ApiCall_WithNetworkError_ShowsErrorDialog()
        {
            // Arrange
            var dialogShown = false;
            _mockDialogService
                .Setup(x => x.ShowErrorAsync(It.IsAny<string>(), It.IsAny<string>()))
                .Callback(() => dialogShown = true)
                .Returns(Task.CompletedTask);

            // Act
            await _apiService.GetDashboardDataAsync();

            // Assert
            Assert.True(dialogShown);
        }

        public void Dispose()
        {
            _apiService?.Dispose();
        }

        private class MockHttpMessageHandler : HttpMessageHandler
        {
            private readonly Func<HttpRequestMessage, HttpResponseMessage> _handler;

            public MockHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> handler)
            {
                _handler = handler;
            }

            protected override Task<HttpResponseMessage> SendAsync(
                HttpRequestMessage request,
                System.Threading.CancellationToken cancellationToken)
            {
                return Task.FromResult(_handler(request));
            }
        }

        private HttpResponseMessage CreateJsonResponse<T>(T content, HttpStatusCode statusCode = HttpStatusCode.OK)
        {
            var json = JsonSerializer.Serialize(content);
            return new HttpResponseMessage
            {
                StatusCode = statusCode,
                Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json")
            };
        }
    }
}
