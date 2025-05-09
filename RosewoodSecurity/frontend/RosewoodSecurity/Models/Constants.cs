namespace RosewoodSecurity.Models
{
    public static class Constants
    {
        public static class App
        {
            public const string Name = "Rosewood Security";
            public const string Version = "1.0.0";
            public const string Company = "Rosewood Doha";
            public const string Copyright = "© 2024 Rosewood Doha. All rights reserved.";
        }

        public static class Routes
        {
            public const string Login = "Login";
            public const string Dashboard = "Dashboard";
            public const string Keys = "Keys";
            public const string AccessCards = "AccessCards";
            public const string Employees = "Employees";
            public const string Departments = "Departments";
            public const string Transactions = "Transactions";
            public const string Reports = "Reports";
            public const string Settings = "Settings";
            public const string Profile = "Profile";
        }

        public static class Permissions
        {
            public const string ManageUsers = "manage_users";
            public const string ManageKeys = "manage_keys";
            public const string ManageAccessCards = "manage_access_cards";
            public const string ManageEmployees = "manage_employees";
            public const string ManageDepartments = "manage_departments";
            public const string ViewReports = "view_reports";
            public const string PerformCheckout = "perform_checkout";
            public const string ExtendCheckout = "extend_checkout";
            public const string MarkLost = "mark_lost";
            public const string ViewAuditTrail = "view_audit_trail";
        }

        public static class Storage
        {
            public const string AuthToken = "auth_token";
            public const string RefreshToken = "refresh_token";
            public const string UserProfile = "user_profile";
            public const string Theme = "app_theme";
            public const string Settings = "app_settings";
            public const string LastSync = "last_sync";
        }

        public static class Formats
        {
            public const string ShortDate = "MM/dd/yyyy";
            public const string LongDate = "MMMM dd, yyyy";
            public const string ShortTime = "HH:mm";
            public const string LongTime = "HH:mm:ss";
            public const string DateTime = "MM/dd/yyyy HH:mm";
            public const string Currency = "C2";
            public const string Percentage = "P0";
        }

        public static class Validation
        {
            public const int MinPasswordLength = 8;
            public const int MaxPasswordLength = 128;
            public const int MinUsernameLength = 3;
            public const int MaxUsernameLength = 50;
            public const int MaxEmailLength = 254;
            public const int MaxPhoneLength = 15;
            public const int MaxNameLength = 100;
            public const int MaxDescriptionLength = 500;
            public const int MaxNotesLength = 1000;
        }

        public static class UI
        {
            public const int DefaultPageSize = 20;
            public const int MaxPageSize = 100;
            public const int MinSearchLength = 3;
            public const int AutoCompleteDelay = 300;
            public const int ToastDuration = 3000;
            public const int DialogWidth = 500;
            public const int DialogHeight = 400;
            public const int SidebarWidth = 250;
            public const int HeaderHeight = 60;
            public const int FooterHeight = 40;
        }

        public static class Api
        {
            public const int DefaultTimeout = 30;
            public const int MaxRetries = 3;
            public const int RetryDelay = 1000;
            public const string JsonContentType = "application/json";
            public const string AuthorizationHeader = "Authorization";
            public const string BearerScheme = "Bearer";
        }

        public static class Cache
        {
            public const int DefaultExpirationMinutes = 5;
            public const int UserProfileExpirationMinutes = 30;
            public const int SettingsExpirationMinutes = 60;
            public const int LookupDataExpirationMinutes = 120;
        }

        public static class Errors
        {
            public const string NetworkError = "Unable to connect to the server. Please check your internet connection.";
            public const string ServerError = "An unexpected server error occurred. Please try again later.";
            public const string AuthenticationError = "Invalid username or password.";
            public const string AuthorizationError = "You don't have permission to perform this action.";
            public const string ValidationError = "Please correct the validation errors and try again.";
            public const string NotFoundError = "The requested resource was not found.";
            public const string ConcurrencyError = "The data has been modified by another user. Please refresh and try again.";
            public const string TimeoutError = "The request timed out. Please try again.";
        }

        public static class Events
        {
            public const string UserLoggedIn = "UserLoggedIn";
            public const string UserLoggedOut = "UserLoggedOut";
            public const string SettingsChanged = "SettingsChanged";
            public const string ThemeChanged = "ThemeChanged";
            public const string DataRefreshed = "DataRefreshed";
            public const string ConnectionStateChanged = "ConnectionStateChanged";
            public const string NotificationReceived = "NotificationReceived";
        }

        public static class Colors
        {
            public const string Primary = "#1976D2";
            public const string Secondary = "#424242";
            public const string Success = "#4CAF50";
            public const string Warning = "#FFC107";
            public const string Error = "#F44336";
            public const string Info = "#2196F3";
            public const string Background = "#FFFFFF";
            public const string Surface = "#FFFFFF";
            public const string Text = "#000000";
        }
    }
}
