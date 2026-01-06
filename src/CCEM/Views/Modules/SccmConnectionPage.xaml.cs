using CCEM.ViewModels.Modules;

namespace CCEM.Views.Modules;

public sealed partial class SccmConnectionPage : Page
{
    public SccmConnectionViewModel ViewModel { get; }

    public SccmConnectionPage()
    {
        ViewModel = App.GetService<SccmConnectionViewModel>();
        InitializeComponent();
    }

    private void PasswordBox_OnPasswordChanged(object sender, RoutedEventArgs e)
    {
        if (sender is PasswordBox pb)
        {
            ViewModel.Password = pb.Password;
        }
    }
}

