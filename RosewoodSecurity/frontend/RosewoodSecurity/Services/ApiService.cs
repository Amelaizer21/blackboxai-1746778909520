using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using RosewoodSecurity.Models;

namespace RosewoodSecurity.Services
{
    public class ApiService : IApiService, IDisposable
    {
        private readonly HttpClient _httpClient;
        private readonly ISettingsService _settingsService;
        private readonly IDialogService _dialogService;
        private readonly JsonSerializerOptions _jsonOptions;

        public ApiService(
            IConfiguration configuration,
            ISettingsService settingsService,
            IDialogService dialogService)
        {
            _settingsService = settingsService;
            _dialogService = dialogService;

            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(_settingsService.ApiBaseUrl),
                Timeout = TimeSpan.FromSeconds(_settingsService.ApiTimeout)
            };

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        }

        public async Task<AuthenticationResult> LoginAsync(string username, string password)
        {
            try
            {
                var response = await PostAsync<AuthenticationResult>("api/auth/login", new
                {
                    username,
                    password
                });

                return response;
            }
            catch (Exception ex)
            {
                await HandleApiError(ex, "Login failed");
                return new AuthenticationResult { Success = false, ErrorMessage = ex.Message };
            }
        }

        public async Task<bool> ValidateTwoFactorAsync(string code)
        {
            try
            {
                var response = await PostAsync<AuthenticationResult>("api/auth/validate-2fa", new { code });
                return response.Success;
            }
            catch (Exception ex)
            {
                await HandleApiError(ex, "2FA validation failed");
                return false;
            }
        }

        public async Task<DashboardData> GetDashboardDataAsync()
        {
            try
            {
                return await GetAsync<DashboardData>("api/dashboard");
            }
            catch (Exception ex)
            {
                await HandleApiError(ex, "Failed to load dashboard data");
                return null;
            }
        }

        public async Task<EmployeeInfo> GetEmployeeByIdAsync(string employeeId)
        {
            try
            {
                return await GetAsync<EmployeeInfo>($"api/employees/{employeeId}");
            }
            catch (Exception ex)
            {
                await HandleApiError(ex, "Failed to load employee information");
                return null;
            }
        }

        public async Task<List<InventoryItem>> GetAvailableItemsAsync(string itemType)
        {
            try
            {
                return await GetAsync<List<InventoryItem>>($"api/inventory/available?type={itemType}");
            }
            catch (Exception ex)
            {
                await HandleApiError(ex, "Failed to load available items");
                return new List<InventoryItem>();
            }
        }

        public async Task<List<ActiveTransaction>> GetActiveTransactionsAsync()
        {
            try
            {
                return await GetAsync<List<ActiveTransaction>>("api/transactions/active");
            }
            catch (Exception ex)
            {
                await HandleApiError(ex, "Failed to load active transactions");
                return new List<ActiveTransaction>();
            }
        }

        public async Task<TransactionResult> CheckOutItemAsync(CheckOutRequest request)
        {
            try
            {
                return await PostAsync<TransactionResult>("api/transactions/checkout", request);
            }
            catch (Exception ex)
            {
                await HandleApiError(ex, "Failed to check out item");
                return new TransactionResult { Success = false, Message = ex.Message };
            }
        }

        public async Task<TransactionResult> CheckInItemAsync(string transactionId)
        {
            try
            {
                return await PostAsync<TransactionResult>($"api/transactions/{transactionId}/checkin", null);
            }
            catch (Exception ex)
            {
                await HandleApiError(ex, "Failed to check in item");
                return new TransactionResult { Success = false, Message = ex.Message };
            }
        }

        public async Task<bool> CheckScannerStatusAsync()
        {
            try
            {
                var response = await GetAsync<dynamic>("api/system/scanner/status");
                return response.isConnected;
            }
            catch
            {
                return false;
            }
        }

        private async Task<T> GetAsync<T>(string endpoint)
        {
            var response = await _httpClient.GetAsync(endpoint);
            await EnsureSuccessStatusCode(response);
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(content, _jsonOptions);
        }

        private async Task<T> PostAsync<T>(string endpoint, object data)
        {
            var content = new StringContent(
                JsonSerializer.Serialize(data, _jsonOptions),
                Encoding.UTF8,
                "application/json");

            var response = await _httpClient.PostAsync(endpoint, content);
            await EnsureSuccessStatusCode(response);
            var responseContent = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(responseContent, _jsonOptions);
        }

        private async Task EnsureSuccessStatusCode(HttpResponseMessage response)
        {
            if (!response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var error = JsonSerializer.Deserialize<ApiError>(content, _jsonOptions);
                throw new ApiException(error?.Message ?? "An error occurred", response.StatusCode);
            }
        }

        private async Task HandleApiError(Exception ex, string defaultMessage)
        {
            var message = ex is ApiException apiEx ? apiEx.Message : defaultMessage;
            await _dialogService.ShowErrorAsync("API Error", message);
        }

        public void SetAuthToken(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = null;
            }
            else
            {
                _httpClient.DefaultRequestHeaders.Authorization = 
                    new AuthenticationHeaderValue("Bearer", token);
            }
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }

    public class ApiException : Exception
    {
        public System.Net.HttpStatusCode StatusCode { get; }

        public ApiException(string message, System.Net.HttpStatusCode statusCode) 
            : base(message)
        {
            StatusCode = statusCode;
        }
    }

    public class ApiError
    {
        public string Message { get; set; }
        public string Details { get; set; }
    }
}
