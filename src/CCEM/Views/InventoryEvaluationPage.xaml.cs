using CCEM.ViewModels.Sccm;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace CCEM.Views;

public sealed partial class InventoryEvaluationPage : Page
{
    public InventoryEvaluationViewModel ViewModel { get; } = App.GetService<InventoryEvaluationViewModel>();

    public InventoryEvaluationPage()
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
