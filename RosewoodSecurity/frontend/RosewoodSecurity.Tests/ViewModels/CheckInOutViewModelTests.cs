using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using RosewoodSecurity.Tests.TestHelpers;
using RosewoodSecurity.ViewModels;
using Xunit;

namespace RosewoodSecurity.Tests.ViewModels
{
    public class CheckInOutViewModelTests : TestBase
    {
        private readonly CheckInOutViewModel _viewModel;

        public CheckInOutViewModelTests()
        {
            _viewModel = new CheckInOutViewModel(
                MockApiService.Object,
                MockAuthenticationService.Object,
                MockNavigationService.Object,
                MockDialogService.Object,
                MockSettingsService.Object);

            // Setup default authenticated user
            MockAuthenticationService
                .Setup(x => x.IsAuthenticated)
                .Returns(true);

            MockAuthenticationService
                .Setup(x => x.CurrentUser)
                .Returns(TestData.CreateTestUser());
        }

        [Fact]
        public void Constructor_InitializesProperties()
        {
            // Assert
            Assert.NotNull(_viewModel.CheckOutCommand);
            Assert.NotNull(_viewModel.CheckInCommand);
            Assert.NotNull(_viewModel.ScanCommand);
            Assert.NotNull(_viewModel.RefreshCommand);
            Assert.False(_viewModel.IsBusy);
            Assert.NotNull(_viewModel.ActiveTransactions);
            Assert.NotNull(_viewModel.AvailableItems);
        }

        [Fact]
        public async Task LoadData_PopulatesActiveTransactions()
        {
            // Arrange
            var transactions = new List<ActiveTransaction>
            {
                new ActiveTransaction
                {
                    Id = "1",
                    ItemId = "KEY001",
                    ItemType = "Key",
                    EmployeeId = "EMP001",
                    CheckOutTime = DateTime.UtcNow
                }
            };

            MockApiService
                .Setup(x => x.GetActiveTransactionsAsync())
                .ReturnsAsync(transactions);

            // Act
            await _viewModel.LoadDataAsync();

            // Assert
            Assert.Single(_viewModel.ActiveTransactions);
            Assert.Equal(transactions[0].Id, _viewModel.ActiveTransactions[0].Id);
        }

        [Fact]
        public async Task LoadData_PopulatesAvailableItems()
        {
            // Arrange
            var items = new List<InventoryItem>
            {
                new InventoryItem
                {
                    Id = "KEY001",
                    Type = "Key",
                    Description = "Main Office Key",
                    IsAvailable = true
                }
            };

            MockApiService
                .Setup(x => x.GetAvailableItemsAsync(It.IsAny<string>()))
                .ReturnsAsync(items);

            // Act
            await _viewModel.LoadDataAsync();

            // Assert
            Assert.Single(_viewModel.AvailableItems);
            Assert.Equal(items[0].Id, _viewModel.AvailableItems[0].Id);
        }

        [Fact]
        public async Task CheckOutCommand_WithValidData_CompletesSuccessfully()
        {
            // Arrange
            var item = new InventoryItem
            {
                Id = "KEY001",
                Type = "Key",
                Description = "Main Office Key"
            };
            _viewModel.SelectedItem = item;
            _viewModel.EmployeeId = "EMP001";

            MockApiService
                .Setup(x => x.CheckOutItemAsync(It.IsAny<CheckOutRequest>()))
                .ReturnsAsync(new TransactionResult { Success = true });

            // Act
            await _viewModel.CheckOutCommand.ExecuteAsync(null);

            // Assert
            MockApiService.Verify(x => x.CheckOutItemAsync(It.Is<CheckOutRequest>(r =>
                r.ItemId == item.Id &&
                r.EmployeeId == "EMP001")), Times.Once);
            MockDialogService.Verify(x => x.ShowSuccessAsync(
                It.IsAny<string>(),
                It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task CheckInCommand_WithValidTransaction_CompletesSuccessfully()
        {
            // Arrange
            var transaction = new ActiveTransaction
            {
                Id = "TRANS001",
                ItemId = "KEY001",
                ItemType = "Key",
                EmployeeId = "EMP001"
            };
            _viewModel.SelectedTransaction = transaction;

            MockApiService
                .Setup(x => x.CheckInItemAsync(transaction.Id))
                .ReturnsAsync(new TransactionResult { Success = true });

            // Act
            await _viewModel.CheckInCommand.ExecuteAsync(null);

            // Assert
            MockApiService.Verify(x => x.CheckInItemAsync(transaction.Id), Times.Once);
            MockDialogService.Verify(x => x.ShowSuccessAsync(
                It.IsAny<string>(),
                It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task ScanCommand_WithValidScan_PopulatesFields()
        {
            // Arrange
            var scanResult = "EMP001|KEY001";
            MockApiService
                .Setup(x => x.CheckScannerStatusAsync())
                .ReturnsAsync(true);

            // Act
            await _viewModel.ScanCommand.ExecuteAsync(scanResult);

            // Assert
            Assert.Equal("EMP001", _viewModel.EmployeeId);
            Assert.Equal("KEY001", _viewModel.ScannedItemId);
        }

        [Fact]
        public void CanExecuteCheckOutCommand_RequiresValidData()
        {
            // Arrange - Invalid state
            _viewModel.SelectedItem = null;
            _viewModel.EmployeeId = string.Empty;

            // Act & Assert
            Assert.False(_viewModel.CheckOutCommand.CanExecute(null));

            // Arrange - Valid state
            _viewModel.SelectedItem = new InventoryItem { Id = "KEY001" };
            _viewModel.EmployeeId = "EMP001";

            // Act & Assert
            Assert.True(_viewModel.CheckOutCommand.CanExecute(null));
        }

        [Fact]
        public void CanExecuteCheckInCommand_RequiresSelectedTransaction()
        {
            // Arrange - Invalid state
            _viewModel.SelectedTransaction = null;

            // Act & Assert
            Assert.False(_viewModel.CheckInCommand.CanExecute(null));

            // Arrange - Valid state
            _viewModel.SelectedTransaction = new ActiveTransaction { Id = "TRANS001" };

            // Act & Assert
            Assert.True(_viewModel.CheckInCommand.CanExecute(null));
        }

        [Fact]
        public async Task RefreshCommand_UpdatesAllData()
        {
            // Arrange
            var transactions = new List<ActiveTransaction> { new ActiveTransaction { Id = "TRANS001" } };
            var items = new List<InventoryItem> { new InventoryItem { Id = "KEY001" } };

            MockApiService
                .Setup(x => x.GetActiveTransactionsAsync())
                .ReturnsAsync(transactions);

            MockApiService
                .Setup(x => x.GetAvailableItemsAsync(It.IsAny<string>()))
                .ReturnsAsync(items);

            // Act
            await _viewModel.RefreshCommand.ExecuteAsync(null);

            // Assert
            Assert.Single(_viewModel.ActiveTransactions);
            Assert.Single(_viewModel.AvailableItems);
            Assert.False(_viewModel.IsBusy);
        }

        [Fact]
        public async Task LoadData_OnError_ShowsErrorDialog()
        {
            // Arrange
            MockApiService
                .Setup(x => x.GetActiveTransactionsAsync())
                .ThrowsAsync(new Exception("Test error"));

            // Act
            await _viewModel.LoadDataAsync();

            // Assert
            MockDialogService.Verify(x => x.ShowErrorAsync(
                It.IsAny<string>(),
                It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public void FilterItems_FiltersCorrectly()
        {
            // Arrange
            _viewModel.AvailableItems.Add(new InventoryItem { Id = "KEY001", Type = "Key" });
            _viewModel.AvailableItems.Add(new InventoryItem { Id = "CARD001", Type = "AccessCard" });

            // Act
            _viewModel.ItemTypeFilter = "Key";

            // Assert
            Assert.Single(_viewModel.FilteredItems);
            Assert.Equal("KEY001", _viewModel.FilteredItems[0].Id);
        }
    }
}
