using System;
using System.Collections.Generic;
using System.Windows.Controls;
using RosewoodSecurity.ViewModels;

namespace RosewoodSecurity.Services
{
    public interface INavigationService
    {
        // Navigation state
        BaseViewModel CurrentViewModel { get; }
        string CurrentPage { get; }
        bool CanGoBack { get; }
        bool CanGoForward { get; }
        
        // Basic navigation
        void NavigateTo(string pageName, object parameter = null);
        void NavigateToViewModel<TViewModel>(object parameter = null) where TViewModel : BaseViewModel;
        bool GoBack();
        bool GoForward();
        void Refresh();
        
        // Frame management
        void Initialize(Frame frame);
        void ClearHistory();
        void RemoveBackEntry();
        
        // Navigation history
        IEnumerable<string> GetNavigationHistory();
        void ClearNavigationHistory();
        
        // Page registration
        void RegisterPage<TViewModel>(string pageName, Type pageType) where TViewModel : BaseViewModel;
        void UnregisterPage(string pageName);
        
        // Navigation events
        event EventHandler<NavigationEventArgs> Navigating;
        event EventHandler<NavigationEventArgs> Navigated;
        event EventHandler<NavigationFailedEventArgs> NavigationFailed;
        
        // State management
        void SaveState();
        void RestoreState();
        void ClearState();
        
        // Modal navigation
        void ShowModal(string pageName, object parameter = null);
        void CloseModal(object result = null);
        
        // Navigation configuration
        void ConfigureTransitions(NavigationTransitionType transitionType);
        void SetNavigationCaching(bool enableCaching);
    }

    public class NavigationEventArgs : EventArgs
    {
        public string SourcePage { get; }
        public string TargetPage { get; }
        public object Parameter { get; }
        public bool Cancel { get; set; }

        public NavigationEventArgs(string sourcePage, string targetPage, object parameter = null)
        {
            SourcePage = sourcePage;
            TargetPage = targetPage;
            Parameter = parameter;
        }
    }

    public class NavigationFailedEventArgs : EventArgs
    {
        public string TargetPage { get; }
        public Exception Exception { get; }
        public bool Handled { get; set; }

        public NavigationFailedEventArgs(string targetPage, Exception exception)
        {
            TargetPage = targetPage;
            Exception = exception;
        }
    }

    public enum NavigationTransitionType
    {
        None,
        Slide,
        Fade,
        Custom
    }

    public interface INavigationAware
    {
        void OnNavigatedTo(object parameter);
        void OnNavigatedFrom();
        bool CanNavigateFrom();
    }

    public class NavigationService : INavigationService
    {
        private readonly Dictionary<string, (Type PageType, Type ViewModelType)> _pageRegistry;
        private readonly Stack<string> _navigationStack;
        private readonly Stack<string> _forwardStack;
        private Frame _frame;
        private bool _enableCaching;
        private NavigationTransitionType _transitionType;

        public BaseViewModel CurrentViewModel { get; private set; }
        public string CurrentPage { get; private set; }
        public bool CanGoBack => _navigationStack.Count > 0;
        public bool CanGoForward => _forwardStack.Count > 0;

        public event EventHandler<NavigationEventArgs> Navigating;
        public event EventHandler<NavigationEventArgs> Navigated;
        public event EventHandler<NavigationFailedEventArgs> NavigationFailed;

        public NavigationService()
        {
            _pageRegistry = new Dictionary<string, (Type PageType, Type ViewModelType)>();
            _navigationStack = new Stack<string>();
            _forwardStack = new Stack<string>();
            _enableCaching = false;
            _transitionType = NavigationTransitionType.None;
        }

        public void Initialize(Frame frame)
        {
            _frame = frame ?? throw new ArgumentNullException(nameof(frame));
        }

        public void RegisterPage<TViewModel>(string pageName, Type pageType) where TViewModel : BaseViewModel
        {
            if (string.IsNullOrEmpty(pageName))
                throw new ArgumentNullException(nameof(pageName));

            if (pageType == null)
                throw new ArgumentNullException(nameof(pageType));

            _pageRegistry[pageName] = (pageType, typeof(TViewModel));
        }

        public void NavigateTo(string pageName, object parameter = null)
        {
            if (!_pageRegistry.ContainsKey(pageName))
                throw new ArgumentException($"Page {pageName} is not registered");

            var args = new NavigationEventArgs(CurrentPage, pageName, parameter);
            Navigating?.Invoke(this, args);

            if (args.Cancel)
                return;

            try
            {
                var (pageType, viewModelType) = _pageRegistry[pageName];
                var page = Activator.CreateInstance(pageType);
                var viewModel = App.GetService(viewModelType) as BaseViewModel;

                if (page is FrameworkElement frameworkElement)
                    frameworkElement.DataContext = viewModel;

                _frame.Navigate(page);
                CurrentViewModel = viewModel;
                CurrentPage = pageName;

                if (CurrentPage != null)
                    _navigationStack.Push(CurrentPage);

                _forwardStack.Clear();

                (viewModel as INavigationAware)?.OnNavigatedTo(parameter);
                Navigated?.Invoke(this, args);
            }
            catch (Exception ex)
            {
                var failedArgs = new NavigationFailedEventArgs(pageName, ex);
                NavigationFailed?.Invoke(this, failedArgs);

                if (!failedArgs.Handled)
                    throw;
            }
        }

        // Implement other interface methods...
    }
}
