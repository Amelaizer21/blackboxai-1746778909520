using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using RosewoodSecurity.ViewModels;

namespace RosewoodSecurity.Services
{
    public class NavigationService : INavigationService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly Stack<object> _navigationStack;
        private object _currentViewModel;

        public event EventHandler<object> CurrentViewModelChanged;

        public NavigationService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _navigationStack = new Stack<object>();
        }

        public void NavigateTo<T>() where T : class
        {
            var viewModel = _serviceProvider.GetRequiredService<T>();
            NavigateToViewModel(viewModel);
        }

        public void NavigateTo(string viewName)
        {
            var viewModelType = Type.GetType($"RosewoodSecurity.ViewModels.{viewName}ViewModel");
            if (viewModelType == null)
            {
                throw new ArgumentException($"ViewModel not found for view: {viewName}");
            }

            var viewModel = _serviceProvider.GetService(viewModelType);
            if (viewModel == null)
            {
                throw new InvalidOperationException($"Failed to create ViewModel for view: {viewName}");
            }

            NavigateToViewModel(viewModel);
        }

        public bool CanNavigateTo<T>() where T : class
        {
            // Add any navigation validation logic here
            // For example, check if user has permission to access the view
            return true;
        }

        public void GoBack()
        {
            if (CanGoBack)
            {
                // Remove current view
                _navigationStack.Pop();

                // Get previous view
                if (_navigationStack.Any())
                {
                    var previousViewModel = _navigationStack.Peek();
                    SetCurrentViewModel(previousViewModel);
                }
                else
                {
                    SetCurrentViewModel(null);
                }
            }
        }

        public bool CanGoBack => _navigationStack.Count > 1;

        private void NavigateToViewModel(object viewModel)
        {
            // If we're navigating to the same type of view, don't add it to the stack
            if (_currentViewModel?.GetType() != viewModel.GetType())
            {
                _navigationStack.Push(viewModel);
            }

            SetCurrentViewModel(viewModel);
        }

        private void SetCurrentViewModel(object viewModel)
        {
            if (_currentViewModel is IDisposable disposable)
            {
                disposable.Dispose();
            }

            _currentViewModel = viewModel;
            CurrentViewModelChanged?.Invoke(this, _currentViewModel);
        }
    }
}
