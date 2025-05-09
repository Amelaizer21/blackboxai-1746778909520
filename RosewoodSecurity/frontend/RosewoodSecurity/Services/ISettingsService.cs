using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace RosewoodSecurity.Services
{
    public interface ISettingsService
    {
        // General settings
        string AppName { get; }
        string CompanyName { get; }
        string Version { get; }
        string ApiBaseUrl { get; set; }
        
        // Feature flags
        bool IsDarkModeEnabled { get; set; }
        bool IsOfflineModeEnabled { get; set; }
        bool IsAutoLogoutEnabled { get; set; }
        int AutoLogoutMinutes { get; set; }
        bool AreNotificationsEnabled { get; set; }
        bool IsBiometricsEnabled { get; set; }
        bool IsQrScanningEnabled { get; set; }
        
        // Security settings
        bool RequireTwoFactor { get; set; }
        int TokenExpirationMinutes { get; set; }
        int RefreshTokenExpirationDays { get; set; }
        
        // UI settings
        int DefaultPageSize { get; set; }
        int MaxPageSize { get; set; }
        string DateFormat { get; set; }
        string TimeFormat { get; set; }
        string DateTimeFormat { get; set; }
        
        // Refresh intervals
        int DashboardRefreshInterval { get; set; }
        int AlertsRefreshInterval { get; set; }
        int ActiveTransactionsRefreshInterval { get; set; }
        
        // API settings
        int RequestTimeout { get; set; }
        int RetryCount { get; set; }
        int RetryDelayMilliseconds { get; set; }
        
        // Methods
        T GetValue<T>(string key, T defaultValue = default);
        void SetValue<T>(string key, T value);
        bool TryGetValue<T>(string key, out T value);
        void RemoveValue(string key);
        
        // Sections
        IConfigurationSection GetSection(string sectionName);
        bool HasSection(string sectionName);
        
        // Save/Load
        Task SaveSettingsAsync();
        Task LoadSettingsAsync();
        void ResetToDefaults();
        
        // Export/Import
        Task<string> ExportSettingsAsync();
        Task ImportSettingsAsync(string settings);
        
        // Events
        event EventHandler<SettingChangedEventArgs> SettingChanged;
        
        // Validation
        bool ValidateSetting<T>(string key, T value);
        IEnumerable<string> GetValidationErrors();
        
        // Encryption
        void EncryptSetting(string key);
        void DecryptSetting(string key);
        
        // Backup
        Task<bool> BackupSettingsAsync(string location);
        Task<bool> RestoreSettingsAsync(string location);
    }

    public class SettingChangedEventArgs : EventArgs
    {
        public string Key { get; }
        public object OldValue { get; }
        public object NewValue { get; }
        public bool IsEncrypted { get; }

        public SettingChangedEventArgs(string key, object oldValue, object newValue, bool isEncrypted = false)
        {
            Key = key;
            OldValue = oldValue;
            NewValue = newValue;
            IsEncrypted = isEncrypted;
        }
    }

    public class SettingsValidationException : Exception
    {
        public string SettingKey { get; }
        public object InvalidValue { get; }
        public IEnumerable<string> ValidationErrors { get; }

        public SettingsValidationException(string settingKey, object invalidValue, 
            IEnumerable<string> validationErrors, string message = null) 
            : base(message ?? $"Invalid value for setting: {settingKey}")
        {
            SettingKey = settingKey;
            InvalidValue = invalidValue;
            ValidationErrors = validationErrors;
        }
    }

    public interface ISettingsValidator
    {
        bool ValidateSetting<T>(string key, T value, out IEnumerable<string> errors);
        void RegisterValidationRule<T>(string key, Func<T, bool> rule, string errorMessage);
        void RemoveValidationRule(string key);
    }

    public interface ISettingsEncryption
    {
        string EncryptValue(string value);
        string DecryptValue(string encryptedValue);
        bool IsEncrypted(string value);
    }

    public class SettingsConstants
    {
        public static class Keys
        {
            public const string AppName = "AppSettings:AppName";
            public const string CompanyName = "AppSettings:CompanyName";
            public const string Version = "AppSettings:Version";
            public const string ApiBaseUrl = "AppSettings:ApiBaseUrl";
            
            public const string DarkMode = "Features:EnableDarkMode";
            public const string OfflineMode = "Features:EnableOfflineMode";
            public const string AutoLogout = "Features:EnableAutoLogout";
            public const string AutoLogoutMinutes = "Features:AutoLogoutMinutes";
            public const string Notifications = "Features:EnableNotifications";
            public const string Biometrics = "Features:EnableBiometrics";
            public const string QrScanning = "Features:EnableQrScanning";
            
            public const string RequireTwoFactor = "Security:RequireTwoFactor";
            public const string TokenExpiration = "Security:TokenExpirationMinutes";
            public const string RefreshTokenExpiration = "Security:RefreshTokenExpirationDays";
        }

        public static class Defaults
        {
            public const int DefaultPageSize = 20;
            public const int MaxPageSize = 100;
            public const string DateFormat = "MM/dd/yyyy";
            public const string TimeFormat = "HH:mm";
            public const string DateTimeFormat = "MM/dd/yyyy HH:mm";
            
            public const int DashboardRefreshInterval = 60;
            public const int AlertsRefreshInterval = 30;
            public const int ActiveTransactionsRefreshInterval = 60;
            
            public const int RequestTimeout = 30;
            public const int RetryCount = 3;
            public const int RetryDelay = 1000;
        }
    }
}
