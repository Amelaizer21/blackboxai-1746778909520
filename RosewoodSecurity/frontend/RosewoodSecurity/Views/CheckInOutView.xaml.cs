using System.Windows.Controls;
using System.Windows.Input;
using RosewoodSecurity.ViewModels;

namespace RosewoodSecurity.Views
{
    public partial class CheckInOutView : UserControl
    {
        private CheckInOutViewModel _viewModel;
        private bool _isProcessingBarcode;

        public CheckInOutView()
        {
            InitializeComponent();

            // ViewModel will be set via dependency injection
            Loaded += (s, e) =>
            {
                if (DataContext is CheckInOutViewModel vm)
                {
                    _viewModel = vm;
                }
            };

            // Focus the employee ID input when the view loads
            Loaded += (s, e) => EmployeeIdInput.Focus();

            // Handle barcode scanner input
            // Most barcode scanners act as keyboard input devices and append a return character
            EmployeeIdInput.PreviewKeyDown += (s, e) =>
            {
                if (e.Key == Key.Return)
                {
                    if (_isProcessingBarcode)
                    {
                        // Prevent double processing of the same scan
                        e.Handled = true;
                        return;
                    }

                    _isProcessingBarcode = true;

                    // Process the scanned input
                    var textBox = (TextBox)s;
                    string scannedValue = textBox.Text.Trim();

                    if (!string.IsNullOrEmpty(scannedValue))
                    {
                        // The ViewModel will handle loading the employee info
                        _viewModel.EmployeeId = scannedValue;
                    }

                    e.Handled = true;

                    // Reset the processing flag after a short delay
                    var timer = new System.Windows.Threading.DispatcherTimer
                    {
                        Interval = System.TimeSpan.FromMilliseconds(500)
                    };
                    timer.Tick += (sender, args) =>
                    {
                        _isProcessingBarcode = false;
                        timer.Stop();
                    };
                    timer.Start();
                }
            };

            // Handle manual input focus
            EmployeeIdInput.GotFocus += (s, e) =>
            {
                // Select all text when the input gets focus
                EmployeeIdInput.SelectAll();
            };
        }
    }
}
