using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace RosewoodSecurity.Services
{
    public interface IThemeService
    {
        // Theme management
        ThemeType CurrentTheme { get; }
        void Initialize();
        void SetTheme(ThemeType theme);
        void ToggleTheme();
        
        // Color management
        Color GetColor(string colorKey);
        void SetColor(string colorKey, Color color);
        void ResetColors();
        
        // Resource management
        void UpdateResource(string key, object value);
        T GetResource<T>(string key);
        void ResetResource(string key);
        
        // Theme customization
        void CustomizeTheme(ThemeCustomization customization);
        ThemeCustomization GetCurrentCustomization();
        void SaveCustomization();
        void LoadCustomization();
        
        // Font management
        void SetFontFamily(string fontFamily);
        void SetFontSize(double size);
        
        // Events
        event EventHandler<ThemeChangedEventArgs> ThemeChanged;
        
        // Theme availability
        IEnumerable<ThemeType> GetAvailableThemes();
        bool IsThemeAvailable(ThemeType theme);
        
        // High contrast support
        bool IsHighContrastEnabled { get; }
        void EnableHighContrast();
        void DisableHighContrast();
    }

    public enum ThemeType
    {
        Light,
        Dark,
        System,
        Custom
    }

    public class ThemeCustomization
    {
        public Color PrimaryColor { get; set; }
        public Color SecondaryColor { get; set; }
        public Color BackgroundColor { get; set; }
        public Color TextColor { get; set; }
        public Color AccentColor { get; set; }
        public string FontFamily { get; set; }
        public double FontSize { get; set; }
        public bool EnableShadows { get; set; }
        public bool EnableAnimations { get; set; }
        public double CornerRadius { get; set; }

        public ThemeCustomization()
        {
            // Default values
            PrimaryColor = (Color)ColorConverter.ConvertFromString("#1976D2");
            SecondaryColor = (Color)ColorConverter.ConvertFromString("#424242");
            BackgroundColor = (Color)ColorConverter.ConvertFromString("#FFFFFF");
            TextColor = (Color)ColorConverter.ConvertFromString("#000000");
            AccentColor = (Color)ColorConverter.ConvertFromString("#FF4081");
            FontFamily = "Segoe UI";
            FontSize = 14;
            EnableShadows = true;
            EnableAnimations = true;
            CornerRadius = 4;
        }
    }

    public class ThemeChangedEventArgs : EventArgs
    {
        public ThemeType OldTheme { get; }
        public ThemeType NewTheme { get; }
        public bool IsSystemTheme { get; }

        public ThemeChangedEventArgs(ThemeType oldTheme, ThemeType newTheme, bool isSystemTheme = false)
        {
            OldTheme = oldTheme;
            NewTheme = newTheme;
            IsSystemTheme = isSystemTheme;
        }
    }

    public interface IThemeResourceProvider
    {
        ResourceDictionary GetThemeResources(ThemeType theme);
        ResourceDictionary GetCustomThemeResources(ThemeCustomization customization);
    }

    public static class ThemeColors
    {
        public static class Light
        {
            public static readonly Color Primary = (Color)ColorConverter.ConvertFromString("#1976D2");
            public static readonly Color Secondary = (Color)ColorConverter.ConvertFromString("#424242");
            public static readonly Color Background = (Color)ColorConverter.ConvertFromString("#FFFFFF");
            public static readonly Color Surface = (Color)ColorConverter.ConvertFromString("#FFFFFF");
            public static readonly Color Error = (Color)ColorConverter.ConvertFromString("#B00020");
            public static readonly Color OnPrimary = (Color)ColorConverter.ConvertFromString("#FFFFFF");
            public static readonly Color OnSecondary = (Color)ColorConverter.ConvertFromString("#FFFFFF");
            public static readonly Color OnBackground = (Color)ColorConverter.ConvertFromString("#000000");
            public static readonly Color OnSurface = (Color)ColorConverter.ConvertFromString("#000000");
            public static readonly Color OnError = (Color)ColorConverter.ConvertFromString("#FFFFFF");
        }

        public static class Dark
        {
            public static readonly Color Primary = (Color)ColorConverter.ConvertFromString("#1565C0");
            public static readonly Color Secondary = (Color)ColorConverter.ConvertFromString("#212121");
            public static readonly Color Background = (Color)ColorConverter.ConvertFromString("#121212");
            public static readonly Color Surface = (Color)ColorConverter.ConvertFromString("#121212");
            public static readonly Color Error = (Color)ColorConverter.ConvertFromString("#CF6679");
            public static readonly Color OnPrimary = (Color)ColorConverter.ConvertFromString("#FFFFFF");
            public static readonly Color OnSecondary = (Color)ColorConverter.ConvertFromString("#FFFFFF");
            public static readonly Color OnBackground = (Color)ColorConverter.ConvertFromString("#FFFFFF");
            public static readonly Color OnSurface = (Color)ColorConverter.ConvertFromString("#FFFFFF");
            public static readonly Color OnError = (Color)ColorConverter.ConvertFromString("#000000");
        }

        public static class HighContrast
        {
            public static readonly Color Primary = (Color)ColorConverter.ConvertFromString("#FFFFFF");
            public static readonly Color Secondary = (Color)ColorConverter.ConvertFromString("#FFFFFF");
            public static readonly Color Background = (Color)ColorConverter.ConvertFromString("#000000");
            public static readonly Color Surface = (Color)ColorConverter.ConvertFromString("#000000");
            public static readonly Color Error = (Color)ColorConverter.ConvertFromString("#FF0000");
            public static readonly Color OnPrimary = (Color)ColorConverter.ConvertFromString("#000000");
            public static readonly Color OnSecondary = (Color)ColorConverter.ConvertFromString("#000000");
            public static readonly Color OnBackground = (Color)ColorConverter.ConvertFromString("#FFFFFF");
            public static readonly Color OnSurface = (Color)ColorConverter.ConvertFromString("#FFFFFF");
            public static readonly Color OnError = (Color)ColorConverter.ConvertFromString("#FFFFFF");
        }
    }
}
