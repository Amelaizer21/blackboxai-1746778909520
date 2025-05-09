using System.Threading.Tasks;
using System.Windows;

namespace RosewoodSecurity.Services
{
    public interface IDialogService
    {
        // Message dialogs
        Task ShowInfoAsync(string title, string message);
        Task ShowWarningAsync(string title, string message);
        Task ShowErrorAsync(string title, string message);
        Task ShowSuccessAsync(string title, string message);
        
        // Confirmation dialogs
        Task<bool> ShowConfirmationAsync(string title, string message);
        Task<bool> ShowDeleteConfirmationAsync(string itemType, string itemName);
        Task<bool> ShowUnsavedChangesConfirmationAsync();
        
        // Input dialogs
        Task<string> ShowInputDialogAsync(string title, string message, string defaultValue = "");
        Task<string> ShowPasswordDialogAsync(string title, string message);
        Task<(string Username, string Password)> ShowCredentialsDialogAsync(string title, string message);
        
        // Custom dialogs
        Task<T> ShowCustomDialogAsync<T>(string title, object content) where T : class;
        Task ShowProgressDialogAsync(string title, string message, Task task);
        
        // File dialogs
        Task<string> ShowOpenFileDialogAsync(string title, string filter);
        Task<string> ShowSaveFileDialogAsync(string title, string defaultFileName, string filter);
        
        // Notifications
        void ShowNotification(string message, NotificationType type = NotificationType.Information);
        void ShowToast(string message, int durationMs = 3000);
        
        // Status messages
        void UpdateStatusMessage(string message);
        void ClearStatusMessage();
    }

    public enum NotificationType
    {
        Information,
        Success,
        Warning,
        Error
    }

    public class DialogResult<T>
    {
        public bool IsConfirmed { get; set; }
        public T Result { get; set; }

        public DialogResult(bool isConfirmed = false, T result = default)
        {
            IsConfirmed = isConfirmed;
            Result = result;
        }
    }

    public class DialogSettings
    {
        public string Title { get; set; }
        public string Message { get; set; }
        public string OkButtonText { get; set; } = "OK";
        public string CancelButtonText { get; set; } = "Cancel";
        public MessageBoxImage Icon { get; set; } = MessageBoxImage.None;
        public bool IsCancellable { get; set; } = true;
        public Window Owner { get; set; }
        public WindowStartupLocation StartupLocation { get; set; } = WindowStartupLocation.CenterOwner;
        
        public DialogSettings(string title = null, string message = null)
        {
            Title = title;
            Message = message;
        }
    }

    public interface IDialog
    {
        string Title { get; set; }
        object Content { get; set; }
        bool? DialogResult { get; set; }
        Window Owner { get; set; }
        
        bool? ShowDialog();
        void Close();
    }

    public interface IProgressDialog : IDialog
    {
        double Progress { get; set; }
        string StatusMessage { get; set; }
        bool IsCancellable { get; set; }
        bool IsCancelled { get; }
        
        void SetProgress(double progress);
        void SetMessage(string message);
        void Complete();
    }

    public interface IInputDialog : IDialog
    {
        string InputText { get; set; }
        string ValidationError { get; set; }
        bool IsValid { get; }
        
        void SetValidationRule(System.Predicate<string> validationRule, string errorMessage);
    }

    public interface ICustomDialog<T> : IDialog where T : class
    {
        T DialogData { get; set; }
        void Initialize(T data);
        bool ValidateData();
    }
}
