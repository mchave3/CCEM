using CCEM.ViewModels.Sccm;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace CCEM.Views;

public sealed partial class InventoryComponentsPage : Page
{
    public InventoryComponentsViewModel ViewModel { get; } = App.GetService<InventoryComponentsViewModel>();

    public InventoryComponentsPage()
    {
        InitializeComponent();
        DataContext = ViewModel;
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        await ViewModel.RefreshAsync();
    }

    private async void OnRefreshClicked(object sender, RoutedEventArgs e)
    {
        await ViewModel.RefreshAsync();
    }
}
