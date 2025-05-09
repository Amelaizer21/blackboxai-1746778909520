using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace RosewoodSecurity.Models
{
    public class DashboardStats
    {
        [JsonPropertyName("total_employees")]
        public int TotalEmployees { get; set; }

        [JsonPropertyName("active_employees")]
        public int ActiveEmployees { get; set; }

        [JsonPropertyName("total_keys")]
        public int TotalKeys { get; set; }

        [JsonPropertyName("available_keys")]
        public int AvailableKeys { get; set; }

        [JsonPropertyName("checked_out_keys")]
        public int CheckedOutKeys { get; set; }

        [JsonPropertyName("lost_keys")]
        public int LostKeys { get; set; }

        [JsonPropertyName("total_access_cards")]
        public int TotalAccessCards { get; set; }

        [JsonPropertyName("active_access_cards")]
        public int ActiveAccessCards { get; set; }

        [JsonPropertyName("expiring_cards")]
        public int ExpiringCards { get; set; }

        [JsonPropertyName("active_transactions")]
        public int ActiveTransactions { get; set; }

        [JsonPropertyName("overdue_transactions")]
        public int OverdueTransactions { get; set; }

        [JsonPropertyName("transactions_today")]
        public int TransactionsToday { get; set; }

        [JsonPropertyName("departments_summary")]
        public List<DepartmentSummary> DepartmentsSummary { get; set; }

        [JsonPropertyName("recent_activities")]
        public List<ActivityLog> RecentActivities { get; set; }

        [JsonPropertyName("alerts")]
        public List<Alert> Alerts { get; set; }
    }

    public class DepartmentSummary
    {
        [JsonPropertyName("department_id")]
        public int DepartmentId { get; set; }

        [JsonPropertyName("department_name")]
        public string DepartmentName { get; set; }

        [JsonPropertyName("employee_count")]
        public int EmployeeCount { get; set; }

        [JsonPropertyName("active_checkouts")]
        public int ActiveCheckouts { get; set; }

        [JsonPropertyName("overdue_items")]
        public int OverdueItems { get; set; }
    }

    public class ActivityLog
    {
        [JsonPropertyName("timestamp")]
        public DateTime Timestamp { get; set; }

        [JsonPropertyName("activity_type")]
        public string ActivityType { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("user")]
        public string User { get; set; }

        [JsonPropertyName("details")]
        public Dictionary<string, object> Details { get; set; }
    }

    public class Alert
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("alert_type")]
        public string AlertType { get; set; }

        [JsonPropertyName("severity")]
        public string Severity { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; }

        [JsonPropertyName("created_at")]
        public DateTime CreatedAt { get; set; }

        [JsonPropertyName("is_resolved")]
        public bool IsResolved { get; set; }

        [JsonPropertyName("resolved_at")]
        public DateTime? ResolvedAt { get; set; }

        [JsonPropertyName("resolved_by")]
        public string ResolvedBy { get; set; }

        [JsonPropertyName("related_entity_type")]
        public string RelatedEntityType { get; set; }

        [JsonPropertyName("related_entity_id")]
        public int? RelatedEntityId { get; set; }
    }

    public class HeatmapData
    {
        [JsonPropertyName("date")]
        public DateTime Date { get; set; }

        [JsonPropertyName("hour")]
        public int Hour { get; set; }

        [JsonPropertyName("activity_count")]
        public int ActivityCount { get; set; }

        [JsonPropertyName("activity_type")]
        public string ActivityType { get; set; }
    }

    public static class AlertType
    {
        public const string OverdueItem = "overdue_item";
        public const string ExpiringCard = "expiring_card";
        public const string UnauthorizedAccess = "unauthorized_access";
        public const string MaintenanceRequired = "maintenance_required";
        public const string LostItem = "lost_item";
        public const string SystemAlert = "system_alert";

        public static readonly string[] AllTypes = new[]
        {
            OverdueItem,
            ExpiringCard,
            UnauthorizedAccess,
            MaintenanceRequired,
            LostItem,
            SystemAlert
        };
    }

    public static class AlertSeverity
    {
        public const string Low = "low";
        public const string Medium = "medium";
        public const string High = "high";
        public const string Critical = "critical";

        public static readonly string[] AllSeverities = new[]
        {
            Low,
            Medium,
            High,
            Critical
        };

        public static readonly Dictionary<string, string> Colors = new Dictionary<string, string>
        {
            { Low, "#4CAF50" },      // Green
            { Medium, "#FFC107" },    // Yellow
            { High, "#FF9800" },      // Orange
            { Critical, "#F44336" }   // Red
        };
    }

    public static class ActivityType
    {
        public const string KeyCheckout = "key_checkout";
        public const string KeyCheckin = "key_checkin";
        public const string CardCheckout = "card_checkout";
        public const string CardCheckin = "card_checkin";
        public const string EmployeeCreated = "employee_created";
        public const string EmployeeUpdated = "employee_updated";
        public const string DepartmentCreated = "department_created";
        public const string DepartmentUpdated = "department_updated";
        public const string UserLogin = "user_login";
        public const string UserLogout = "user_logout";
        public const string SystemEvent = "system_event";

        public static readonly string[] AllTypes = new[]
        {
            KeyCheckout,
            KeyCheckin,
            CardCheckout,
            CardCheckin,
            EmployeeCreated,
            EmployeeUpdated,
            DepartmentCreated,
            DepartmentUpdated,
            UserLogin,
            UserLogout,
            SystemEvent
        };
    }
}
