namespace CCEM.SCCM.Services;

/// <summary>
/// Service interface for managing SCCM plugins and extensions
/// </summary>
public interface ISCCMPluginService
{
    /// <summary>
    /// Gets the list of available plugin categories
    /// </summary>
    IReadOnlyList<string> AvailableCategories { get; }

    /// <summary>
    /// Loads plugins from the specified directory
    /// </summary>
    /// <param name="pluginDirectory">Directory containing plugins</param>
    /// <returns>Number of plugins loaded</returns>
    Task<int> LoadPluginsAsync(string pluginDirectory);

    /// <summary>
    /// Gets plugins by category
    /// </summary>
    /// <param name="category">The plugin category</param>
    /// <returns>List of plugin names in the category</returns>
    IReadOnlyList<string> GetPluginsByCategory(string category);

    /// <summary>
    /// Executes a plugin action
    /// </summary>
    /// <param name="pluginName">Name of the plugin</param>
    /// <param name="parameters">Optional parameters for the plugin</param>
    /// <returns>Result of the plugin execution</returns>
    Task<object?> ExecutePluginAsync(string pluginName, Dictionary<string, object>? parameters = null);
}
