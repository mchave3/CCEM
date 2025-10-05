namespace CCEM.Views.SCCM;

/// <summary>
/// Connection page for SCCM client management
/// </summary>
public sealed partial class ConnectionPage : Page
{
    /// <summary>
    /// Gets the view model for this page
    /// </summary>
    public ConnectionViewModel ViewModel { get; }

    /// <summary>
    /// Initializes a new instance of the ConnectionPage class
    /// </summary>
    public ConnectionPage()
    {
        ViewModel = App.GetService<ConnectionViewModel>();
        this.InitializeComponent();
    }
}
