using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RosewoodSecurity.Models;

namespace RosewoodSecurity.Services
{
    public interface IApiService
    {
        // Authentication
        Task<AuthenticationResult> LoginAsync(string username, string password);
        Task<bool> ValidateTwoFactorAsync(string code);
        Task LogoutAsync();

        // Dashboard
        Task<DashboardData> GetDashboardDataAsync();

        // Employee Management
        Task<EmployeeInfo> GetEmployeeByIdAsync(string employeeId);
        Task<List<EmployeeInfo>> GetAllEmployeesAsync();
        Task<EmployeeInfo> CreateEmployeeAsync(EmployeeInfo employee);
        Task<EmployeeInfo> UpdateEmployeeAsync(string id, EmployeeInfo employee);
        Task DeleteEmployeeAsync(string id);

        // Key Management
        Task<List<InventoryItem>> GetAvailableItemsAsync(string itemType);
        Task<InventoryItem> GetItemByIdAsync(string id);
        Task<InventoryItem> CreateItemAsync(InventoryItem item);
        Task<InventoryItem> UpdateItemAsync(string id, InventoryItem item);
        Task DeleteItemAsync(string id);

        // Transaction Management
        Task<List<ActiveTransaction>> GetActiveTransactionsAsync();
        Task<TransactionResult> CheckOutItemAsync(CheckOutRequest request);
        Task<TransactionResult> CheckInItemAsync(string transactionId);
        Task<List<TransactionHistory>> GetTransactionHistoryAsync(DateTime? startDate = null, DateTime? endDate = null);

        // Reports
        Task<byte[]> GenerateReportAsync(ReportRequest request);
        Task<List<ReportTemplate>> GetAvailableReportTemplatesAsync();

        // Scanner Integration
        Task<bool> CheckScannerStatusAsync();
        Task<string> GetLastScannedValueAsync();

        // System
        Task<HealthCheckResult> CheckSystemHealthAsync();
        Task<List<SystemLog>> GetSystemLogsAsync(DateTime startDate, DateTime endDate);
        Task<bool> BackupDataAsync();
    }

    public class AuthenticationResult
    {
        public bool Success { get; set; }
        public bool RequiresTwoFactor { get; set; }
        public string Token { get; set; }
        public string RefreshToken { get; set; }
        public UserInfo User { get; set; }
        public string ErrorMessage { get; set; }
    }

    public class DashboardData
    {
        public int TotalKeysOut { get; set; }
        public int TotalAccessCardsOut { get; set; }
        public int OverdueItemsCount { get; set; }
        public int ActiveUsersCount { get; set; }
        public List<DepartmentUsage> DepartmentUsage { get; set; }
        public List<RecentActivity> RecentActivities { get; set; }
    }

    public class DepartmentUsage
    {
        public string DepartmentName { get; set; }
        public double UsagePercentage { get; set; }
    }

    public class RecentActivity
    {
        public string Description { get; set; }
        public DateTime Timestamp { get; set; }
        public string Type { get; set; }
    }

    public class TransactionResult
    {
        public bool Success { get; set; }
        public string TransactionId { get; set; }
        public string Message { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public class TransactionHistory
    {
        public string TransactionId { get; set; }
        public string EmployeeName { get; set; }
        public string ItemId { get; set; }
        public string ItemType { get; set; }
        public DateTime CheckOutTime { get; set; }
        public DateTime? CheckInTime { get; set; }
        public string Status { get; set; }
    }

    public class ReportRequest
    {
        public string TemplateId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public Dictionary<string, string> Parameters { get; set; }
    }

    public class ReportTemplate
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public List<ReportParameter> Parameters { get; set; }
    }

    public class ReportParameter
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public bool Required { get; set; }
        public string DefaultValue { get; set; }
    }

    public class HealthCheckResult
    {
        public bool IsHealthy { get; set; }
        public Dictionary<string, string> Components { get; set; }
        public string Message { get; set; }
    }

    public class SystemLog
    {
        public DateTime Timestamp { get; set; }
        public string Level { get; set; }
        public string Message { get; set; }
        public string Component { get; set; }
        public string Details { get; set; }
    }
}
