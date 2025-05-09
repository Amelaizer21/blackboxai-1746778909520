using System;
using System.Collections.Generic;
using MaterialDesignThemes.Wpf;
using Microsoft.Extensions.Configuration;
using Moq;
using RosewoodSecurity.Services;
using Xunit;

namespace RosewoodSecurity.Tests.Services
{
    public class SettingsServiceTests
    {
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly Mock<ISnackbarMessageQueue> _mockMessageQueue;
        private readonly SettingsService _settingsService;

        public SettingsServiceTests()
        {
            _mockConfiguration = new Mock<IConfiguration>();
            _mockMessageQueue = new Mock<ISnackbarMessageQueue>();

            // Setup configuration
            var configSection = new Mock<IConfigurationSection>();
            configSection.Setup(x => x.Value).Returns("http://localhost:5000");
            _mockConfiguration.Setup(x => x["Api:BaseUrl"]).Returns("http://localhost:5000");

            _settingsService = new SettingsService(_mockConfiguration.Object, _mockMessageQueue.Object);
        }

        [Fact]
        public void GetSetting_WithExistingKey_ReturnsValue()
        {
            // Arrange
            var key = "TestKey";
            var expectedValue = "TestValue";
            _settingsService.SetSetting(key, expectedValue);

            // Act
            var actualValue = _settingsService.GetSetting<string>(key);

            // Assert
            Assert.Equal(expectedValue, actualValue);
        }

        [Fact]
        public void GetSetting_WithNonExistingKey_ReturnsDefaultValue()
        {
            // Arrange
            var key = "NonExistingKey";
            var defaultValue = "DefaultValue";

            // Act
            var actualValue = _settingsService.GetSetting(key, defaultValue);

            // Assert
            Assert.Equal(defaultValue, actualValue);
        }

        [Fact]
        public void SetSetting_RaisesSettingsChangedEvent()
        {
            // Arrange
            var key = "TestKey";
            var oldValue = "OldValue";
            var newValue = "NewValue";
            var eventRaised = false;
            object capturedOldValue = null;
            object capturedNewValue = null;

            _settingsService.SetSetting(key, oldValue);
            _settingsService.SettingsChanged += (s, e) =>
            {
                eventRaised = true;
                capturedOldValue = e.OldValue;
                capturedNewValue = e.NewValue;
            };

            // Act
            _settingsService.SetSetting(key, newValue);

            // Assert
            Assert.True(eventRaised);
            Assert.Equal(oldValue, capturedOldValue);
            Assert.Equal(newValue, capturedNewValue);
        }

        [Fact]
        public void HasSetting_WithExistingKey_ReturnsTrue()
        {
            // Arrange
            var key = "TestKey";
            _settingsService.SetSetting(key, "TestValue");

            // Act
            var result = _settingsService.HasSetting(key);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void HasSetting_WithNonExistingKey_ReturnsFalse()
        {
            // Act
            var result = _settingsService.HasSetting("NonExistingKey");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void RemoveSetting_RemovesExistingKey()
        {
            // Arrange
            var key = "TestKey";
            _settingsService.SetSetting(key, "TestValue");

            // Act
            _settingsService.RemoveSetting(key);

            // Assert
            Assert.False(_settingsService.HasSetting(key));
        }

        [Fact]
        public void ClearSettings_RemovesAllSettings()
        {
            // Arrange
            _settingsService.SetSetting("Key1", "Value1");
            _settingsService.SetSetting("Key2", "Value2");

            // Act
            _settingsService.ClearSettings();

            // Assert
            Assert.False(_settingsService.HasSetting("Key1"));
            Assert.False(_settingsService.HasSetting("Key2"));
        }

        [Fact]
        public void ApiBaseUrl_DefaultValue_ReturnsConfigurationValue()
        {
            // Act
            var apiBaseUrl = _settingsService.ApiBaseUrl;

            // Assert
            Assert.Equal("http://localhost:5000", apiBaseUrl);
        }

        [Fact]
        public void ApiTimeout_DefaultValue_Returns30()
        {
            // Act
            var timeout = _settingsService.ApiTimeout;

            // Assert
            Assert.Equal(30, timeout);
        }

        [Fact]
        public void EnableNotifications_DefaultValue_ReturnsTrue()
        {
            // Act
            var enableNotifications = _settingsService.EnableNotifications;

            // Assert
            Assert.True(enableNotifications);
        }

        [Theory]
        [InlineData("yyyy-MM-dd")]
        [InlineData("MM/dd/yyyy")]
        public void DefaultDateFormat_CanBeChanged(string format)
        {
            // Act
            _settingsService.DefaultDateFormat = format;
            var result = _settingsService.DefaultDateFormat;

            // Assert
            Assert.Equal(format, result);
        }

        [Fact]
        public void SaveAndLoadSettings_PreservesValues()
        {
            // Arrange
            _settingsService.SetSetting("TestKey", "TestValue");
            _settingsService.SetSetting("NumberSetting", 42);

            // Act
            _settingsService.SaveSettings();
            _settingsService.ClearSettings();
            _settingsService.LoadSettings();

            // Assert
            Assert.Equal("TestValue", _settingsService.GetSetting<string>("TestKey"));
            Assert.Equal(42, _settingsService.GetSetting<int>("NumberSetting"));
        }
    }
}
