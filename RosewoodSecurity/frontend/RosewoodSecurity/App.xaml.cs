using System;
using System.Windows;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RosewoodSecurity.Services;
using RosewoodSecurity.ViewModels;
using RosewoodSecurity.Views;
using Serilog;
using System.IO;

namespace RosewoodSecurity
{
    public partial class App : Application
    {
        private IServiceProvider _serviceProvider;
        private IConfiguration _configuration;

        public App()
        {
            // Initialize configuration
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            _configuration = builder.Build();

            // Initialize logging
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(_configuration)
                .WriteTo.File(
                    _configuration["Logging:FilePath"],
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: int.Parse(_configuration["Logging:RetainDays"]))
                .CreateLogger();

            // Configure services
            var services = new ServiceCollection();
            ConfigureServices(services);
            _serviceProvider = services.BuildServiceProvider();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            // Register configuration
            services.AddSingleton(_configuration);

            // Register services
            services.AddSingleton<IAuthenticationService, AuthenticationService>();
            services.AddSingleton<IApiService, ApiService>();
            services.AddSingleton<IDialogService, DialogService>();
            services.AddSingleton<INavigationService, NavigationService>();
            services.AddSingleton<IThemeService, ThemeService>();
            services.AddSingleton<ISettingsService, SettingsService>();

            // Register ViewModels
            services.AddTransient<MainWindowViewModel>();
            services.AddTransient<LoginViewModel>();
            services.AddTransient<DashboardViewModel>();
            services.AddTransient<CheckInOutViewModel>();
            services.AddTransient<KeyManagementViewModel>();
            services.AddTransient<AccessCardManagementViewModel>();
            services.AddTransient<EmployeeManagementViewModel>();
            services.AddTransient<TransactionViewModel>();
            services.AddTransient<ReportViewModel>();
            services.AddTransient<SettingsViewModel>();

            // Register Views
            services.AddTransient<MainWindow>();
            services.AddTransient<LoginView>();
            services.AddTransient<DashboardView>();
            services.AddTransient<CheckInOutView>();
            services.AddTransient<KeyManagementView>();
            services.AddTransient<AccessCardManagementView>();
            services.AddTransient<EmployeeManagementView>();
            services.AddTransient<TransactionView>();
            services.AddTransient<ReportView>();
            services.AddTransient<SettingsView>();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            try
            {
                // Set up exception handling
                Current.DispatcherUnhandledException += Current_DispatcherUnhandledException;
                AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

                // Get theme service and initialize theme
                var themeService = _serviceProvider.GetRequiredService<IThemeService>();
                themeService.Initialize();

                // Show main window
                var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
                mainWindow.DataContext = _serviceProvider.GetRequiredService<MainWindowViewModel>();
                mainWindow.Show();

                Log.Information("Application started successfully");
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Application failed to start");
                MessageBox.Show("Failed to start application. Please check the logs for details.",
                    "Startup Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Current.Shutdown();
            }
        }

        private void Current_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            Log.Error(e.Exception, "Unhandled exception occurred");
            MessageBox.Show($"An error occurred: {e.Exception.Message}",
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            e.Handled = true;
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var exception = e.ExceptionObject as Exception;
            Log.Fatal(exception, "Fatal unhandled exception occurred");
            MessageBox.Show($"A fatal error occurred: {exception?.Message}",
                "Fatal Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
            Log.Information("Application shutting down");
            Log.CloseAndFlush();
        }

        public static IServiceProvider GetServiceProvider()
        {
            var app = Current as App;
            return app._serviceProvider;
        }

        public static T GetService<T>()
        {
            var app = Current as App;
            return app._serviceProvider.GetService<T>();
        }

        public static IConfiguration GetConfiguration()
        {
            var app = Current as App;
            return app._configuration;
        }
    }
}
