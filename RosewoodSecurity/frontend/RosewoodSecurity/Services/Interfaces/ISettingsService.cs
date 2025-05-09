using System;
using System.Collections.Generic;

namespace RosewoodSecurity.Services
{
    public interface ISettingsService
    {
        event EventHandler<SettingsChangedEventArgs> SettingsChanged;

        T GetSetting<T>(string key, T defaultValue = default);
        void SetSetting<T>(string key, T value);
        bool HasSetting(string key);
        void RemoveSetting(string key);
        void ClearSettings();
        
        void SaveSettings();
        void LoadSettings();
        
        // Application specific settings
        string ApiBaseUrl { get; set; }
        int ApiTimeout { get; set; }
        bool EnableNotifications { get; set; }
        string DefaultDateFormat { get; set; }
        string DefaultTimeFormat { get; set; }
        int AutoRefreshInterval { get; set; }
        bool EnableAutoRefresh { get; set; }
        
        // Scanner settings
        bool EnableScanner { get; set; }
        string ScannerPort { get; set; }
        int ScannerBaudRate { get; set; }
        string ScannerType { get; set; }
        
        // Security settings
        int SessionTimeout { get; set; }
        bool RequireTwoFactor { get; set; }
        int PasswordExpiryDays { get; set; }
        bool EnablePasswordComplexity { get; set; }
        
        // UI settings
        bool EnableDarkMode { get; set; }
        string PrimaryColor { get; set; }
        string SecondaryColor { get; set; }
        double FontSize { get; set; }
        
        // Export settings
        string DefaultExportFormat { get; set; }
        string DefaultExportPath { get; set; }
        bool IncludeHeadersInExport { get; set; }
        
        // Notification settings
        bool EnableEmailNotifications { get; set; }
        bool EnableSmsNotifications { get; set; }
        List<string> NotificationRecipients { get; set; }
        
        // Backup settings
        bool EnableAutoBackup { get; set; }
        string BackupPath { get; set; }
        int BackupRetentionDays { get; set; }
        TimeSpan BackupTime { get; set; }
    }

    public class SettingsChangedEventArgs : EventArgs
    {
        public string Key { get; set; }
        public object OldValue { get; set; }
        public object NewValue { get; set; }
        public string Category { get; set; }
    }

    public class ApplicationSettings
    {
        public Dictionary<string, object> Values { get; set; } = new Dictionary<string, object>();
        public DateTime LastModified { get; set; }
        public string LastModifiedBy { get; set; }
        public Dictionary<string, SettingMetadata> Metadata { get; set; } = new Dictionary<string, SettingMetadata>();
    }

    public class SettingMetadata
    {
        public string Description { get; set; }
        public string Category { get; set; }
        public bool IsSecure { get; set; }
        public bool RequiresRestart { get; set; }
        public string[] AllowedValues { get; set; }
        public string ValidationRegex { get; set; }
        public string DefaultValue { get; set; }
    }
}
