using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace RosewoodSecurity.Models
{
    public class EventBase
    {
        [JsonPropertyName("event_id")]
        public string EventId { get; set; }

        [JsonPropertyName("event_type")]
        public string EventType { get; set; }

        [JsonPropertyName("timestamp")]
        public DateTime Timestamp { get; set; }

        [JsonPropertyName("user_id")]
        public string UserId { get; set; }

        [JsonPropertyName("source")]
        public string Source { get; set; }

        [JsonPropertyName("severity")]
        public string Severity { get; set; }
    }

    public class SecurityEvent : EventBase
    {
        [JsonPropertyName("location")]
        public string Location { get; set; }

        [JsonPropertyName("action")]
        public string Action { get; set; }

        [JsonPropertyName("outcome")]
        public string Outcome { get; set; }

        [JsonPropertyName("target_type")]
        public string TargetType { get; set; }

        [JsonPropertyName("target_id")]
        public string TargetId { get; set; }

        [JsonPropertyName("details")]
        public Dictionary<string, object> Details { get; set; }
    }

    public class SystemEvent : EventBase
    {
        [JsonPropertyName("component")]
        public string Component { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; }

        [JsonPropertyName("error_code")]
        public string ErrorCode { get; set; }

        [JsonPropertyName("stack_trace")]
        public string StackTrace { get; set; }
    }

    public class AccessEvent : EventBase
    {
        [JsonPropertyName("employee_id")]
        public int EmployeeId { get; set; }

        [JsonPropertyName("access_point")]
        public string AccessPoint { get; set; }

        [JsonPropertyName("access_type")]
        public string AccessType { get; set; }

        [JsonPropertyName("granted")]
        public bool Granted { get; set; }

        [JsonPropertyName("reason")]
        public string Reason { get; set; }
    }

    public class AuditEvent : EventBase
    {
        [JsonPropertyName("action")]
        public string Action { get; set; }

        [JsonPropertyName("entity_type")]
        public string EntityType { get; set; }

        [JsonPropertyName("entity_id")]
        public string EntityId { get; set; }

        [JsonPropertyName("changes")]
        public Dictionary<string, object> Changes { get; set; }

        [JsonPropertyName("ip_address")]
        public string IpAddress { get; set; }
    }

    public class NotificationEvent : EventBase
    {
        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; }

        [JsonPropertyName("notification_type")]
        public string NotificationType { get; set; }

        [JsonPropertyName("recipient_ids")]
        public List<string> RecipientIds { get; set; }

        [JsonPropertyName("action_url")]
        public string ActionUrl { get; set; }
    }

    public static class EventType
    {
        public const string SecurityAlert = "security_alert";
        public const string SystemAlert = "system_alert";
        public const string AccessAttempt = "access_attempt";
        public const string UserActivity = "user_activity";
        public const string ItemCheckout = "item_checkout";
        public const string ItemCheckin = "item_checkin";
        public const string ItemLost = "item_lost";
        public const string MaintenanceRequired = "maintenance_required";
        public const string ConfigurationChange = "configuration_change";
        public const string DatabaseBackup = "database_backup";
        public const string SystemStartup = "system_startup";
        public const string SystemShutdown = "system_shutdown";

        public static readonly string[] AllTypes = new[]
        {
            SecurityAlert,
            SystemAlert,
            AccessAttempt,
            UserActivity,
            ItemCheckout,
            ItemCheckin,
            ItemLost,
            MaintenanceRequired,
            ConfigurationChange,
            DatabaseBackup,
            SystemStartup,
            SystemShutdown
        };
    }

    public static class EventSeverity
    {
        public const string Debug = "debug";
        public const string Info = "info";
        public const string Warning = "warning";
        public const string Error = "error";
        public const string Critical = "critical";

        public static readonly Dictionary<string, int> Priority = new Dictionary<string, int>
        {
            { Debug, 0 },
            { Info, 1 },
            { Warning, 2 },
            { Error, 3 },
            { Critical, 4 }
        };
    }

    public static class AccessType
    {
        public const string Entry = "entry";
        public const string Exit = "exit";
        public const string Denied = "denied";
        public const string Override = "override";
        public const string Emergency = "emergency";

        public static readonly string[] AllTypes = new[]
        {
            Entry,
            Exit,
            Denied,
            Override,
            Emergency
        };
    }

    public static class NotificationType
    {
        public const string Alert = "alert";
        public const string Warning = "warning";
        public const string Info = "info";
        public const string Success = "success";
        public const string Error = "error";

        public static readonly Dictionary<string, string> Icons = new Dictionary<string, string>
        {
            { Alert, "exclamation-triangle" },
            { Warning, "exclamation-circle" },
            { Info, "info-circle" },
            { Success, "check-circle" },
            { Error, "times-circle" }
        };
    }
}
