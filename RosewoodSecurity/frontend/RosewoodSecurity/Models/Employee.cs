using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace RosewoodSecurity.Models
{
    public class Employee : INotifyPropertyChanged
    {
        private int _id;
        private string _employeeNumber;
        private string _firstName;
        private string _lastName;
        private string _email;
        private string _phone;
        private string _photoUrl;
        private string _status;
        private int _departmentId;
        private Department _department;
        private DateTime _createdAt;
        private DateTime _updatedAt;
        private List<Transaction> _transactions;

        public event PropertyChangedEventHandler PropertyChanged;

        [JsonPropertyName("id")]
        public int Id
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }

        [JsonPropertyName("employee_number")]
        public string EmployeeNumber
        {
            get => _employeeNumber;
            set => SetProperty(ref _employeeNumber, value);
        }

        [JsonPropertyName("first_name")]
        public string FirstName
        {
            get => _firstName;
            set => SetProperty(ref _firstName, value);
        }

        [JsonPropertyName("last_name")]
        public string LastName
        {
            get => _lastName;
            set => SetProperty(ref _lastName, value);
        }

        [JsonPropertyName("email")]
        public string Email
        {
            get => _email;
            set => SetProperty(ref _email, value);
        }

        [JsonPropertyName("phone")]
        public string Phone
        {
            get => _phone;
            set => SetProperty(ref _phone, value);
        }

        [JsonPropertyName("photo_url")]
        public string PhotoUrl
        {
            get => _photoUrl;
            set => SetProperty(ref _photoUrl, value);
        }

        [JsonPropertyName("status")]
        public string Status
        {
            get => _status;
            set => SetProperty(ref _status, value);
        }

        [JsonPropertyName("department_id")]
        public int DepartmentId
        {
            get => _departmentId;
            set => SetProperty(ref _departmentId, value);
        }

        [JsonPropertyName("department")]
        public Department Department
        {
            get => _department;
            set => SetProperty(ref _department, value);
        }

        [JsonPropertyName("created_at")]
        public DateTime CreatedAt
        {
            get => _createdAt;
            set => SetProperty(ref _createdAt, value);
        }

        [JsonPropertyName("updated_at")]
        public DateTime UpdatedAt
        {
            get => _updatedAt;
            set => SetProperty(ref _updatedAt, value);
        }

        [JsonPropertyName("transactions")]
        public List<Transaction> Transactions
        {
            get => _transactions;
            set => SetProperty(ref _transactions, value);
        }

        // Computed properties
        [JsonIgnore]
        public string FullName => $"{FirstName} {LastName}";

        [JsonIgnore]
        public bool IsActive => Status?.ToLower() == "active";

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

    public class EmployeeCreateRequest
    {
        [JsonPropertyName("employee_number")]
        public string EmployeeNumber { get; set; }

        [JsonPropertyName("first_name")]
        public string FirstName { get; set; }

        [JsonPropertyName("last_name")]
        public string LastName { get; set; }

        [JsonPropertyName("email")]
        public string Email { get; set; }

        [JsonPropertyName("phone")]
        public string Phone { get; set; }

        [JsonPropertyName("department_id")]
        public int DepartmentId { get; set; }

        [JsonPropertyName("photo_url")]
        public string PhotoUrl { get; set; }
    }

    public class EmployeeUpdateRequest
    {
        [JsonPropertyName("first_name")]
        public string FirstName { get; set; }

        [JsonPropertyName("last_name")]
        public string LastName { get; set; }

        [JsonPropertyName("email")]
        public string Email { get; set; }

        [JsonPropertyName("phone")]
        public string Phone { get; set; }

        [JsonPropertyName("department_id")]
        public int DepartmentId { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; }

        [JsonPropertyName("photo_url")]
        public string PhotoUrl { get; set; }
    }

    public static class EmployeeStatus
    {
        public const string Active = "active";
        public const string Inactive = "inactive";
        public const string Suspended = "suspended";

        public static readonly string[] AllStatuses = new[]
        {
            Active,
            Inactive,
            Suspended
        };
    }
}
