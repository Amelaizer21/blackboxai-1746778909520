using System;
using System.Threading.Tasks;
using RosewoodSecurity.Models;

namespace RosewoodSecurity.Services
{
    public interface IAuthenticationService
    {
        event EventHandler<AuthenticationEventArgs> AuthenticationChanged;

        bool IsAuthenticated { get; }
        UserInfo CurrentUser { get; }
        string AuthToken { get; }

        Task<bool> InitiateLoginAsync(string username, string password);
        Task<bool> ValidateTwoFactorAsync(string code);
        Task<bool> LoginWithTokenAsync(string token);
        Task<bool> RefreshTokenAsync();
        void Logout();
        
        bool HasPermission(string permission);
        bool IsInRole(string role);
        Task<bool> ChangePasswordAsync(string currentPassword, string newPassword);
        Task<bool> RequestPasswordResetAsync(string email);
        Task<bool> ResetPasswordAsync(string token, string newPassword);
        
        void SaveAuthenticationState(AuthenticationResult result);
        AuthenticationResult LoadAuthenticationState();
        void ClearAuthenticationState();
    }

    public class AuthenticationResult
    {
        public bool Success { get; set; }
        public UserInfo User { get; set; }
        public string Token { get; set; }
        public string RefreshToken { get; set; }
        public DateTime TokenExpiration { get; set; }
        public bool RequiresTwoFactor { get; set; }
        public string TwoFactorType { get; set; }
        public string ErrorMessage { get; set; }
    }

    public class UserInfo
    {
        public string Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public string Role { get; set; }
        public string[] Permissions { get; set; }
        public bool IsActive { get; set; }
        public DateTime LastLogin { get; set; }
        public bool RequirePasswordChange { get; set; }
    }

    public class AuthenticationEventArgs : EventArgs
    {
        public bool IsAuthenticated { get; set; }
        public UserInfo CurrentUser { get; set; }
        public string ErrorMessage { get; set; }
    }

    public class PasswordChangeResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public bool RequiresRelogin { get; set; }
    }
}
