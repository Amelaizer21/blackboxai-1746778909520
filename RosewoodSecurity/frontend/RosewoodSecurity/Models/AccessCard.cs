using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace RosewoodSecurity.Models
{
    public class AccessCard : INotifyPropertyChanged
    {
        private int _id;
        private string _cardNumber;
        private string _cardType;
        private List<string> _accessZones;
        private string _status;
        private DateTime? _expiryDate;
        private DateTime? _lastUsed;
        private int? _employeeId;
        private Employee _assignedEmployee;
        private List<Transaction> _transactions;

        public event PropertyChangedEventHandler PropertyChanged;

        [JsonPropertyName("id")]
        public int Id
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }

        [JsonPropertyName("card_number")]
        public string CardNumber
        {
            get => _cardNumber;
            set => SetProperty(ref _cardNumber, value);
        }

        [JsonPropertyName("card_type")]
        public string CardType
        {
            get => _cardType;
            set => SetProperty(ref _cardType, value);
        }

        [JsonPropertyName("access_zones")]
        public List<string> AccessZones
        {
            get => _accessZones;
            set => SetProperty(ref _accessZones, value);
        }

        [JsonPropertyName("status")]
        public string Status
        {
            get => _status;
            set => SetProperty(ref _status, value);
        }

        [JsonPropertyName("expiry_date")]
        public DateTime? ExpiryDate
        {
            get => _expiryDate;
            set => SetProperty(ref _expiryDate, value);
        }

        [JsonPropertyName("last_used")]
        public DateTime? LastUsed
        {
            get => _lastUsed;
            set => SetProperty(ref _lastUsed, value);
        }

        [JsonPropertyName("employee_id")]
        public int? EmployeeId
        {
            get => _employeeId;
            set => SetProperty(ref _employeeId, value);
        }

        [JsonPropertyName("assigned_employee")]
        public Employee AssignedEmployee
        {
            get => _assignedEmployee;
            set => SetProperty(ref _assignedEmployee, value);
        }

        [JsonPropertyName("transactions")]
        public List<Transaction> Transactions
        {
            get => _transactions;
            set => SetProperty(ref _transactions, value);
        }

        // Computed properties
        [JsonIgnore]
        public bool IsActive => Status?.ToLower() == "active";

        [JsonIgnore]
        public bool IsExpired => ExpiryDate.HasValue && ExpiryDate.Value < DateTime.UtcNow;

        [JsonIgnore]
        public bool IsAssigned => EmployeeId.HasValue;

        [JsonIgnore]
        public int DaysUntilExpiry => ExpiryDate.HasValue 
            ? (ExpiryDate.Value - DateTime.UtcNow).Days 
            : -1;

        [JsonIgnore]
        public bool ExpiringWithin30Days => ExpiryDate.HasValue && 
            DaysUntilExpiry is >= 0 and <= 30;

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

    public class AccessCardCreateRequest
    {
        [JsonPropertyName("card_number")]
        public string CardNumber { get; set; }

        [JsonPropertyName("card_type")]
        public string CardType { get; set; }

        [JsonPropertyName("access_zones")]
        public List<string> AccessZones { get; set; }

        [JsonPropertyName("employee_id")]
        public int? EmployeeId { get; set; }

        [JsonPropertyName("expiry_date")]
        public DateTime? ExpiryDate { get; set; }
    }

    public class AccessCardUpdateRequest
    {
        [JsonPropertyName("card_type")]
        public string CardType { get; set; }

        [JsonPropertyName("access_zones")]
        public List<string> AccessZones { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; }

        [JsonPropertyName("employee_id")]
        public int? EmployeeId { get; set; }

        [JsonPropertyName("expiry_date")]
        public DateTime? ExpiryDate { get; set; }
    }

    public static class CardStatus
    {
        public const string Active = "active";
        public const string Inactive = "inactive";
        public const string Lost = "lost";
        public const string Expired = "expired";
        public const string Deactivated = "deactivated";

        public static readonly string[] AllStatuses = new[]
        {
            Active,
            Inactive,
            Lost,
            Expired,
            Deactivated
        };
    }

    public static class CardType
    {
        public const string Standard = "standard";
        public const string Temporary = "temporary";
        public const string Visitor = "visitor";
        public const string Contractor = "contractor";
        public const string VIP = "vip";

        public static readonly string[] AllTypes = new[]
        {
            Standard,
            Temporary,
            Visitor,
            Contractor,
            VIP
        };

        public static readonly Dictionary<string, string> Descriptions = new Dictionary<string, string>
        {
            { Standard, "Standard employee access card" },
            { Temporary, "Temporary access card with limited duration" },
            { Visitor, "Visitor access card with restricted access" },
            { Contractor, "Contractor access card with specific permissions" },
            { VIP, "VIP access card with extended privileges" }
        };
    }

    public static class AccessZone
    {
        public const string Main = "main";
        public const string Office = "office";
        public const string Restricted = "restricted";
        public const string Security = "security";
        public const string Parking = "parking";

        public static readonly string[] AllZones = new[]
        {
            Main,
            Office,
            Restricted,
            Security,
            Parking
        };

        public static readonly Dictionary<string, string> Descriptions = new Dictionary<string, string>
        {
            { Main, "Main building access" },
            { Office, "Office area access" },
            { Restricted, "Restricted area access" },
            { Security, "Security area access" },
            { Parking, "Parking area access" }
        };
    }
}
