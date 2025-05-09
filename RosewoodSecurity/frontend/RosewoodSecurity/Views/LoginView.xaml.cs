using System.Windows.Controls;
using RosewoodSecurity.ViewModels;

namespace RosewoodSecurity.Views
{
    public partial class LoginView : UserControl
    {
        private LoginViewModel _viewModel;

        public LoginView()
        {
            InitializeComponent();
            
            // ViewModel will be set via dependency injection
            Loaded += (s, e) =>
            {
                if (DataContext is LoginViewModel vm)
                {
                    _viewModel = vm;
                    // Set up the password getter for the ViewModel
                    _viewModel.GetPassword = () => PasswordBox.Password;
                }
            };
        }
    }
}
