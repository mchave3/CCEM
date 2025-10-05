using System.Collections.Specialized;
using System.Linq;
using Microsoft.UI.Windowing;

namespace CCEM.Views;

public sealed partial class MainWindow : Window
{
    public MainViewModel ViewModel { get; }
    private readonly MenuFlyout _pluginsFlyout = new();

    public MainWindow()
    {
        ViewModel = App.GetService<MainViewModel>();
        InitializeComponent();
        DataContext = ViewModel;

        ExtendsContentIntoTitleBar = true;
        SetTitleBar(AppTitleBar);
        AppWindow.TitleBar.PreferredHeightOption = TitleBarHeightOption.Tall;

        PluginsButton.Flyout = _pluginsFlyout;
        HookPluginCollections();
        UpdatePluginFlyout();

        if (App.GetService<IJsonNavigationService>() is JsonNavigationService navService)
        {
            navService.Initialize(NavView, NavFrame, NavigationPageMappings.PageDictionary)
                .ConfigureDefaultPage(typeof(HomeLandingPage))
                .ConfigureSettingsPage(typeof(SettingsPage))
                .ConfigureJsonFile("Assets/NavViewMenu/AppData.json")
                .ConfigureTitleBar(AppTitleBar)
                .ConfigureBreadcrumbBar(BreadCrumbNav, BreadcrumbPageMappings.PageDictionary);
        }

        Closed += OnWindowClosed;
    }

    private async void ThemeButton_Click(object sender, RoutedEventArgs e)
    {
        await App.Current.ThemeService.SetElementThemeWithoutSaveAsync();
    }

    private void OnTextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        AutoSuggestBoxHelper.OnITitleBarAutoSuggestBoxTextChangedEvent(sender, args, NavFrame);
    }

    private void OnQuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
    {
        AutoSuggestBoxHelper.OnITitleBarAutoSuggestBoxQuerySubmittedEvent(sender, args, NavFrame);
    }

    private void HookPluginCollections()
    {
        ViewModel.PluginCategories.CollectionChanged += OnPluginCategoriesChanged;
        foreach (var category in ViewModel.PluginCategories)
        {
            category.Plugins.CollectionChanged += OnPluginCollectionChanged;
        }
    }

    private void OnPluginCategoriesChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.OldItems != null)
        {
            foreach (MainViewModel.PluginCategoryViewModel category in e.OldItems)
            {
                category.Plugins.CollectionChanged -= OnPluginCollectionChanged;
            }
        }

        if (e.NewItems != null)
        {
            foreach (MainViewModel.PluginCategoryViewModel category in e.NewItems)
            {
                category.Plugins.CollectionChanged += OnPluginCollectionChanged;
            }
        }

        UpdatePluginFlyout();
    }

    private void OnPluginCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        UpdatePluginFlyout();
    }

    private void UpdatePluginFlyout()
    {
        DetachPluginHandlers();
        _pluginsFlyout.Items.Clear();

        var populatedCategories = ViewModel.PluginCategories
            .Where(category => category.Plugins.Count > 0)
            .ToList();

        if (populatedCategories.Count == 0)
        {
            _pluginsFlyout.Items.Add(new MenuFlyoutItem
            {
                Text = "Plugins coming soon",
                IsEnabled = false
            });
            return;
        }

        foreach (var category in populatedCategories)
        {
            var subItem = new MenuFlyoutSubItem { Text = category.Name };
            foreach (var plugin in category.Plugins)
            {
                var item = new MenuFlyoutItem
                {
                    Text = plugin.DisplayName,
                    Tag = plugin
                };
                item.Click += OnPluginMenuItemClick;
                subItem.Items.Add(item);
            }

            _pluginsFlyout.Items.Add(subItem);
        }
    }

    private void DetachPluginHandlers()
    {
        foreach (var subItem in _pluginsFlyout.Items.OfType<MenuFlyoutSubItem>())
        {
            foreach (var menuItem in subItem.Items.OfType<MenuFlyoutItem>())
            {
                menuItem.Click -= OnPluginMenuItemClick;
            }
        }
    }

    private async void OnPluginMenuItemClick(object sender, RoutedEventArgs e)
    {
        if (sender is MenuFlyoutItem { Tag: MainViewModel.PluginShortcut plugin })
        {
            await ViewModel.ExecutePluginAsync(plugin);
        }
    }

    private void OnWindowClosed(object sender, WindowEventArgs args)
    {
        ViewModel.PluginCategories.CollectionChanged -= OnPluginCategoriesChanged;
        foreach (var category in ViewModel.PluginCategories)
        {
            category.Plugins.CollectionChanged -= OnPluginCollectionChanged;
        }

        DetachPluginHandlers();
        ViewModel.Dispose();
    }
}
