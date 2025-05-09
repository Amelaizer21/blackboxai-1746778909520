using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Security.Claims;

namespace RosewoodSecurity.Models
{
    public static class Extensions
    {
        // Collection Extensions
        public static ObservableCollection<T> ToObservableCollection<T>(this IEnumerable<T> source)
        {
            return new ObservableCollection<T>(source);
        }

        public static void AddRange<T>(this ObservableCollection<T> collection, IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                collection.Add(item);
            }
        }

        public static void RemoveRange<T>(this ObservableCollection<T> collection, IEnumerable<T> items)
        {
            foreach (var item in items.ToList())
            {
                collection.Remove(item);
            }
        }

        // DateTime Extensions
        public static string ToRelativeTime(this DateTime dateTime)
        {
            var timeSpan = DateTime.UtcNow - dateTime;

            if (timeSpan <= TimeSpan.FromSeconds(60))
                return "just now";
            if (timeSpan <= TimeSpan.FromMinutes(60))
                return $"{timeSpan.Minutes} minutes ago";
            if (timeSpan <= TimeSpan.FromHours(24))
                return $"{timeSpan.Hours} hours ago";
            if (timeSpan <= TimeSpan.FromDays(30))
                return $"{timeSpan.Days} days ago";
            if (timeSpan <= TimeSpan.FromDays(365))
                return $"{timeSpan.Days / 30} months ago";

            return $"{timeSpan.Days / 365} years ago";
        }

        public static bool IsExpired(this DateTime expiryDate)
        {
            return expiryDate < DateTime.UtcNow;
        }

        public static bool IsExpiringSoon(this DateTime expiryDate, int daysThreshold = 30)
        {
            var timeUntilExpiry = expiryDate - DateTime.UtcNow;
            return timeUntilExpiry.TotalDays <= daysThreshold && timeUntilExpiry.TotalDays > 0;
        }

        // String Extensions
        public static string Truncate(this string value, int maxLength, string suffix = "...")
        {
            if (string.IsNullOrEmpty(value)) return value;
            return value.Length <= maxLength ? value : value.Substring(0, maxLength) + suffix;
        }

        public static string ToTitleCase(this string value)
        {
            if (string.IsNullOrEmpty(value)) return value;
            var words = value.Split(' ');
            var titleCase = words.Select(w => char.ToUpper(w[0]) + w.Substring(1).ToLower());
            return string.Join(" ", titleCase);
        }

        public static string ToSnakeCase(this string value)
        {
            if (string.IsNullOrEmpty(value)) return value;
            return string.Concat(value.Select((x, i) => i > 0 && char.IsUpper(x) ? "_" + x.ToString() : x.ToString())).ToLower();
        }

        // Color Extensions
        public static Color ToColor(this string hexColor)
        {
            return (Color)ColorConverter.ConvertFromString(hexColor);
        }

        public static SolidColorBrush ToBrush(this string hexColor)
        {
            return new SolidColorBrush(hexColor.ToColor());
        }

        public static string ToHexString(this Color color)
        {
            return $"#{color.R:X2}{color.G:X2}{color.B:X2}";
        }

        // Enum Extensions
        public static string GetDescription(this Enum value)
        {
            var field = value.GetType().GetField(value.ToString());
            var attribute = (System.ComponentModel.DescriptionAttribute)Attribute.GetCustomAttribute(field, typeof(System.ComponentModel.DescriptionAttribute));
            return attribute?.Description ?? value.ToString();
        }

        // Claims Extensions
        public static string GetUserId(this ClaimsPrincipal principal)
        {
            return principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }

        public static string GetUserRole(this ClaimsPrincipal principal)
        {
            return principal.FindFirst(ClaimTypes.Role)?.Value;
        }

        public static bool HasPermission(this ClaimsPrincipal principal, string permission)
        {
            return principal.HasClaim("permission", permission);
        }

        // Validation Extensions
        public static bool IsValidEmail(this string email)
        {
            return !string.IsNullOrEmpty(email) && Validator.Rules.EmailRegex.IsMatch(email);
        }

        public static bool IsValidPhone(this string phone)
        {
            return !string.IsNullOrEmpty(phone) && Validator.Rules.PhoneRegex.IsMatch(phone);
        }

        public static bool IsValidPassword(this string password)
        {
            return !string.IsNullOrEmpty(password) && Validator.Rules.PasswordRegex.IsMatch(password);
        }

        // UI Extensions
        public static void SetBusy(this FrameworkElement element, bool isBusy)
        {
            element.IsEnabled = !isBusy;
            element.Opacity = isBusy ? 0.5 : 1.0;
        }

        public static T FindParent<T>(this DependencyObject child) where T : DependencyObject
        {
            var parent = VisualTreeHelper.GetParent(child);
            while (parent != null && !(parent is T))
            {
                parent = VisualTreeHelper.GetParent(parent);
            }
            return parent as T;
        }

        public static T FindChild<T>(this DependencyObject parent) where T : DependencyObject
        {
            if (parent == null) return null;

            T foundChild = null;
            var childrenCount = VisualTreeHelper.GetChildrenCount(parent);
            for (var i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T)
                {
                    foundChild = (T)child;
                    break;
                }
                foundChild = FindChild<T>(child);
                if (foundChild != null) break;
            }
            return foundChild;
        }

        // Error Handling Extensions
        public static string GetFriendlyErrorMessage(this Exception ex)
        {
            return ex switch
            {
                ApiException apiEx => apiEx.Message,
                AuthenticationException => Constants.Errors.AuthenticationError,
                AuthorizationException => Constants.Errors.AuthorizationError,
                ValidationException => Constants.Errors.ValidationError,
                ConcurrencyException => Constants.Errors.ConcurrencyError,
                OfflineModeException => Constants.Errors.NetworkError,
                _ => Constants.Errors.ServerError
            };
        }
    }
}
