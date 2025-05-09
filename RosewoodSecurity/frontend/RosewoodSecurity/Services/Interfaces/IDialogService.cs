using System.Threading.Tasks;

namespace RosewoodSecurity.Services
{
    public interface IDialogService
    {
        Task ShowErrorAsync(string title, string message);
        Task ShowWarningAsync(string title, string message);
        Task ShowSuccessAsync(string title, string message);
        Task ShowInfoAsync(string title, string message);
        Task<bool> ShowConfirmationAsync(string title, string message);
        Task<string> ShowInputAsync(string title, string message, string defaultValue = "");
        void ShowError(string title, string message);
        void ShowWarning(string title, string message);
        void ShowSuccess(string title, string message);
        void ShowInfo(string title, string message);
        bool ShowConfirmation(string title, string message);
    }
}
