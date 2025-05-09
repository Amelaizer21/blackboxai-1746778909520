using System;
using MaterialDesignThemes.Wpf;
using Moq;
using RosewoodSecurity.Services;
using Xunit;

namespace RosewoodSecurity.Tests.Services
{
    public class ThemeServiceTests
    {
        private readonly Mock<ISettingsService> _mockSettingsService;
        private readonly Mock<ISnackbarMessageQueue> _mockMessageQueue;
        private readonly ThemeService _themeService;

        public ThemeServiceTests()
        {
            _mockSettingsService = new Mock<ISettingsService>();
            _mockMessageQueue = new Mock<ISnackbarMessageQueue>();
            _themeService = new ThemeService(_mockSettingsService.Object, _mockMessageQueue.Object);
        }

        [Fact]
        public void Initialize_LoadsThemePreference()
        {
            // Arrange
            _mockSettingsService.Setup(x => x.GetSetting("Theme:IsDark", false))
                .Returns(true);

            // Act
            _themeService.Initialize();

            // Assert
            Assert.True(_themeService.IsDarkTheme);
        }

        [Fact]
        public void SetTheme_UpdatesThemeAndSavesPreference()
        {
            // Arrange
            bool savedValue = false;
            _mockSettingsService.Setup(x => x.SetSetting("Theme:IsDark", It.IsAny<bool>()))
                .Callback<string, bool>((key, value) => savedValue = value);

            bool eventRaised = false;
            _themeService.ThemeChanged += (s, isDark) => eventRaised = true;

            // Act
            _themeService.SetTheme(true);

            // Assert
            Assert.True(_themeService.IsDarkTheme);
            Assert.True(savedValue);
            Assert.True(eventRaised);
        }

        [Fact]
        public void ToggleTheme_SwitchesTheme()
        {
            // Arrange
            var initialTheme = _themeService.IsDarkTheme;
            bool eventRaised = false;
            _themeService.ThemeChanged += (s, isDark) => eventRaised = true;

            // Act
            _themeService.ToggleTheme();

            // Assert
            Assert.NotEqual(initialTheme, _themeService.IsDarkTheme);
            Assert.True(eventRaised);
        }

        [Fact]
        public void ApplyTheme_WithDarkTheme_UpdatesResources()
        {
            // Arrange
            bool eventRaised = false;
            _themeService.ThemeChanged += (s, isDark) => eventRaised = true;

            // Act
            _themeService.ApplyTheme(true);

            // Assert
            Assert.True(_themeService.IsDarkTheme);
            Assert.True(eventRaised);
        }

        [Fact]
        public void ApplyTheme_WithLightTheme_UpdatesResources()
        {
            // Arrange
            bool eventRaised = false;
            _themeService.ThemeChanged += (s, isDark) => eventRaised = true;

            // Act
            _themeService.ApplyTheme(false);

            // Assert
            Assert.False(_themeService.IsDarkTheme);
            Assert.True(eventRaised);
        }

        [Fact]
        public void SaveThemePreference_StoresInSettings()
        {
            // Arrange
            bool savedValue = false;
            _mockSettingsService.Setup(x => x.SetSetting("Theme:IsDark", It.IsAny<bool>()))
                .Callback<string, bool>((key, value) => savedValue = value);

            // Act
            _themeService.SaveThemePreference(true);

            // Assert
            Assert.True(savedValue);
            _mockSettingsService.Verify(x => x.SaveSettings(), Times.Once);
        }

        [Fact]
        public void LoadThemePreference_RetrievesFromSettings()
        {
            // Arrange
            _mockSettingsService.Setup(x => x.GetSetting("Theme:IsDark", false))
                .Returns(true);

            // Act
            var result = _themeService.LoadThemePreference();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void LoadThemePreference_WithError_ReturnsFalse()
        {
            // Arrange
            _mockSettingsService.Setup(x => x.GetSetting("Theme:IsDark", false))
                .Throws(new Exception("Test error"));

            // Act
            var result = _themeService.LoadThemePreference();

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void ApplyTheme_WithError_ShowsErrorMessage()
        {
            // Arrange
            bool messageShown = false;
            _mockMessageQueue.Setup(x => x.Enqueue(
                It.IsAny<string>(),
                It.IsAny<object>(),
                It.IsAny<object>(),
                It.IsAny<object>(),
                It.IsAny<bool>(),
                It.IsAny<bool>(),
                It.IsAny<TimeSpan>()))
                .Callback(() => messageShown = true);

            // Simulate an error by making settings throw an exception
            _mockSettingsService.Setup(x => x.SetSetting(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws(new Exception("Test error"));

            // Act
            _themeService.ApplyTheme(true);

            // Assert
            Assert.True(messageShown);
        }

        [Fact]
        public void SaveThemePreference_WithError_ShowsErrorMessage()
        {
            // Arrange
            bool messageShown = false;
            _mockMessageQueue.Setup(x => x.Enqueue(
                It.IsAny<string>(),
                It.IsAny<object>(),
                It.IsAny<object>(),
                It.IsAny<object>(),
                It.IsAny<bool>(),
                It.IsAny<bool>(),
                It.IsAny<TimeSpan>()))
                .Callback(() => messageShown = true);

            // Simulate an error
            _mockSettingsService.Setup(x => x.SetSetting(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws(new Exception("Test error"));

            // Act
            _themeService.SaveThemePreference(true);

            // Assert
            Assert.True(messageShown);
        }
    }
}
