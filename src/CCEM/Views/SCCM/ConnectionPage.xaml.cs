namespace CCEM.Views.SCCM;

public sealed partial class ConnectionPage : Page
{
    public ConnectionViewModel ViewModel { get; }

    public ConnectionPage()
    {
        ViewModel = App.GetService<ConnectionViewModel>();
        InitializeComponent();
    }

    private void HostnameBox_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
    {
        if (args.SelectedItem is string hostname)
        {
            ViewModel.TargetHost = hostname;
        }
    }

    private void OptionsButton_Click(object sender, RoutedEventArgs e)
    {
        ConnectionOptionsTip.IsOpen = !ConnectionOptionsTip.IsOpen;
    }

    private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        if (sender is PasswordBox passwordBox)
        {
            ViewModel.UpdatePassword(passwordBox.Password);
        }
    }
}
