using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using RosewoodSecurity.Models;
using RosewoodSecurity.Services;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Threading;

namespace RosewoodSecurity.ViewModels
{
    public class DashboardViewModel : BaseViewModel
    {
        private readonly IApiService _apiService;
        private readonly IDialogService _dialogService;
        private readonly DispatcherTimer _refreshTimer;
        
        private DateTime _currentDateTime;
        private int _totalKeysOut;
        private int _totalAccessCardsOut;
        private int _overdueItemsCount;
        private int _activeUsersCount;
        private bool _isLoading;

        public DashboardViewModel(IApiService apiService, IDialogService dialogService)
        {
            _apiService = apiService;
            _dialogService = dialogService;

            // Initialize commands
            RefreshCommand = new RelayCommand(async () => await RefreshDashboardAsync());

            // Initialize collections
            DepartmentUsage = new ObservableCollection<DepartmentUsageModel>();
            RecentActivities = new ObservableCollection<ActivityModel>();

            // Set up auto-refresh timer (every 30 seconds)
            _refreshTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(30)
            };
            _refreshTimer.Tick += async (s, e) => await RefreshDashboardAsync();
            _refreshTimer.Start();

            // Initial load
            _ = RefreshDashboardAsync();
        }

        public DateTime CurrentDateTime
        {
            get => _currentDateTime;
            set => SetProperty(ref _currentDateTime, value);
        }

        public int TotalKeysOut
        {
            get => _totalKeysOut;
            set => SetProperty(ref _totalKeysOut, value);
        }

        public int TotalAccessCardsOut
        {
            get => _totalAccessCardsOut;
            set => SetProperty(ref _totalAccessCardsOut, value);
        }

        public int OverdueItemsCount
        {
            get => _overdueItemsCount;
            set => SetProperty(ref _overdueItemsCount, value);
        }

        public int ActiveUsersCount
        {
            get => _activeUsersCount;
            set => SetProperty(ref _activeUsersCount, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                if (SetProperty(ref _isLoading, value))
                {
                    (RefreshCommand as RelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        public ObservableCollection<DepartmentUsageModel> DepartmentUsage { get; }
        public ObservableCollection<ActivityModel> RecentActivities { get; }

        public ICommand RefreshCommand { get; }

        private async Task RefreshDashboardAsync()
        {
            try
            {
                IsLoading = true;
                CurrentDateTime = DateTime.Now;

                // Get dashboard data from API
                var dashboardData = await _apiService.GetDashboardDataAsync();

                // Update properties
                TotalKeysOut = dashboardData.TotalKeysOut;
                TotalAccessCardsOut = dashboardData.TotalAccessCardsOut;
                OverdueItemsCount = dashboardData.OverdueItemsCount;
                ActiveUsersCount = dashboardData.ActiveUsersCount;

                // Update department usage
                DepartmentUsage.Clear();
                foreach (var dept in dashboardData.DepartmentUsage)
                {
                    DepartmentUsage.Add(new DepartmentUsageModel
                    {
                        DepartmentName = dept.DepartmentName,
                        UsagePercentage = dept.UsagePercentage
                    });
                }

                // Update recent activities
                RecentActivities.Clear();
                foreach (var activity in dashboardData.RecentActivities)
                {
                    RecentActivities.Add(new ActivityModel
                    {
                        Description = activity.Description,
                        Timestamp = activity.Timestamp,
                        ActivityIcon = GetActivityIcon(activity.Type)
                    });
                }
            }
            catch (Exception ex)
            {
                await _dialogService.ShowErrorAsync("Dashboard Error", 
                    "Failed to refresh dashboard data. Please try again.");
                // Log the error
                Console.WriteLine($"Dashboard refresh error: {ex}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private string GetActivityIcon(string activityType)
        {
            return activityType?.ToLower() switch
            {
                "checkout" => "KeyArrowRight",
                "checkin" => "KeyArrowLeft",
                "alert" => "Alert",
                "user" => "Account",
                _ => "Information"
            };
        }

        public override void Dispose()
        {
            _refreshTimer.Stop();
            base.Dispose();
        }
    }

    public class DepartmentUsageModel
    {
        public string DepartmentName { get; set; }
        public double UsagePercentage { get; set; }
    }

    public class ActivityModel
    {
        public string Description { get; set; }
        public DateTime Timestamp { get; set; }
        public string ActivityIcon { get; set; }
    }
}
