using System.Collections.Generic;
using System.Threading.Tasks;
using RosewoodSecurity.Models;

namespace RosewoodSecurity.Services
{
    public interface IApiService
    {
        // Authentication
        Task<AuthResponse> LoginAsync(string username, string password, string twoFactorCode = null);
        Task<AuthResponse> RefreshTokenAsync(string refreshToken);
        Task<bool> LogoutAsync();
        Task<TwoFactorSetupResponse> Setup2FAAsync();
        Task<bool> Disable2FAAsync();
        Task<bool> ChangePasswordAsync(string currentPassword, string newPassword);

        // Dashboard
        Task<DashboardStats> GetDashboardStatsAsync();
        Task<HeatmapData> GetUsageHeatmapAsync();
        Task<List<Alert>> GetAlertsAsync();

        // Keys
        Task<List<Key>> GetKeysAsync(string status = null, string location = null, int? departmentId = null);
        Task<Key> GetKeyAsync(int keyId);
        Task<Key> CreateKeyAsync(KeyCreateRequest request);
        Task<Key> UpdateKeyAsync(int keyId, KeyUpdateRequest request);
        Task<bool> DeleteKeyAsync(int keyId);
        Task<List<Department>> GetKeyPermissionsAsync(int keyId);
        Task<List<Transaction>> GetKeyHistoryAsync(int keyId);
        Task<bool> RecordKeyMaintenanceAsync(int keyId, string notes = null);

        // Access Cards
        Task<List<AccessCard>> GetAccessCardsAsync(string status = null, string cardType = null, int? departmentId = null);
        Task<AccessCard> GetAccessCardAsync(int cardId);
        Task<AccessCard> CreateAccessCardAsync(AccessCardCreateRequest request);
        Task<AccessCard> UpdateAccessCardAsync(int cardId, AccessCardUpdateRequest request);
        Task<bool> DeleteAccessCardAsync(int cardId);
        Task<bool> ExtendCardExpiryAsync(int cardId, string newExpiryDate);
        Task<List<Transaction>> GetCardHistoryAsync(int cardId);
        Task<List<AccessCard>> GetExpiringCardsAsync(int days = 30);

        // Employees
        Task<List<Employee>> GetEmployeesAsync(int? departmentId = null, string status = null, string search = null);
        Task<Employee> GetEmployeeAsync(int employeeId);
        Task<Employee> CreateEmployeeAsync(EmployeeCreateRequest request);
        Task<Employee> UpdateEmployeeAsync(int employeeId, EmployeeUpdateRequest request);
        Task<List<Transaction>> GetEmployeeTransactionsAsync(int employeeId);
        Task<List<ActiveCheckout>> GetEmployeeActiveItemsAsync(int employeeId);

        // Departments
        Task<List<Department>> GetDepartmentsAsync();
        Task<Department> CreateDepartmentAsync(DepartmentCreateRequest request);
        Task<Department> UpdateDepartmentAsync(int departmentId, DepartmentUpdateRequest request);
        Task<List<Employee>> GetDepartmentEmployeesAsync(int departmentId);

        // Transactions
        Task<Transaction> CheckoutItemAsync(CheckoutRequest request);
        Task<Transaction> CheckinItemAsync(string transactionNumber, string notes = null);
        Task<List<Transaction>> GetActiveTransactionsAsync(int? employeeId = null, string itemType = null);
        Task<List<Transaction>> GetOverdueTransactionsAsync();
        Task<Transaction> ExtendCheckoutAsync(string transactionNumber, double additionalHours);
        Task<Transaction> ReportLostItemAsync(string transactionNumber, string notes = null);

        // Reports
        Task<byte[]> GenerateDailyReportAsync();
        Task<byte[]> GenerateWeeklyReportAsync();
        Task<DepartmentReport> GenerateDepartmentReportAsync(int departmentId);
        Task<EmployeeReport> GenerateEmployeeReportAsync(int employeeId);
        Task<LostItemsReport> GetLostItemsReportAsync();
        Task<List<AuditLogEntry>> GetAuditTrailAsync(string startDate = null, string endDate = null, string actionType = null);
    }
}
