using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace RosewoodSecurity.Models
{
    public class ApiResponse<T>
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("data")]
        public T Data { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; }

        [JsonPropertyName("errors")]
        public List<string> Errors { get; set; }

        [JsonPropertyName("timestamp")]
        public DateTime Timestamp { get; set; }

        [JsonPropertyName("request_id")]
        public string RequestId { get; set; }
    }

    public class PaginatedResponse<T>
    {
        [JsonPropertyName("items")]
        public List<T> Items { get; set; }

        [JsonPropertyName("total_items")]
        public int TotalItems { get; set; }

        [JsonPropertyName("page")]
        public int Page { get; set; }

        [JsonPropertyName("page_size")]
        public int PageSize { get; set; }

        [JsonPropertyName("total_pages")]
        public int TotalPages { get; set; }

        [JsonPropertyName("has_next")]
        public bool HasNext { get; set; }

        [JsonPropertyName("has_previous")]
        public bool HasPrevious { get; set; }
    }

    public class ValidationError
    {
        [JsonPropertyName("field")]
        public string Field { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; }

        [JsonPropertyName("code")]
        public string Code { get; set; }
    }

    public class ApiError
    {
        [JsonPropertyName("code")]
        public string Code { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; }

        [JsonPropertyName("details")]
        public string Details { get; set; }

        [JsonPropertyName("validation_errors")]
        public List<ValidationError> ValidationErrors { get; set; }

        [JsonPropertyName("stack_trace")]
        public string StackTrace { get; set; }

        [JsonPropertyName("inner_error")]
        public ApiError InnerError { get; set; }
    }

    public class BatchOperationResponse
    {
        [JsonPropertyName("total_operations")]
        public int TotalOperations { get; set; }

        [JsonPropertyName("successful_operations")]
        public int SuccessfulOperations { get; set; }

        [JsonPropertyName("failed_operations")]
        public int FailedOperations { get; set; }

        [JsonPropertyName("operation_results")]
        public List<OperationResult> OperationResults { get; set; }
    }

    public class OperationResult
    {
        [JsonPropertyName("operation_id")]
        public string OperationId { get; set; }

        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; }

        [JsonPropertyName("error")]
        public ApiError Error { get; set; }
    }

    public static class ApiErrorCodes
    {
        public const string ValidationFailed = "VALIDATION_FAILED";
        public const string NotFound = "NOT_FOUND";
        public const string Unauthorized = "UNAUTHORIZED";
        public const string Forbidden = "FORBIDDEN";
        public const string InternalError = "INTERNAL_ERROR";
        public const string ServiceUnavailable = "SERVICE_UNAVAILABLE";
        public const string DuplicateEntry = "DUPLICATE_ENTRY";
        public const string InvalidOperation = "INVALID_OPERATION";
        public const string RateLimitExceeded = "RATE_LIMIT_EXCEEDED";
        public const string TokenExpired = "TOKEN_EXPIRED";
        public const string InvalidCredentials = "INVALID_CREDENTIALS";
        public const string TwoFactorRequired = "TWO_FACTOR_REQUIRED";
        public const string InvalidTwoFactorCode = "INVALID_TWO_FACTOR_CODE";
        public const string AccountLocked = "ACCOUNT_LOCKED";
        public const string ItemNotAvailable = "ITEM_NOT_AVAILABLE";
        public const string UnauthorizedDepartment = "UNAUTHORIZED_DEPARTMENT";
        public const string ExpiredAccessCard = "EXPIRED_ACCESS_CARD";
        public const string OverdueItems = "OVERDUE_ITEMS";
        public const string MaintenanceRequired = "MAINTENANCE_REQUIRED";
    }

    public class ApiRequestOptions
    {
        public int? Page { get; set; }
        public int? PageSize { get; set; }
        public string SortBy { get; set; }
        public bool? SortDescending { get; set; }
        public string SearchQuery { get; set; }
        public Dictionary<string, string> Filters { get; set; }
        public List<string> Include { get; set; }
        public bool? IncludeDeleted { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public ApiRequestOptions()
        {
            Filters = new Dictionary<string, string>();
            Include = new List<string>();
        }

        public string ToQueryString()
        {
            var queryParams = new List<string>();

            if (Page.HasValue)
                queryParams.Add($"page={Page.Value}");
            if (PageSize.HasValue)
                queryParams.Add($"page_size={PageSize.Value}");
            if (!string.IsNullOrEmpty(SortBy))
                queryParams.Add($"sort_by={SortBy}");
            if (SortDescending.HasValue)
                queryParams.Add($"sort_desc={SortDescending.Value}");
            if (!string.IsNullOrEmpty(SearchQuery))
                queryParams.Add($"search={Uri.EscapeDataString(SearchQuery)}");
            if (Include.Count > 0)
                queryParams.Add($"include={string.Join(",", Include)}");
            if (IncludeDeleted.HasValue)
                queryParams.Add($"include_deleted={IncludeDeleted.Value}");
            if (StartDate.HasValue)
                queryParams.Add($"start_date={StartDate.Value:yyyy-MM-dd}");
            if (EndDate.HasValue)
                queryParams.Add($"end_date={EndDate.Value:yyyy-MM-dd}");

            foreach (var filter in Filters)
            {
                queryParams.Add($"{filter.Key}={Uri.EscapeDataString(filter.Value)}");
            }

            return queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : string.Empty;
        }
    }
}
