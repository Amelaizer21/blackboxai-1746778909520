using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using MaterialDesignThemes.Wpf;

namespace RosewoodSecurity.Services
{
    public class SettingsService : ISettingsService
    {
        private readonly string _settingsPath;
        private readonly IConfiguration _configuration;
        private readonly ISnackbarMessageQueue _messageQueue;
        private ApplicationSettings _settings;

        public event EventHandler<SettingsChangedEventArgs> SettingsChanged;

        public SettingsService(IConfiguration configuration, ISnackbarMessageQueue messageQueue)
        {
            _configuration = configuration;
            _messageQueue = messageQueue;
            _settingsPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "RosewoodSecurity",
                "settings.json"
            );

            // Initialize default settings
            _settings = new ApplicationSettings();
            InitializeDefaultSettings();
        }

        #region ISettingsService Implementation

        public T GetSetting<T>(string key, T defaultValue = default)
        {
            if (_settings.Values.TryGetValue(key, out var value))
            {
                try
                {
                    if (value is JsonElement jsonElement)
                    {
                        return jsonElement.Deserialize<T>();
                    }
                    return (T)Convert.ChangeType(value, typeof(T));
                }
                catch
                {
                    return defaultValue;
                }
            }
            return defaultValue;
        }

        public void SetSetting<T>(string key, T value)
        {
            var oldValue = GetSetting<T>(key);
            _settings.Values[key] = value;
            _settings.LastModified = DateTime.UtcNow;
            
            OnSettingChanged(key, oldValue, value);
        }

        public bool HasSetting(string key)
        {
            return _settings.Values.ContainsKey(key);
        }

        public void RemoveSetting(string key)
        {
            if (_settings.Values.ContainsKey(key))
            {
                var oldValue = _settings.Values[key];
                _settings.Values.Remove(key);
                OnSettingChanged(key, oldValue, null);
            }
        }

        public void ClearSettings()
        {
            _settings.Values.Clear();
            InitializeDefaultSettings();
            SaveSettings();
        }

        public void SaveSettings()
        {
            try
            {
                var directory = Path.GetDirectoryName(_settingsPath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                var json = JsonSerializer.Serialize(_settings, new JsonSerializerOptions
                {
                    WriteIndented = true
                });
                File.WriteAllText(_settingsPath, json);

                _messageQueue.Enqueue("Settings saved successfully",
                    null, null, null, false, true, TimeSpan.FromSeconds(2));
            }
            catch (Exception ex)
            {
                _messageQueue.Enqueue($"Failed to save settings: {ex.Message}",
                    null, null, null, false, true, TimeSpan.FromSeconds(3));
            }
        }

        public void LoadSettings()
        {
            try
            {
                if (File.Exists(_settingsPath))
                {
                    var json = File.ReadAllText(_settingsPath);
                    _settings = JsonSerializer.Deserialize<ApplicationSettings>(json);
                }
                else
                {
                    InitializeDefaultSettings();
                }
            }
            catch (Exception ex)
            {
                _messageQueue.Enqueue($"Failed to load settings: {ex.Message}",
                    null, null, null, false, true, TimeSpan.FromSeconds(3));
                InitializeDefaultSettings();
            }
        }

        #region Properties

        public string ApiBaseUrl
        {
            get => GetSetting<string>("Api:BaseUrl", _configuration["Api:BaseUrl"]);
            set => SetSetting("Api:BaseUrl", value);
        }

        public int ApiTimeout
        {
            get => GetSetting("Api:Timeout", 30);
            set => SetSetting("Api:Timeout", value);
        }

        public bool EnableNotifications
        {
            get => GetSetting("Notifications:Enabled", true);
            set => SetSetting("Notifications:Enabled", value);
        }

        public string DefaultDateFormat
        {
            get => GetSetting("Format:Date", "yyyy-MM-dd");
            set => SetSetting("Format:Date", value);
        }

        public string DefaultTimeFormat
        {
            get => GetSetting("Format:Time", "HH:mm");
            set => SetSetting("Format:Time", value);
        }

        public int AutoRefreshInterval
        {
            get => GetSetting("UI:AutoRefreshInterval", 30);
            set => SetSetting("UI:AutoRefreshInterval", value);
        }

        public bool EnableAutoRefresh
        {
            get => GetSetting("UI:EnableAutoRefresh", true);
            set => SetSetting("UI:EnableAutoRefresh", value);
        }

        public bool EnableScanner
        {
            get => GetSetting("Scanner:Enabled", true);
            set => SetSetting("Scanner:Enabled", value);
        }

        public string ScannerPort
        {
            get => GetSetting<string>("Scanner:Port", "COM1");
            set => SetSetting("Scanner:Port", value);
        }

        public int ScannerBaudRate
        {
            get => GetSetting("Scanner:BaudRate", 9600);
            set => SetSetting("Scanner:BaudRate", value);
        }

        public string ScannerType
        {
            get => GetSetting<string>("Scanner:Type", "Barcode");
            set => SetSetting("Scanner:Type", value);
        }

        public int SessionTimeout
        {
            get => GetSetting("Security:SessionTimeout", 30);
            set => SetSetting("Security:SessionTimeout", value);
        }

        public bool RequireTwoFactor
        {
            get => GetSetting("Security:RequireTwoFactor", true);
            set => SetSetting("Security:RequireTwoFactor", value);
        }

        public int PasswordExpiryDays
        {
            get => GetSetting("Security:PasswordExpiryDays", 90);
            set => SetSetting("Security:PasswordExpiryDays", value);
        }

        public bool EnablePasswordComplexity
        {
            get => GetSetting("Security:EnablePasswordComplexity", true);
            set => SetSetting("Security:EnablePasswordComplexity", value);
        }

        public bool EnableDarkMode
        {
            get => GetSetting("UI:EnableDarkMode", false);
            set => SetSetting("UI:EnableDarkMode", value);
        }

        public string PrimaryColor
        {
            get => GetSetting<string>("UI:PrimaryColor", "#1976D2");
            set => SetSetting("UI:PrimaryColor", value);
        }

        public string SecondaryColor
        {
            get => GetSetting<string>("UI:SecondaryColor", "#424242");
            set => SetSetting("UI:SecondaryColor", value);
        }

        public double FontSize
        {
            get => GetSetting("UI:FontSize", 13.0);
            set => SetSetting("UI:FontSize", value);
        }

        public string DefaultExportFormat
        {
            get => GetSetting<string>("Export:DefaultFormat", "PDF");
            set => SetSetting("Export:DefaultFormat", value);
        }

        public string DefaultExportPath
        {
            get => GetSetting<string>("Export:DefaultPath", Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "RosewoodSecurity",
                "Exports"
            ));
            set => SetSetting("Export:DefaultPath", value);
        }

        public bool IncludeHeadersInExport
        {
            get => GetSetting("Export:IncludeHeaders", true);
            set => SetSetting("Export:IncludeHeaders", value);
        }

        public bool EnableEmailNotifications
        {
            get => GetSetting("Notifications:EnableEmail", true);
            set => SetSetting("Notifications:EnableEmail", value);
        }

        public bool EnableSmsNotifications
        {
            get => GetSetting("Notifications:EnableSms", false);
            set => SetSetting("Notifications:EnableSms", value);
        }

        public List<string> NotificationRecipients
        {
            get => GetSetting<List<string>>("Notifications:Recipients", new List<string>());
            set => SetSetting("Notifications:Recipients", value);
        }

        public bool EnableAutoBackup
        {
            get => GetSetting("Backup:EnableAuto", true);
            set => SetSetting("Backup:EnableAuto", value);
        }

        public string BackupPath
        {
            get => GetSetting<string>("Backup:Path", Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "RosewoodSecurity",
                "Backups"
            ));
            set => SetSetting("Backup:Path", value);
        }

        public int BackupRetentionDays
        {
            get => GetSetting("Backup:RetentionDays", 30);
            set => SetSetting("Backup:RetentionDays", value);
        }

        public TimeSpan BackupTime
        {
            get => TimeSpan.Parse(GetSetting<string>("Backup:Time", "02:00:00"));
            set => SetSetting("Backup:Time", value.ToString());
        }

        #endregion

        #endregion

        #region Private Methods

        private void InitializeDefaultSettings()
        {
            // Add metadata for settings
            _settings.Metadata["Api:BaseUrl"] = new SettingMetadata
            {
                Description = "Base URL for the API",
                Category = "API",
                IsSecure = false,
                RequiresRestart = true
            };

            _settings.Metadata["Security:RequireTwoFactor"] = new SettingMetadata
            {
                Description = "Require two-factor authentication for login",
                Category = "Security",
                IsSecure = false,
                RequiresRestart = false
            };

            // Set default values if not already set
            if (!HasSetting("Api:BaseUrl"))
            {
                SetSetting("Api:BaseUrl", _configuration["Api:BaseUrl"]);
            }

            if (!HasSetting("Security:RequireTwoFactor"))
            {
                SetSetting("Security:RequireTwoFactor", true);
            }

            // Save initial settings
            SaveSettings();
        }

        private void OnSettingChanged(string key, object oldValue, object newValue)
        {
            var metadata = _settings.Metadata.TryGetValue(key, out var meta) ? meta : null;
            SettingsChanged?.Invoke(this, new SettingsChangedEventArgs
            {
                Key = key,
                OldValue = oldValue,
                NewValue = newValue,
                Category = metadata?.Category
            });
        }

        #endregion
    }
}
