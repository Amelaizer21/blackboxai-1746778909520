using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace RosewoodSecurity.Models
{
    public class Key : INotifyPropertyChanged
    {
        private int _id;
        private string _keyNumber;
        private string _name;
        private string _location;
        private string _description;
        private string _keyType;
        private string _status;
        private string _photoUrl;
        private DateTime? _lastMaintenance;
        private List<Department> _authorizedDepartments;
        private List<Transaction> _transactions;

        public event PropertyChangedEventHandler PropertyChanged;

        [JsonPropertyName("id")]
        public int Id
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }

        [JsonPropertyName("key_number")]
        public string KeyNumber
        {
            get => _keyNumber;
            set => SetProperty(ref _keyNumber, value);
        }

        [JsonPropertyName("name")]
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        [JsonPropertyName("location")]
        public string Location
        {
            get => _location;
            set => SetProperty(ref _location, value);
        }

        [JsonPropertyName("description")]
        public string Description
        {
            get => _description;
            set => SetProperty(ref _description, value);
        }

        [JsonPropertyName("key_type")]
        public string KeyType
        {
            get => _keyType;
            set => SetProperty(ref _keyType, value);
        }

        [JsonPropertyName("status")]
        public string Status
        {
            get => _status;
            set => SetProperty(ref _status, value);
        }

        [JsonPropertyName("photo_url")]
        public string PhotoUrl
        {
            get => _photoUrl;
            set => SetProperty(ref _photoUrl, value);
        }

        [JsonPropertyName("last_maintenance")]
        public DateTime? LastMaintenance
        {
            get => _lastMaintenance;
            set => SetProperty(ref _lastMaintenance, value);
        }

        [JsonPropertyName("authorized_departments")]
        public List<Department> AuthorizedDepartments
        {
            get => _authorizedDepartments;
            set => SetProperty(ref _authorizedDepartments, value);
        }

        [JsonPropertyName("transactions")]
        public List<Transaction> Transactions
        {
            get => _transactions;
            set => SetProperty(ref _transactions, value);
        }

        // Computed properties
        [JsonIgnore]
        public bool IsAvailable => Status?.ToLower() == "available";

        [JsonIgnore]
        public bool IsCheckedOut => Status?.ToLower() == "checked_out";

        [JsonIgnore]
        public bool IsLost => Status?.ToLower() == "lost";

        [JsonIgnore]
        public bool IsRetired => Status?.ToLower() == "retired";

        [JsonIgnore]
        public bool NeedsMaintenanceCheck => LastMaintenance.HasValue && 
            (DateTime.UtcNow - LastMaintenance.Value).TotalDays > 90;

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

    public class KeyCreateRequest
    {
        [JsonPropertyName("key_number")]
        public string KeyNumber { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("location")]
        public string Location { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("key_type")]
        public string KeyType { get; set; }

        [JsonPropertyName("photo_url")]
        public string PhotoUrl { get; set; }

        [JsonPropertyName("department_ids")]
        public List<int> DepartmentIds { get; set; }
    }

    public class KeyUpdateRequest
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("location")]
        public string Location { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("key_type")]
        public string KeyType { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; }

        [JsonPropertyName("photo_url")]
        public string PhotoUrl { get; set; }

        [JsonPropertyName("department_ids")]
        public List<int> DepartmentIds { get; set; }

        [JsonPropertyName("maintenance_performed")]
        public bool MaintenancePerformed { get; set; }
    }

    public static class KeyStatus
    {
        public const string Available = "available";
        public const string CheckedOut = "checked_out";
        public const string Lost = "lost";
        public const string Retired = "retired";
        public const string UnderMaintenance = "under_maintenance";

        public static readonly string[] AllStatuses = new[]
        {
            Available,
            CheckedOut,
            Lost,
            Retired,
            UnderMaintenance
        };
    }

    public static class KeyType
    {
        public const string Regular = "regular";
        public const string Master = "master";
        public const string Restricted = "restricted";
        public const string Emergency = "emergency";

        public static readonly string[] AllTypes = new[]
        {
            Regular,
            Master,
            Restricted,
            Emergency
        };

        public static readonly Dictionary<string, string> Descriptions = new Dictionary<string, string>
        {
            { Regular, "Standard key for regular access" },
            { Master, "Master key with access to multiple areas" },
            { Restricted, "Restricted key with limited access" },
            { Emergency, "Emergency key for critical situations" }
        };
    }
}
