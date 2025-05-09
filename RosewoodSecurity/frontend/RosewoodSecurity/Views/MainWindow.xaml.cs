using System.Windows;
using RosewoodSecurity.ViewModels;

namespace RosewoodSecurity.Views
{
    public partial class MainWindow : Window
    {
        private MainWindowViewModel _viewModel;

        public MainWindow()
        {
            InitializeComponent();

            // ViewModel will be set via dependency injection
            Loaded += (s, e) =>
            {
                if (DataContext is MainWindowViewModel vm)
                {
                    _viewModel = vm;
                }
            };

            // Handle window closing
            Closing += (s, e) =>
            {
                // Cleanup resources
                _viewModel?.Dispose();
            };
        }

        protected override void OnSourceInitialized(System.EventArgs e)
        {
            base.OnSourceInitialized(e);

            // Set minimum window size
            MinWidth = 1000;
            MinHeight = 600;

            // Center window on screen
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
        }
    }
}
