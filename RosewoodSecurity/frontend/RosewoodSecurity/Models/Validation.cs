using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace RosewoodSecurity.Models
{
    public class ValidationResult
    {
        public bool IsValid => !Errors.Any();
        public List<ValidationError> Errors { get; } = new List<ValidationError>();

        public void AddError(string field, string message, string code = null)
        {
            Errors.Add(new ValidationError
            {
                Field = field,
                Message = message,
                Code = code
            });
        }

        public void Merge(ValidationResult other)
        {
            if (other?.Errors != null)
            {
                Errors.AddRange(other.Errors);
            }
        }
    }

    public static class Validator
    {
        public static class Rules
        {
            public static readonly Regex EmailRegex = new Regex(
                @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$",
                RegexOptions.Compiled);

            public static readonly Regex PhoneRegex = new Regex(
                @"^\+?[1-9]\d{1,14}$",
                RegexOptions.Compiled);

            public static readonly Regex PasswordRegex = new Regex(
                @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$",
                RegexOptions.Compiled);

            public static readonly Regex EmployeeNumberRegex = new Regex(
                @"^[A-Z]{2}\d{6}$",
                RegexOptions.Compiled);

            public static readonly Regex KeyNumberRegex = new Regex(
                @"^KEY-\d{6}$",
                RegexOptions.Compiled);

            public static readonly Regex CardNumberRegex = new Regex(
                @"^CARD-\d{8}$",
                RegexOptions.Compiled);
        }

        public static ValidationResult ValidateUser(User user)
        {
            var result = new ValidationResult();

            if (string.IsNullOrWhiteSpace(user.Username))
            {
                result.AddError("username", "Username is required");
            }
            else if (user.Username.Length < 3 || user.Username.Length > 50)
            {
                result.AddError("username", "Username must be between 3 and 50 characters");
            }

            if (string.IsNullOrWhiteSpace(user.Email))
            {
                result.AddError("email", "Email is required");
            }
            else if (!Rules.EmailRegex.IsMatch(user.Email))
            {
                result.AddError("email", "Invalid email format");
            }

            if (!UserRoles.AllRoles.Contains(user.Role))
            {
                result.AddError("role", "Invalid user role");
            }

            return result;
        }

        public static ValidationResult ValidateEmployee(Employee employee)
        {
            var result = new ValidationResult();

            if (string.IsNullOrWhiteSpace(employee.EmployeeNumber))
            {
                result.AddError("employee_number", "Employee number is required");
            }
            else if (!Rules.EmployeeNumberRegex.IsMatch(employee.EmployeeNumber))
            {
                result.AddError("employee_number", "Invalid employee number format (e.g., AB123456)");
            }

            if (string.IsNullOrWhiteSpace(employee.FirstName))
            {
                result.AddError("first_name", "First name is required");
            }

            if (string.IsNullOrWhiteSpace(employee.LastName))
            {
                result.AddError("last_name", "Last name is required");
            }

            if (string.IsNullOrWhiteSpace(employee.Email))
            {
                result.AddError("email", "Email is required");
            }
            else if (!Rules.EmailRegex.IsMatch(employee.Email))
            {
                result.AddError("email", "Invalid email format");
            }

            if (!string.IsNullOrWhiteSpace(employee.Phone) && !Rules.PhoneRegex.IsMatch(employee.Phone))
            {
                result.AddError("phone", "Invalid phone number format");
            }

            if (employee.DepartmentId <= 0)
            {
                result.AddError("department_id", "Department is required");
            }

            return result;
        }

        public static ValidationResult ValidateKey(Key key)
        {
            var result = new ValidationResult();

            if (string.IsNullOrWhiteSpace(key.KeyNumber))
            {
                result.AddError("key_number", "Key number is required");
            }
            else if (!Rules.KeyNumberRegex.IsMatch(key.KeyNumber))
            {
                result.AddError("key_number", "Invalid key number format (e.g., KEY-123456)");
            }

            if (string.IsNullOrWhiteSpace(key.Name))
            {
                result.AddError("name", "Key name is required");
            }

            if (string.IsNullOrWhiteSpace(key.Location))
            {
                result.AddError("location", "Location is required");
            }

            if (!KeyType.AllTypes.Contains(key.KeyType))
            {
                result.AddError("key_type", "Invalid key type");
            }

            return result;
        }

        public static ValidationResult ValidateAccessCard(AccessCard card)
        {
            var result = new ValidationResult();

            if (string.IsNullOrWhiteSpace(card.CardNumber))
            {
                result.AddError("card_number", "Card number is required");
            }
            else if (!Rules.CardNumberRegex.IsMatch(card.CardNumber))
            {
                result.AddError("card_number", "Invalid card number format (e.g., CARD-12345678)");
            }

            if (!CardType.AllTypes.Contains(card.CardType))
            {
                result.AddError("card_type", "Invalid card type");
            }

            if (card.AccessZones == null || !card.AccessZones.Any())
            {
                result.AddError("access_zones", "At least one access zone is required");
            }
            else if (card.AccessZones.Any(zone => !AccessZone.AllZones.Contains(zone)))
            {
                result.AddError("access_zones", "Invalid access zone specified");
            }

            if (card.ExpiryDate.HasValue && card.ExpiryDate.Value <= DateTime.UtcNow)
            {
                result.AddError("expiry_date", "Expiry date must be in the future");
            }

            return result;
        }

        public static ValidationResult ValidateTransaction(Transaction transaction)
        {
            var result = new ValidationResult();

            if (transaction.EmployeeId <= 0)
            {
                result.AddError("employee_id", "Employee is required");
            }

            if (!transaction.KeyId.HasValue && !transaction.AccessCardId.HasValue)
            {
                result.AddError("item", "Either key or access card must be specified");
            }

            if (transaction.KeyId.HasValue && transaction.AccessCardId.HasValue)
            {
                result.AddError("item", "Cannot checkout both key and access card in same transaction");
            }

            if (string.IsNullOrWhiteSpace(transaction.Purpose))
            {
                result.AddError("purpose", "Purpose is required");
            }

            if (transaction.ExpectedReturnTime.HasValue && 
                transaction.ExpectedReturnTime.Value <= transaction.CheckOutTime)
            {
                result.AddError("expected_return_time", "Expected return time must be after checkout time");
            }

            return result;
        }

        public static ValidationResult ValidatePassword(string password)
        {
            var result = new ValidationResult();

            if (string.IsNullOrWhiteSpace(password))
            {
                result.AddError("password", "Password is required");
            }
            else if (!Rules.PasswordRegex.IsMatch(password))
            {
                result.AddError("password", 
                    "Password must be at least 8 characters long and contain uppercase, " +
                    "lowercase, number and special character");
            }

            return result;
        }
    }
}
