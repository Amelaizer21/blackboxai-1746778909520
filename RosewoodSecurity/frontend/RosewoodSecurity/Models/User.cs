using System;
using System.Text.Json.Serialization;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace RosewoodSecurity.Models
{
    public class User : INotifyPropertyChanged
    {
        private int _id;
        private string _username;
        private string _email;
        private string _role;
        private bool _isActive;
        private bool _twoFaEnabled;
        private DateTime _createdAt;
        private DateTime? _lastLogin;

        public event PropertyChangedEventHandler PropertyChanged;

        [JsonPropertyName("id")]
        public int Id
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }

        [JsonPropertyName("username")]
        public string Username
        {
            get => _username;
            set => SetProperty(ref _username, value);
        }

        [JsonPropertyName("email")]
        public string Email
        {
            get => _email;
            set => SetProperty(ref _email, value);
        }

        [JsonPropertyName("role")]
        public string Role
        {
            get => _role;
            set => SetProperty(ref _role, value);
        }

        [JsonPropertyName("is_active")]
        public bool IsActive
        {
            get => _isActive;
            set => SetProperty(ref _isActive, value);
        }

        [JsonPropertyName("two_fa_enabled")]
        public bool TwoFaEnabled
        {
            get => _twoFaEnabled;
            set => SetProperty(ref _twoFaEnabled, value);
        }

        [JsonPropertyName("created_at")]
        public DateTime CreatedAt
        {
            get => _createdAt;
            set => SetProperty(ref _createdAt, value);
        }

        [JsonPropertyName("last_login")]
        public DateTime? LastLogin
        {
            get => _lastLogin;
            set => SetProperty(ref _lastLogin, value);
        }

        // Computed properties
        public bool IsAdmin => Role?.ToLower() == "admin";
        public bool IsSecurityStaff => Role?.ToLower() == "security_staff" || IsAdmin;
        public bool IsAuditor => Role?.ToLower() == "auditor" || IsSecurityStaff;

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class AuthRequest
    {
        [JsonPropertyName("username")]
        public string Username { get; set; }

        [JsonPropertyName("password")]
        public string Password { get; set; }

        [JsonPropertyName("two_fa_code")]
        public string TwoFactorCode { get; set; }
    }

    public class AuthResponse
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; }

        [JsonPropertyName("refresh_token")]
        public string RefreshToken { get; set; }

        [JsonPropertyName("user")]
        public User User { get; set; }

        [JsonPropertyName("requires_2fa")]
        public bool Requires2FA { get; set; }
    }

    public class TwoFactorSetupResponse
    {
        [JsonPropertyName("message")]
        public string Message { get; set; }

        [JsonPropertyName("secret")]
        public string Secret { get; set; }

        [JsonPropertyName("qr_uri")]
        public string QrUri { get; set; }
    }

    public class PasswordChangeRequest
    {
        [JsonPropertyName("current_password")]
        public string CurrentPassword { get; set; }

        [JsonPropertyName("new_password")]
        public string NewPassword { get; set; }
    }

    public class UserCreateRequest
    {
        [JsonPropertyName("username")]
        public string Username { get; set; }

        [JsonPropertyName("email")]
        public string Email { get; set; }

        [JsonPropertyName("password")]
        public string Password { get; set; }

        [JsonPropertyName("role")]
        public string Role { get; set; }
    }

    public class UserUpdateRequest
    {
        [JsonPropertyName("email")]
        public string Email { get; set; }

        [JsonPropertyName("role")]
        public string Role { get; set; }

        [JsonPropertyName("is_active")]
        public bool IsActive { get; set; }

        [JsonPropertyName("two_fa_enabled")]
        public bool TwoFaEnabled { get; set; }
    }

    public static class UserRoles
    {
        public const string Admin = "admin";
        public const string SecurityStaff = "security_staff";
        public const string Auditor = "auditor";

        public static readonly string[] AllRoles = new[]
        {
            Admin,
            SecurityStaff,
            Auditor
        };
    }
}
