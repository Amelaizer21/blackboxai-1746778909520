using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace RosewoodSecurity.Models
{
    public class AppConfiguration
    {
        [JsonPropertyName("app_settings")]
        public AppSettings AppSettings { get; set; }

        [JsonPropertyName("theme")]
        public ThemeSettings Theme { get; set; }

        [JsonPropertyName("logging")]
        public LoggingSettings Logging { get; set; }

        [JsonPropertyName("security")]
        public SecuritySettings Security { get; set; }

        [JsonPropertyName("features")]
        public FeatureSettings Features { get; set; }

        [JsonPropertyName("api")]
        public ApiSettings Api { get; set; }

        [JsonPropertyName("ui")]
        public UiSettings Ui { get; set; }
    }

    public class AppSettings
    {
        [JsonPropertyName("api_base_url")]
        public string ApiBaseUrl { get; set; }

        [JsonPropertyName("app_name")]
        public string AppName { get; set; }

        [JsonPropertyName("company_name")]
        public string CompanyName { get; set; }

        [JsonPropertyName("version")]
        public string Version { get; set; }
    }

    public class ThemeSettings
    {
        [JsonPropertyName("primary_color")]
        public string PrimaryColor { get; set; }

        [JsonPropertyName("secondary_color")]
        public string SecondaryColor { get; set; }

        [JsonPropertyName("background_color")]
        public string BackgroundColor { get; set; }

        [JsonPropertyName("text_color")]
        public string TextColor { get; set; }

        [JsonPropertyName("accent_color")]
        public string AccentColor { get; set; }
    }

    public class LoggingSettings
    {
        [JsonPropertyName("log_level")]
        public Dictionary<string, string> LogLevel { get; set; }

        [JsonPropertyName("file_path")]
        public string FilePath { get; set; }

        [JsonPropertyName("retain_days")]
        public int RetainDays { get; set; }
    }

    public class SecuritySettings
    {
        [JsonPropertyName("token_storage_key")]
        public string TokenStorageKey { get; set; }

        [JsonPropertyName("refresh_token_storage_key")]
        public string RefreshTokenStorageKey { get; set; }

        [JsonPropertyName("token_expiration_minutes")]
        public int TokenExpirationMinutes { get; set; }

        [JsonPropertyName("refresh_token_expiration_days")]
        public int RefreshTokenExpirationDays { get; set; }

        [JsonPropertyName("require_two_factor")]
        public bool RequireTwoFactor { get; set; }
    }

    public class FeatureSettings
    {
        [JsonPropertyName("enable_dark_mode")]
        public bool EnableDarkMode { get; set; }

        [JsonPropertyName("enable_offline_mode")]
        public bool EnableOfflineMode { get; set; }

        [JsonPropertyName("enable_auto_logout")]
        public bool EnableAutoLogout { get; set; }

        [JsonPropertyName("auto_logout_minutes")]
        public int AutoLogoutMinutes { get; set; }

        [JsonPropertyName("enable_notifications")]
        public bool EnableNotifications { get; set; }

        [JsonPropertyName("enable_biometrics")]
        public bool EnableBiometrics { get; set; }

        [JsonPropertyName("enable_qr_scanning")]
        public bool EnableQrScanning { get; set; }
    }

    public class ApiSettings
    {
        [JsonPropertyName("endpoints")]
        public ApiEndpoints Endpoints { get; set; }

        [JsonPropertyName("request_timeout")]
        public int RequestTimeout { get; set; }

        [JsonPropertyName("retry_count")]
        public int RetryCount { get; set; }

        [JsonPropertyName("retry_delay_milliseconds")]
        public int RetryDelayMilliseconds { get; set; }
    }

    public class ApiEndpoints
    {
        [JsonPropertyName("auth")]
        public AuthEndpoints Auth { get; set; }

        [JsonPropertyName("dashboard")]
        public DashboardEndpoints Dashboard { get; set; }

        [JsonPropertyName("keys")]
        public KeyEndpoints Keys { get; set; }

        [JsonPropertyName("access_cards")]
        public AccessCardEndpoints AccessCards { get; set; }

        [JsonPropertyName("employees")]
        public EmployeeEndpoints Employees { get; set; }

        [JsonPropertyName("departments")]
        public DepartmentEndpoints Departments { get; set; }

        [JsonPropertyName("transactions")]
        public TransactionEndpoints Transactions { get; set; }

        [JsonPropertyName("reports")]
        public ReportEndpoints Reports { get; set; }
    }

    public class UiSettings
    {
        [JsonPropertyName("refresh_intervals")]
        public RefreshIntervals RefreshIntervals { get; set; }

        [JsonPropertyName("pagination")]
        public PaginationSettings Pagination { get; set; }

        [JsonPropertyName("date_time_format")]
        public DateTimeFormatSettings DateTimeFormat { get; set; }
    }

    public class RefreshIntervals
    {
        [JsonPropertyName("dashboard")]
        public int Dashboard { get; set; }

        [JsonPropertyName("alerts")]
        public int Alerts { get; set; }

        [JsonPropertyName("active_transactions")]
        public int ActiveTransactions { get; set; }
    }

    public class PaginationSettings
    {
        [JsonPropertyName("default_page_size")]
        public int DefaultPageSize { get; set; }

        [JsonPropertyName("max_page_size")]
        public int MaxPageSize { get; set; }
    }

    public class DateTimeFormatSettings
    {
        [JsonPropertyName("short_date")]
        public string ShortDate { get; set; }

        [JsonPropertyName("long_date")]
        public string LongDate { get; set; }

        [JsonPropertyName("short_time")]
        public string ShortTime { get; set; }

        [JsonPropertyName("long_time")]
        public string LongTime { get; set; }

        [JsonPropertyName("date_time")]
        public string DateTime { get; set; }
    }

    // Endpoint classes for strongly-typed API routes
    public class AuthEndpoints
    {
        [JsonPropertyName("login")]
        public string Login { get; set; }

        [JsonPropertyName("refresh")]
        public string Refresh { get; set; }

        [JsonPropertyName("setup_2fa")]
        public string Setup2FA { get; set; }

        [JsonPropertyName("disable_2fa")]
        public string Disable2FA { get; set; }

        [JsonPropertyName("change_password")]
        public string ChangePassword { get; set; }

        [JsonPropertyName("logout")]
        public string Logout { get; set; }
    }

    // Add other endpoint classes (DashboardEndpoints, KeyEndpoints, etc.) as needed...
}
