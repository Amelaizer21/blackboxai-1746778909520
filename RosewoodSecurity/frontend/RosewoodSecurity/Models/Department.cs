using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace RosewoodSecurity.Models
{
    public class Department : INotifyPropertyChanged
    {
        private int _id;
        private string _name;
        private string _description;
        private int _accessLevel;
        private List<Employee> _employees;
        private List<Key> _keyPermissions;

        public event PropertyChangedEventHandler PropertyChanged;

        [JsonPropertyName("id")]
        public int Id
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }

        [JsonPropertyName("name")]
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        [JsonPropertyName("description")]
        public string Description
        {
            get => _description;
            set => SetProperty(ref _description, value);
        }

        [JsonPropertyName("access_level")]
        public int AccessLevel
        {
            get => _accessLevel;
            set => SetProperty(ref _accessLevel, value);
        }

        [JsonPropertyName("employees")]
        public List<Employee> Employees
        {
            get => _employees;
            set => SetProperty(ref _employees, value);
        }

        [JsonPropertyName("key_permissions")]
        public List<Key> KeyPermissions
        {
            get => _keyPermissions;
            set => SetProperty(ref _keyPermissions, value);
        }

        // Computed properties
        [JsonIgnore]
        public int EmployeeCount => Employees?.Count ?? 0;

        [JsonIgnore]
        public int KeyPermissionCount => KeyPermissions?.Count ?? 0;

        [JsonIgnore]
        public string AccessLevelDescription => AccessLevels.Descriptions.TryGetValue(AccessLevel, out var description) 
            ? description 
            : "Unknown access level";

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

    public class DepartmentCreateRequest
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("access_level")]
        public int AccessLevel { get; set; }
    }

    public class DepartmentUpdateRequest
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("access_level")]
        public int AccessLevel { get; set; }
    }

    public class DepartmentReport
    {
        [JsonPropertyName("department")]
        public Department Department { get; set; }

        [JsonPropertyName("employee_count")]
        public int EmployeeCount { get; set; }

        [JsonPropertyName("key_permissions")]
        public List<Key> KeyPermissions { get; set; }

        [JsonPropertyName("recent_transactions")]
        public List<Transaction> RecentTransactions { get; set; }

        [JsonPropertyName("active_checkouts")]
        public List<Transaction> ActiveCheckouts { get; set; }
    }

    public static class AccessLevels
    {
        public const int Basic = 1;
        public const int Intermediate = 2;
        public const int Advanced = 3;
        public const int Full = 4;

        public static readonly Dictionary<int, string> Descriptions = new Dictionary<int, string>
        {
            { Basic, "Basic access to common areas" },
            { Intermediate, "Access to department-specific areas" },
            { Advanced, "Access to sensitive areas" },
            { Full, "Full access to all areas" }
        };

        public static readonly Dictionary<int, string> Names = new Dictionary<int, string>
        {
            { Basic, "Basic" },
            { Intermediate, "Intermediate" },
            { Advanced, "Advanced" },
            { Full, "Full" }
        };

        public static string GetName(int level) => Names.TryGetValue(level, out var name) ? name : "Unknown";
        public static string GetDescription(int level) => Descriptions.TryGetValue(level, out var desc) ? desc : "Unknown access level";

        public static IEnumerable<(int Level, string Name, string Description)> GetAllLevels()
        {
            foreach (var level in Names.Keys)
            {
                yield return (level, Names[level], Descriptions[level]);
            }
        }
    }
}
