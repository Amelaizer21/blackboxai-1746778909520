using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using RosewoodSecurity.Models;
using RosewoodSecurity.Services;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Linq;
using System.Windows.Threading;

namespace RosewoodSecurity.ViewModels
{
    public class CheckInOutViewModel : BaseViewModel
    {
        private readonly IApiService _apiService;
        private readonly IDialogService _dialogService;
        private readonly DispatcherTimer _scannerCheckTimer;
        
        private string _employeeId;
        private EmployeeInfo _employeeInfo;
        private string _selectedItemType;
        private InventoryItem _selectedItem;
        private string _purpose;
        private DateTime? _expectedReturnDate;
        private string _errorMessage;
        private bool _isScannerConnected;
        private bool _isCheckOut = true;

        public CheckInOutViewModel(IApiService apiService, IDialogService dialogService)
        {
            _apiService = apiService;
            _dialogService = dialogService;

            // Initialize commands
            ProcessTransactionCommand = new RelayCommand(async () => await ProcessTransactionAsync(), CanProcessTransaction);
            CheckInCommand = new RelayCommand<ActiveTransaction>(async (t) => await CheckInItemAsync(t));

            // Initialize collections
            ActiveTransactions = new ObservableCollection<ActiveTransaction>();
            ItemTypes = new ObservableCollection<string> { "Key", "Access Card" };
            AvailableItems = new ObservableCollection<InventoryItem>();

            // Set up scanner check timer
            _scannerCheckTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(5)
            };
            _scannerCheckTimer.Tick += async (s, e) => await CheckScannerStatusAsync();
            _scannerCheckTimer.Start();

            // Initial load
            _ = LoadActiveTransactionsAsync();
            _ = CheckScannerStatusAsync();
        }

        public string EmployeeId
        {
            get => _employeeId;
            set
            {
                if (SetProperty(ref _employeeId, value))
                {
                    _ = LoadEmployeeInfoAsync();
                    (ProcessTransactionCommand as RelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        public EmployeeInfo EmployeeInfo
        {
            get => _employeeInfo;
            set => SetProperty(ref _employeeInfo, value);
        }

        public string SelectedItemType
        {
            get => _selectedItemType;
            set
            {
                if (SetProperty(ref _selectedItemType, value))
                {
                    _ = LoadAvailableItemsAsync();
                    (ProcessTransactionCommand as RelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        public InventoryItem SelectedItem
        {
            get => _selectedItem;
            set
            {
                if (SetProperty(ref _selectedItem, value))
                {
                    (ProcessTransactionCommand as RelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        public string Purpose
        {
            get => _purpose;
            set
            {
                if (SetProperty(ref _purpose, value))
                {
                    (ProcessTransactionCommand as RelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        public DateTime? ExpectedReturnDate
        {
            get => _expectedReturnDate;
            set
            {
                if (SetProperty(ref _expectedReturnDate, value))
                {
                    (ProcessTransactionCommand as RelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        public bool IsScannerConnected
        {
            get => _isScannerConnected;
            set => SetProperty(ref _isScannerConnected, value);
        }

        public bool IsCheckOut
        {
            get => _isCheckOut;
            set => SetProperty(ref _isCheckOut, value);
        }

        public ObservableCollection<string> ItemTypes { get; }
        public ObservableCollection<InventoryItem> AvailableItems { get; }
        public ObservableCollection<ActiveTransaction> ActiveTransactions { get; }

        public ICommand ProcessTransactionCommand { get; }
        public ICommand CheckInCommand { get; }

        private async Task LoadEmployeeInfoAsync()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(EmployeeId))
                {
                    EmployeeInfo = null;
                    return;
                }

                ErrorMessage = null;
                EmployeeInfo = await _apiService.GetEmployeeByIdAsync(EmployeeId);
            }
            catch (Exception ex)
            {
                ErrorMessage = "Failed to load employee information.";
                EmployeeInfo = null;
                Console.WriteLine($"Error loading employee info: {ex}");
            }
        }

        private async Task LoadAvailableItemsAsync()
        {
            try
            {
                AvailableItems.Clear();
                if (string.IsNullOrWhiteSpace(SelectedItemType)) return;

                var items = await _apiService.GetAvailableItemsAsync(SelectedItemType.ToLower());
                foreach (var item in items)
                {
                    AvailableItems.Add(item);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = "Failed to load available items.";
                Console.WriteLine($"Error loading available items: {ex}");
            }
        }

        private async Task LoadActiveTransactionsAsync()
        {
            try
            {
                ActiveTransactions.Clear();
                var transactions = await _apiService.GetActiveTransactionsAsync();
                foreach (var transaction in transactions)
                {
                    ActiveTransactions.Add(transaction);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = "Failed to load active transactions.";
                Console.WriteLine($"Error loading active transactions: {ex}");
            }
        }

        private async Task ProcessTransactionAsync()
        {
            try
            {
                ErrorMessage = null;

                if (IsCheckOut)
                {
                    var request = new CheckOutRequest
                    {
                        EmployeeId = EmployeeId,
                        ItemId = SelectedItem.Id,
                        ItemType = SelectedItemType,
                        Purpose = Purpose,
                        ExpectedReturnDate = ExpectedReturnDate.Value
                    };

                    await _apiService.CheckOutItemAsync(request);
                    await _dialogService.ShowSuccessAsync("Success", "Item checked out successfully.");
                }

                // Reset form
                EmployeeId = null;
                SelectedItem = null;
                Purpose = null;
                ExpectedReturnDate = null;

                // Refresh active transactions
                await LoadActiveTransactionsAsync();
            }
            catch (Exception ex)
            {
                ErrorMessage = "Failed to process transaction.";
                Console.WriteLine($"Transaction error: {ex}");
            }
        }

        private async Task CheckInItemAsync(ActiveTransaction transaction)
        {
            try
            {
                ErrorMessage = null;
                await _apiService.CheckInItemAsync(transaction.TransactionId);
                await _dialogService.ShowSuccessAsync("Success", "Item checked in successfully.");
                await LoadActiveTransactionsAsync();
            }
            catch (Exception ex)
            {
                ErrorMessage = "Failed to check in item.";
                Console.WriteLine($"Check-in error: {ex}");
            }
        }

        private async Task CheckScannerStatusAsync()
        {
            try
            {
                IsScannerConnected = await _apiService.CheckScannerStatusAsync();
            }
            catch
            {
                IsScannerConnected = false;
            }
        }

        private bool CanProcessTransaction()
        {
            if (IsCheckOut)
            {
                return !string.IsNullOrWhiteSpace(EmployeeId) &&
                       EmployeeInfo != null &&
                       SelectedItem != null &&
                       !string.IsNullOrWhiteSpace(Purpose) &&
                       ExpectedReturnDate.HasValue &&
                       ExpectedReturnDate.Value > DateTime.Now;
            }
            
            return !string.IsNullOrWhiteSpace(EmployeeId) &&
                   EmployeeInfo != null &&
                   SelectedItem != null;
        }

        public override void Dispose()
        {
            _scannerCheckTimer.Stop();
            base.Dispose();
        }
    }

    public class EmployeeInfo
    {
        public string Id { get; set; }
        public string FullName { get; set; }
        public string Department { get; set; }
        public string PhotoUrl { get; set; }
    }

    public class InventoryItem
    {
        public string Id { get; set; }
        public string Location { get; set; }
        public string Status { get; set; }
    }

    public class ActiveTransaction
    {
        public string TransactionId { get; set; }
        public string EmployeeName { get; set; }
        public string ItemId { get; set; }
        public string ItemType { get; set; }
        public DateTime CheckOutTime { get; set; }
        public DateTime ExpectedReturnTime { get; set; }
    }

    public class CheckOutRequest
    {
        public string EmployeeId { get; set; }
        public string ItemId { get; set; }
        public string ItemType { get; set; }
        public string Purpose { get; set; }
        public DateTime ExpectedReturnDate { get; set; }
    }
}
