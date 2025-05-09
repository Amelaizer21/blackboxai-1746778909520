using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace RosewoodSecurity.Models
{
    public class ReportBase
    {
        [JsonPropertyName("generated_at")]
        public DateTime GeneratedAt { get; set; }

        [JsonPropertyName("generated_by")]
        public string GeneratedBy { get; set; }

        [JsonPropertyName("report_type")]
        public string ReportType { get; set; }

        [JsonPropertyName("date_range")]
        public DateRange DateRange { get; set; }
    }

    public class DateRange
    {
        [JsonPropertyName("start_date")]
        public DateTime StartDate { get; set; }

        [JsonPropertyName("end_date")]
        public DateTime EndDate { get; set; }
    }

    public class DailyReport : ReportBase
    {
        [JsonPropertyName("transactions")]
        public List<Transaction> Transactions { get; set; }

        [JsonPropertyName("summary")]
        public DailyReportSummary Summary { get; set; }
    }

    public class DailyReportSummary
    {
        [JsonPropertyName("total_transactions")]
        public int TotalTransactions { get; set; }

        [JsonPropertyName("checkouts")]
        public int Checkouts { get; set; }

        [JsonPropertyName("checkins")]
        public int Checkins { get; set; }

        [JsonPropertyName("overdue_items")]
        public int OverdueItems { get; set; }

        [JsonPropertyName("by_department")]
        public Dictionary<string, int> TransactionsByDepartment { get; set; }

        [JsonPropertyName("by_item_type")]
        public Dictionary<string, int> TransactionsByItemType { get; set; }
    }

    public class WeeklyReport : ReportBase
    {
        [JsonPropertyName("daily_summaries")]
        public List<DailyReportSummary> DailySummaries { get; set; }

        [JsonPropertyName("department_summaries")]
        public List<DepartmentSummary> DepartmentSummaries { get; set; }

        [JsonPropertyName("overdue_items")]
        public List<Transaction> OverdueItems { get; set; }

        [JsonPropertyName("weekly_trends")]
        public WeeklyTrends Trends { get; set; }
    }

    public class WeeklyTrends
    {
        [JsonPropertyName("busiest_day")]
        public string BusiestDay { get; set; }

        [JsonPropertyName("peak_hours")]
        public List<TimeRange> PeakHours { get; set; }

        [JsonPropertyName("most_active_department")]
        public string MostActiveDepartment { get; set; }

        [JsonPropertyName("most_used_items")]
        public List<ItemUsage> MostUsedItems { get; set; }
    }

    public class TimeRange
    {
        [JsonPropertyName("start_hour")]
        public int StartHour { get; set; }

        [JsonPropertyName("end_hour")]
        public int EndHour { get; set; }

        [JsonPropertyName("transaction_count")]
        public int TransactionCount { get; set; }
    }

    public class ItemUsage
    {
        [JsonPropertyName("item_type")]
        public string ItemType { get; set; }

        [JsonPropertyName("item_id")]
        public string ItemId { get; set; }

        [JsonPropertyName("usage_count")]
        public int UsageCount { get; set; }
    }

    public class EmployeeReport : ReportBase
    {
        [JsonPropertyName("employee")]
        public Employee Employee { get; set; }

        [JsonPropertyName("statistics")]
        public EmployeeStatistics Statistics { get; set; }

        [JsonPropertyName("recent_transactions")]
        public List<Transaction> RecentTransactions { get; set; }

        [JsonPropertyName("department_permissions")]
        public List<Key> DepartmentPermissions { get; set; }
    }

    public class EmployeeStatistics
    {
        [JsonPropertyName("total_transactions")]
        public int TotalTransactions { get; set; }

        [JsonPropertyName("active_checkouts")]
        public int ActiveCheckouts { get; set; }

        [JsonPropertyName("overdue_items")]
        public int OverdueItems { get; set; }

        [JsonPropertyName("avg_checkout_duration")]
        public double AverageCheckoutDuration { get; set; }
    }

    public class LostItemsReport : ReportBase
    {
        [JsonPropertyName("lost_keys")]
        public List<LostItem<Key>> LostKeys { get; set; }

        [JsonPropertyName("lost_cards")]
        public List<LostItem<AccessCard>> LostCards { get; set; }

        [JsonPropertyName("total_lost_value")]
        public decimal TotalLostValue { get; set; }

        [JsonPropertyName("by_department")]
        public Dictionary<string, int> LostItemsByDepartment { get; set; }
    }

    public class LostItem<T>
    {
        [JsonPropertyName("item")]
        public T Item { get; set; }

        [JsonPropertyName("last_transaction")]
        public Transaction LastTransaction { get; set; }

        [JsonPropertyName("days_since_lost")]
        public int DaysSinceLost { get; set; }

        [JsonPropertyName("replacement_cost")]
        public decimal? ReplacementCost { get; set; }
    }

    public class AuditLogEntry
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("timestamp")]
        public DateTime Timestamp { get; set; }

        [JsonPropertyName("user")]
        public string User { get; set; }

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

        [JsonPropertyName("user_agent")]
        public string UserAgent { get; set; }
    }

    public static class ReportType
    {
        public const string Daily = "daily";
        public const string Weekly = "weekly";
        public const string Monthly = "monthly";
        public const string Employee = "employee";
        public const string Department = "department";
        public const string LostItems = "lost_items";
        public const string Audit = "audit";
        public const string Custom = "custom";

        public static readonly string[] AllTypes = new[]
        {
            Daily,
            Weekly,
            Monthly,
            Employee,
            Department,
            LostItems,
            Audit,
            Custom
        };
    }
}
