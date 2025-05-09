using System;

namespace RosewoodSecurity.Services
{
    public interface IThemeService
    {
        event EventHandler<bool> ThemeChanged;
        
        bool IsDarkTheme { get; }
        void Initialize();
        void SetTheme(bool isDark);
        void ToggleTheme();
        void ApplyTheme(bool isDark);
        void SaveThemePreference(bool isDark);
        bool LoadThemePreference();
    }
}
