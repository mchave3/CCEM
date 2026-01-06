using System;
using System.Threading.Tasks;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace CCEM.Services;

public interface IUpdateDialogService
{
    Task<bool> ShowUpdateAvailableDialogAsync(string? availableVersion, Func<Action<int>, Task> downloadAsync);
}

public sealed class UpdateDialogService : IUpdateDialogService
{
    public Task<bool> ShowUpdateAvailableDialogAsync(string? availableVersion, Func<Action<int>, Task> downloadAsync)
    {
        if (downloadAsync is null)
        {
            throw new ArgumentNullException(nameof(downloadAsync));
        }

        var dispatcher = App.MainWindow?.DispatcherQueue;
        if (dispatcher is null)
        {
            return Task.FromResult(false);
        }

        var completion = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        if (!dispatcher.TryEnqueue(() => ShowDialog(availableVersion, downloadAsync, completion)))
        {
            completion.TrySetResult(false);
        }

        return completion.Task;
    }

    private static void ShowDialog(string? availableVersion, Func<Action<int>, Task> downloadAsync, TaskCompletionSource<bool> completion)
    {
        var dispatcher = App.MainWindow?.DispatcherQueue;
        if (dispatcher is null)
        {
            completion.TrySetResult(false);
            return;
        }

        var statusText = new TextBlock
        {
            Text = $"A new version ({availableVersion ?? "unknown"}) is available. Would you like to download and install it now?",
            TextWrapping = TextWrapping.Wrap
        };

        var progressBar = new ProgressBar
        {
            Minimum = 0,
            Maximum = 100,
            Value = 0,
            Visibility = Visibility.Collapsed,
            Margin = new Thickness(0, 12, 0, 0)
        };

        var layout = new StackPanel
        {
            Spacing = 8
        };
        layout.Children.Add(statusText);
        layout.Children.Add(progressBar);

        var dialog = new ContentDialog
        {
            Title = "Update Available",
            Content = layout,
            PrimaryButtonText = "Install now",
            CloseButtonText = "Later",
            DefaultButton = ContentDialogButton.Primary
        };

        if (App.MainWindow?.Content is FrameworkElement root)
        {
            dialog.XamlRoot = root.XamlRoot;
        }

        async void OnPrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            args.Cancel = true;

            sender.IsPrimaryButtonEnabled = false;
            dialog.CloseButtonText = string.Empty;
            progressBar.Visibility = Visibility.Visible;
            statusText.Text = "Downloading update... 0%";

            void UpdateProgress(int value)
            {
                progressBar.Value = value;
                statusText.Text = $"Downloading update... {value}%";
            }

            void ReportProgress(int value)
            {
                var queue = dispatcher;
                if (queue is not null && !queue.HasThreadAccess)
                {
                    queue.TryEnqueue(() => UpdateProgress(value));
                }
                else
                {
                    UpdateProgress(value);
                }
            }

            try
            {
                await downloadAsync(ReportProgress);
                UpdateProgress(100);
                statusText.Text = "Download complete.";
                completion.TrySetResult(true);
                sender.Hide();
            }
            catch (Exception ex)
            {
                Logger?.Error(ex, "Failed to download update.");

                statusText.Text = ex.Message;
                progressBar.Visibility = Visibility.Collapsed;
                sender.IsPrimaryButtonEnabled = true;
                dialog.CloseButtonText = "Later";
            }
        }

        dialog.PrimaryButtonClick += OnPrimaryButtonClick;

        dialog.Closed += (_, _) =>
        {
            dialog.PrimaryButtonClick -= OnPrimaryButtonClick;

            if (!completion.Task.IsCompleted)
            {
                completion.TrySetResult(false);
            }
        };

        _ = dialog.ShowAsync();
    }
}
