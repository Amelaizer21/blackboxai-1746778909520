using System.Threading.Tasks;
using System.Windows;
using MaterialDesignThemes.Wpf;

namespace RosewoodSecurity.Services
{
    public class DialogService : IDialogService
    {
        private readonly ISnackbarMessageQueue _messageQueue;

        public DialogService(ISnackbarMessageQueue messageQueue)
        {
            _messageQueue = messageQueue;
        }

        public async Task ShowErrorAsync(string title, string message)
        {
            await ShowDialogAsync(title, message, PackIconKind.Error, "MaterialDesignErrorDarkBackground");
        }

        public async Task ShowWarningAsync(string title, string message)
        {
            await ShowDialogAsync(title, message, PackIconKind.Alert, "MaterialDesignWarningDarkBackground");
        }

        public async Task ShowSuccessAsync(string title, string message)
        {
            await ShowDialogAsync(title, message, PackIconKind.CheckCircle, "MaterialDesignSuccessDarkBackground");
        }

        public async Task ShowInfoAsync(string title, string message)
        {
            await ShowDialogAsync(title, message, PackIconKind.Information, "MaterialDesignInfoDarkBackground");
        }

        public async Task<bool> ShowConfirmationAsync(string title, string message)
        {
            var dialog = new ContentDialog
            {
                Title = title,
                Content = message,
                PrimaryButtonText = "Yes",
                SecondaryButtonText = "No",
                DefaultButton = ContentDialogButton.Primary
            };

            var result = await dialog.ShowAsync();
            return result == ContentDialogResult.Primary;
        }

        public async Task<string> ShowInputAsync(string title, string message, string defaultValue = "")
        {
            var dialog = new ContentDialog
            {
                Title = title,
                Content = new TextBox
                {
                    Text = defaultValue,
                    Margin = new Thickness(0, 8, 0, 0)
                },
                PrimaryButtonText = "OK",
                SecondaryButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Primary
            };

            var result = await dialog.ShowAsync();
            return result == ContentDialogResult.Primary
                ? ((TextBox)dialog.Content).Text
                : null;
        }

        public void ShowError(string title, string message)
        {
            _messageQueue.Enqueue($"{title}: {message}", null, null, null, false, true, TimeSpan.FromSeconds(3));
        }

        public void ShowWarning(string title, string message)
        {
            _messageQueue.Enqueue($"{title}: {message}", null, null, null, false, true, TimeSpan.FromSeconds(3));
        }

        public void ShowSuccess(string title, string message)
        {
            _messageQueue.Enqueue($"{title}: {message}", null, null, null, false, true, TimeSpan.FromSeconds(3));
        }

        public void ShowInfo(string title, string message)
        {
            _messageQueue.Enqueue($"{title}: {message}", null, null, null, false, true, TimeSpan.FromSeconds(3));
        }

        public bool ShowConfirmation(string title, string message)
        {
            return MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;
        }

        private async Task ShowDialogAsync(string title, string message, PackIconKind icon, string backgroundResourceKey)
        {
            var dialog = new ContentDialog
            {
                Title = title,
                Content = new StackPanel
                {
                    Children =
                    {
                        new PackIcon
                        {
                            Kind = icon,
                            Width = 32,
                            Height = 32,
                            Margin = new Thickness(0, 0, 0, 8)
                        },
                        new TextBlock
                        {
                            Text = message,
                            TextWrapping = TextWrapping.Wrap
                        }
                    }
                },
                PrimaryButtonText = "OK",
                DefaultButton = ContentDialogButton.Primary,
                Background = (Brush)Application.Current.Resources[backgroundResourceKey]
            };

            await dialog.ShowAsync();
        }

        private class ContentDialog : Window
        {
            public string PrimaryButtonText { get; set; }
            public string SecondaryButtonText { get; set; }
            public ContentDialogButton DefaultButton { get; set; }

            public async Task<ContentDialogResult> ShowAsync()
            {
                var tcs = new TaskCompletionSource<ContentDialogResult>();

                var primaryButton = new Button
                {
                    Content = PrimaryButtonText,
                    Style = (Style)Application.Current.Resources["MaterialDesignRaisedButton"],
                    Margin = new Thickness(8)
                };

                var secondaryButton = new Button
                {
                    Content = SecondaryButtonText,
                    Style = (Style)Application.Current.Resources["MaterialDesignOutlinedButton"],
                    Margin = new Thickness(8)
                };

                primaryButton.Click += (s, e) =>
                {
                    tcs.SetResult(ContentDialogResult.Primary);
                    Close();
                };

                secondaryButton.Click += (s, e) =>
                {
                    tcs.SetResult(ContentDialogResult.Secondary);
                    Close();
                };

                var buttonPanel = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    HorizontalAlignment = HorizontalAlignment.Right,
                    Margin = new Thickness(0, 16, 0, 0)
                };

                if (!string.IsNullOrEmpty(SecondaryButtonText))
                {
                    buttonPanel.Children.Add(secondaryButton);
                }
                buttonPanel.Children.Add(primaryButton);

                var mainPanel = new StackPanel
                {
                    Margin = new Thickness(24)
                };

                if (!string.IsNullOrEmpty(Title))
                {
                    mainPanel.Children.Add(new TextBlock
                    {
                        Text = Title,
                        Style = (Style)Application.Current.Resources["MaterialDesignHeadline6TextBlock"],
                        Margin = new Thickness(0, 0, 0, 16)
                    });
                }

                mainPanel.Children.Add(Content as UIElement);
                mainPanel.Children.Add(buttonPanel);

                Content = mainPanel;
                WindowStartupLocation = WindowStartupLocation.CenterOwner;
                Owner = Application.Current.MainWindow;
                ShowInTaskbar = false;
                ResizeMode = ResizeMode.NoResize;
                SizeToContent = SizeToContent.WidthAndHeight;
                WindowStyle = WindowStyle.None;
                Background = (Brush)Application.Current.Resources["MaterialDesignPaper"];

                if (DefaultButton == ContentDialogButton.Primary)
                {
                    primaryButton.Focus();
                }
                else
                {
                    secondaryButton.Focus();
                }

                ShowDialog();
                return await tcs.Task;
            }
        }

        private enum ContentDialogButton
        {
            Primary,
            Secondary
        }

        private enum ContentDialogResult
        {
            None,
            Primary,
            Secondary
        }
    }
}
