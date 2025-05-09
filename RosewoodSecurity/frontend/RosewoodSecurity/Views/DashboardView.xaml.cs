using System.Windows.Controls;
using RosewoodSecurity.ViewModels;

namespace RosewoodSecurity.Views
{
    public partial class DashboardView : UserControl
    {
        public DashboardView()
        {
            InitializeComponent();

            // ViewModel will be set via dependency injection
            Loaded += (s, e) =>
            {
                if (DataContext is DashboardViewModel vm)
                {
                    // Any additional initialization if needed
                }
            };
        }
    }
}
