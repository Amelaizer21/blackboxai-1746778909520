using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace RosewoodSecurity.Models
{
    public class Transaction : INotifyPropertyChanged
    {
        private string _transactionNumber;
        private int _employeeId;
        private Employee _employee;
        private int? _keyId;
        private Key _key;
        private int? _accessCardId;
        private AccessCard _accessCard;
        private string _purpose;
        private DateTime _checkOutTime;
        private DateTime? _checkInTime;
        private DateTime? _expectedReturnTime;
        private string _status;
        private string _notes;
        private string _createdBy;
        private DateTime _createdAt;
        private DateTime _updatedAt;

        public event PropertyChangedEventHandler PropertyChanged;

        [JsonPropertyName("transaction_number")]
        public string TransactionNumber
        {
            get => _transactionNumber;
            set => SetProperty(ref _transactionNumber, value);
        }

        [JsonPropertyName("employee_id")]
        public int EmployeeId
        {
            get => _employeeId;
            set => SetProperty(ref _employeeId, value);
        }

        [JsonPropertyName("employee")]
        public Employee Employee
        {
            get => _employee;
            set => SetProperty(ref _employee, value);
        }

        [JsonPropertyName("key_id")]
        public int? KeyId
        {
            get => _keyId;
            set => SetProperty(ref _keyId, value);
        }

        [JsonPropertyName("key")]
        public Key Key
        {
            get => _key;
            set => SetProperty(ref _key, value);
        }

        [JsonPropertyName("access_card_id")]
        public int? AccessCardId
        {
            get => _accessCardId;
            set => SetProperty(ref _accessCardId, value);
        }

        [JsonPropertyName("access_card")]
        public AccessCard AccessCard
        {
            get => _accessCard;
            set => SetProperty(ref _accessCard, value);
        }

        [JsonPropertyName("purpose")]
        public string Purpose
        {
            get => _purpose;
            set => SetProperty(ref _purpose, value);
        }

        [JsonPropertyName("check_out_time")]
        public DateTime CheckOutTime
        {
            get => _checkOutTime;
            set => SetProperty(ref _checkOutTime, value);
        }

        [JsonPropertyName("check_in_time")]
        public DateTime? CheckInTime
        {
            get => _checkInTime;
            set => SetProperty(ref _checkInTime, value);
        }

        [JsonPropertyName("expected_return_time")]
        public DateTime? ExpectedReturnTime
        {
            get => _expectedReturnTime;
            set => SetProperty(ref _expectedReturnTime, value);
        }

        [JsonPropertyName("status")]
        public string Status
        {
            get => _status;
            set => SetProperty(ref _status, value);
        }

        [JsonPropertyName("notes")]
        public string Notes
        {
            get => _notes;
            set => SetProperty(ref _notes, value);
        }

        [JsonPropertyName("created_by")]
        public string CreatedBy
        {
            get => _createdBy;
            set => SetProperty(ref _createdBy, value);
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

        // Computed properties
        [JsonIgnore]
        public bool IsActive => !CheckInTime.HasValue;

        [JsonIgnore]
        public bool IsOverdue => ExpectedReturnTime.HasValue && 
            !CheckInTime.HasValue && 
            DateTime.UtcNow > ExpectedReturnTime.Value;

        [JsonIgnore]
        public TimeSpan? Duration => CheckInTime.HasValue 
            ? CheckInTime.Value - CheckOutTime 
            : DateTime.UtcNow - CheckOutTime;

        [JsonIgnore]
        public string ItemType => KeyId.HasValue ? "Key" : "Access Card";

        [JsonIgnore]
        public string ItemIdentifier => KeyId.HasValue 
            ? Key?.KeyNumber 
            : AccessCard?.CardNumber;

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

    public class CheckoutRequest
    {
        [JsonPropertyName("employee_id")]
        public int EmployeeId { get; set; }

        [JsonPropertyName("key_id")]
        public int? KeyId { get; set; }

        [JsonPropertyName("access_card_id")]
        public int? AccessCardId { get; set; }

        [JsonPropertyName("purpose")]
        public string Purpose { get; set; }

        [JsonPropertyName("expected_return_hours")]
        public double? ExpectedReturnHours { get; set; }
    }

    public static class TransactionStatus
    {
        public const string Active = "active";
        public const string Completed = "completed";
        public const string Overdue = "overdue";
        public const string Lost = "lost";
        public const string Cancelled = "cancelled";

        public static readonly string[] AllStatuses = new[]
        {
            Active,
            Completed,
            Overdue,
            Lost,
            Cancelled
        };
    }

    public class ActiveCheckout
    {
        [JsonPropertyName("transaction")]
        public Transaction Transaction { get; set; }

        [JsonPropertyName("days_overdue")]
        public int? DaysOverdue { get; set; }

        [JsonPropertyName("item_type")]
        public string ItemType { get; set; }

        [JsonPropertyName("item_identifier")]
        public string ItemIdentifier { get; set; }
    }
}
