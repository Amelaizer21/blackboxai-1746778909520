using System;
using System.Windows;
using MaterialDesignThemes.Wpf;
using RosewoodSecurity.Models;

namespace RosewoodSecurity.Services
{
    public class ThemeService : IThemeService
    {
        private readonly ISettingsService _settingsService;
        private readonly ISnackbarMessageQueue _messageQueue;
        private bool _isDarkTheme;

        public event EventHandler<bool> ThemeChanged;

        public ThemeService(ISettingsService settingsService, ISnackbarMessageQueue messageQueue)
        {
            _settingsService = settingsService;
            _messageQueue = messageQueue;
        }

        public bool IsDarkTheme => _isDarkTheme;

        public void Initialize()
        {
            // Load theme preference from settings
            var isDark = LoadThemePreference();
            ApplyTheme(isDark);
        }

        public void SetTheme(bool isDark)
        {
            if (_isDarkTheme != isDark)
            {
                ApplyTheme(isDark);
                SaveThemePreference(isDark);
            }
        }

        public void ToggleTheme()
        {
            SetTheme(!_isDarkTheme);
        }

        public void ApplyTheme(bool isDark)
        {
            try
            {
                var paletteHelper = new PaletteHelper();
                var theme = paletteHelper.GetTheme();

                theme.SetBaseTheme(isDark ? Theme.Dark : Theme.Light);
                paletteHelper.SetTheme(theme);

                // Update application resources
                if (isDark)
                {
                    Application.Current.Resources["MaterialDesignPaper"] = Application.Current.Resources["DarkBackgroundBrush"];
                    Application.Current.Resources["MaterialDesignBody"] = Application.Current.Resources["DarkTextBrush"];
                }
                else
                {
                    Application.Current.Resources["MaterialDesignPaper"] = Application.Current.Resources["BackgroundBrush"];
                    Application.Current.Resources["MaterialDesignBody"] = Application.Current.Resources["TextBrush"];
                }

                _isDarkTheme = isDark;
                ThemeChanged?.Invoke(this, isDark);

                // Show feedback
                _messageQueue.Enqueue($"Switched to {(isDark ? "dark" : "light")} theme", 
                    null, null, null, false, true, TimeSpan.FromSeconds(2));
            }
            catch (Exception ex)
            {
                _messageQueue.Enqueue($"Failed to apply theme: {ex.Message}",
                    null, null, null, false, true, TimeSpan.FromSeconds(3));
            }
        }

        public void SaveThemePreference(bool isDark)
        {
            try
            {
                _settingsService.SetSetting("Theme:IsDark", isDark);
                _settingsService.SaveSettings();
            }
            catch (Exception ex)
            {
                _messageQueue.Enqueue($"Failed to save theme preference: {ex.Message}",
                    null, null, null, false, true, TimeSpan.FromSeconds(3));
            }
        }

        public bool LoadThemePreference()
        {
            try
            {
                return _settingsService.GetSetting("Theme:IsDark", false);
            }
            catch (Exception)
            {
                // If there's an error loading the preference, default to light theme
                return false;
            }
        }
    }
}
