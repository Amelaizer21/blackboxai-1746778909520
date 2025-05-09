using System.Threading.Tasks;
using RosewoodSecurity.Models;

namespace RosewoodSecurity.Services
{
    public interface IAuthenticationService
    {
        bool IsAuthenticated { get; }
        string CurrentUserRole { get; }
        string CurrentUsername { get; }
        string AccessToken { get; }
        
        Task<bool> LoginAsync(string username, string password, string twoFactorCode = null);
        Task<bool> LogoutAsync();
        Task<bool> RefreshTokenAsync();
        
        bool HasPermission(string requiredRole);
        bool CanManageKeys();
        bool CanViewReports();
        bool CanManageEmployees();
        bool CanManageDepartments();
        bool CanPerformCheckout();
        bool CanViewAuditTrail();
        bool CanExtendCheckout();
        bool CanMarkLost();
        
        Task<TwoFactorSetupResponse> Setup2FAAsync();
        Task<bool> Disable2FAAsync();
        Task<bool> ChangePasswordAsync(string currentPassword, string newPassword);
        
        void SaveAuthenticationState(AuthResponse authResponse);
        void ClearAuthenticationState();
        bool TryRestoreAuthenticationState();
        
        event System.EventHandler AuthenticationStateChanged;
    }

    public class AuthenticationEventArgs : System.EventArgs
    {
        public bool IsAuthenticated { get; }
        public string Username { get; }
        public string Role { get; }

        public AuthenticationEventArgs(bool isAuthenticated, string username = null, string role = null)
        {
            IsAuthenticated = isAuthenticated;
            Username = username;
            Role = role;
        }
    }
}
