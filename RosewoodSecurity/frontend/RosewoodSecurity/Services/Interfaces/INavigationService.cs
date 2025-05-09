using System;

namespace RosewoodSecurity.Services
{
    public interface INavigationService
    {
        event EventHandler<object> CurrentViewModelChanged;
        
        void NavigateTo<T>() where T : class;
        void NavigateTo(string viewName);
        bool CanNavigateTo<T>() where T : class;
        void GoBack();
        bool CanGoBack { get; }
    }
}
