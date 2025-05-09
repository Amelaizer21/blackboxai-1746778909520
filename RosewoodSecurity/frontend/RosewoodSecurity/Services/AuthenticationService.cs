using System;
using System.Threading.Tasks;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Linq;
using MaterialDesignThemes.Wpf;

namespace RosewoodSecurity.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly IApiService _apiService;
        private readonly ISettingsService _settingsService;
        private readonly ISnackbarMessageQueue _messageQueue;
        private AuthenticationResult _currentAuthState;
        private readonly string _authStateKey = "AuthState";

        public event EventHandler<AuthenticationEventArgs> AuthenticationChanged;

        public AuthenticationService(
            IApiService apiService,
            ISettingsService settingsService,
            ISnackbarMessageQueue messageQueue)
        {
            _apiService = apiService;
            _settingsService = settingsService;
            _messageQueue = messageQueue;

            // Try to restore authentication state
            _currentAuthState = LoadAuthenticationState();
            if (_currentAuthState?.Success == true && !IsTokenExpired(_currentAuthState.Token))
            {
                IsAuthenticated = true;
                CurrentUser = _currentAuthState.User;
            }
        }

        public bool IsAuthenticated { get; private set; }
        public UserInfo CurrentUser { get; private set; }
        public string AuthToken => _currentAuthState?.Token;

        public async Task<bool> InitiateLoginAsync(string username, string password)
        {
            try
            {
                var result = await _apiService.LoginAsync(username, password);
                
                if (result.Success && !result.RequiresTwoFactor)
                {
                    CompleteAuthentication(result);
                    return true;
                }
                
                if (result.RequiresTwoFactor)
                {
                    _currentAuthState = result;
                    return true;
                }

                _messageQueue.Enqueue(result.ErrorMessage ?? "Login failed",
                    null, null, null, false, true, TimeSpan.FromSeconds(3));
                return false;
            }
            catch (Exception ex)
            {
                _messageQueue.Enqueue($"Login error: {ex.Message}",
                    null, null, null, false, true, TimeSpan.FromSeconds(3));
                return false;
            }
        }

        public async Task<bool> ValidateTwoFactorAsync(string code)
        {
            try
            {
                if (_currentAuthState == null || !_currentAuthState.RequiresTwoFactor)
                {
                    return false;
                }

                var result = await _apiService.ValidateTwoFactorAsync(code);
                if (result)
                {
                    CompleteAuthentication(_currentAuthState);
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                _messageQueue.Enqueue($"2FA validation error: {ex.Message}",
                    null, null, null, false, true, TimeSpan.FromSeconds(3));
                return false;
            }
        }

        public async Task<bool> LoginWithTokenAsync(string token)
        {
            if (IsTokenExpired(token))
            {
                return false;
            }

            try
            {
                var principal = GetPrincipalFromToken(token);
                if (principal == null)
                {
                    return false;
                }

                var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return false;
                }

                // Verify token with API
                var result = await _apiService.LoginAsync(token, null);
                if (result.Success)
                {
                    CompleteAuthentication(result);
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                _messageQueue.Enqueue($"Token login error: {ex.Message}",
                    null, null, null, false, true, TimeSpan.FromSeconds(3));
                return false;
            }
        }

        public async Task<bool> RefreshTokenAsync()
        {
            if (_currentAuthState?.RefreshToken == null)
            {
                return false;
            }

            try
            {
                var result = await _apiService.LoginAsync(null, null, _currentAuthState.RefreshToken);
                if (result.Success)
                {
                    CompleteAuthentication(result);
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                _messageQueue.Enqueue($"Token refresh error: {ex.Message}",
                    null, null, null, false, true, TimeSpan.FromSeconds(3));
                return false;
            }
        }

        public void Logout()
        {
            IsAuthenticated = false;
            CurrentUser = null;
            _currentAuthState = null;
            ClearAuthenticationState();

            OnAuthenticationChanged(false, null);
        }

        public bool HasPermission(string permission)
        {
            return CurrentUser?.Permissions?.Contains(permission) ?? false;
        }

        public bool IsInRole(string role)
        {
            return CurrentUser?.Role == role;
        }

        public async Task<bool> ChangePasswordAsync(string currentPassword, string newPassword)
        {
            try
            {
                var result = await _apiService.ChangePasswordAsync(currentPassword, newPassword);
                if (result.Success)
                {
                    _messageQueue.Enqueue("Password changed successfully",
                        null, null, null, false, true, TimeSpan.FromSeconds(3));

                    if (result.RequiresRelogin)
                    {
                        Logout();
                    }

                    return true;
                }

                _messageQueue.Enqueue(result.Message ?? "Failed to change password",
                    null, null, null, false, true, TimeSpan.FromSeconds(3));
                return false;
            }
            catch (Exception ex)
            {
                _messageQueue.Enqueue($"Password change error: {ex.Message}",
                    null, null, null, false, true, TimeSpan.FromSeconds(3));
                return false;
            }
        }

        public async Task<bool> RequestPasswordResetAsync(string email)
        {
            try
            {
                return await _apiService.RequestPasswordResetAsync(email);
            }
            catch (Exception ex)
            {
                _messageQueue.Enqueue($"Password reset request error: {ex.Message}",
                    null, null, null, false, true, TimeSpan.FromSeconds(3));
                return false;
            }
        }

        public async Task<bool> ResetPasswordAsync(string token, string newPassword)
        {
            try
            {
                return await _apiService.ResetPasswordAsync(token, newPassword);
            }
            catch (Exception ex)
            {
                _messageQueue.Enqueue($"Password reset error: {ex.Message}",
                    null, null, null, false, true, TimeSpan.FromSeconds(3));
                return false;
            }
        }

        public void SaveAuthenticationState(AuthenticationResult result)
        {
            try
            {
                var json = System.Text.Json.JsonSerializer.Serialize(result);
                _settingsService.SetSetting(_authStateKey, json);
                _settingsService.SaveSettings();
            }
            catch (Exception ex)
            {
                _messageQueue.Enqueue($"Failed to save authentication state: {ex.Message}",
                    null, null, null, false, true, TimeSpan.FromSeconds(3));
            }
        }

        public AuthenticationResult LoadAuthenticationState()
        {
            try
            {
                var json = _settingsService.GetSetting<string>(_authStateKey);
                if (string.IsNullOrEmpty(json))
                {
                    return null;
                }

                return System.Text.Json.JsonSerializer.Deserialize<AuthenticationResult>(json);
            }
            catch (Exception ex)
            {
                _messageQueue.Enqueue($"Failed to load authentication state: {ex.Message}",
                    null, null, null, false, true, TimeSpan.FromSeconds(3));
                return null;
            }
        }

        public void ClearAuthenticationState()
        {
            try
            {
                _settingsService.RemoveSetting(_authStateKey);
                _settingsService.SaveSettings();
            }
            catch (Exception ex)
            {
                _messageQueue.Enqueue($"Failed to clear authentication state: {ex.Message}",
                    null, null, null, false, true, TimeSpan.FromSeconds(3));
            }
        }

        private void CompleteAuthentication(AuthenticationResult result)
        {
            _currentAuthState = result;
            IsAuthenticated = true;
            CurrentUser = result.User;

            SaveAuthenticationState(result);
            OnAuthenticationChanged(true, result.User);
        }

        private void OnAuthenticationChanged(bool isAuthenticated, UserInfo user)
        {
            AuthenticationChanged?.Invoke(this, new AuthenticationEventArgs
            {
                IsAuthenticated = isAuthenticated,
                CurrentUser = user
            });
        }

        private bool IsTokenExpired(string token)
        {
            var principal = GetPrincipalFromToken(token);
            if (principal == null)
            {
                return true;
            }

            var expClaim = principal.FindFirst(JwtRegisteredClaimNames.Exp);
            if (expClaim == null)
            {
                return true;
            }

            var expiration = DateTimeOffset.FromUnixTimeSeconds(long.Parse(expClaim.Value));
            return expiration <= DateTimeOffset.UtcNow;
        }

        private ClaimsPrincipal GetPrincipalFromToken(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_settingsService.GetSetting<string>("Security:JwtKey"));
                
                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _settingsService.GetSetting<string>("Security:JwtIssuer"),
                    ValidateAudience = true,
                    ValidAudience = _settingsService.GetSetting<string>("Security:JwtAudience"),
                    ValidateLifetime = false // We check expiration separately
                };

                var principal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);
                return principal;
            }
            catch
            {
                return null;
            }
        }
    }
}
