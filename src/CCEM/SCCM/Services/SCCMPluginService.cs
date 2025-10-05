using System;
using System.Collections.Generic;
using System.Linq;

namespace CCEM.SCCM.Services;

/// <summary>
/// Implementation of SCCM plugin service
/// </summary>
public class SCCMPluginService : ISCCMPluginService
{
    private readonly Dictionary<string, List<string>> _pluginsByCategory = new();
    private readonly Dictionary<string, object> _loadedPlugins = new();

    public IReadOnlyList<string> AvailableCategories => _pluginsByCategory.Keys.ToList();

    public SCCMPluginService()
    {
        SeedBuiltInShortcuts();
    }

    /// <summary>
    /// Loads plugins from the specified directory
    /// </summary>
    public async Task<int> LoadPluginsAsync(string pluginDirectory)
    {
        // Placeholder implementation
        // In Phase 3, this will scan the directory for plugin assemblies
        // and load them dynamically
        await Task.CompletedTask;
        return 0;
    }

    /// <summary>
    /// Gets plugins by category
    /// </summary>
    public IReadOnlyList<string> GetPluginsByCategory(string category)
    {
        if (_pluginsByCategory.TryGetValue(category, out var plugins))
        {
            return plugins;
        }
        return Array.Empty<string>();
    }

    /// <summary>
    /// Executes a plugin action
    /// </summary>
    public async Task<object?> ExecutePluginAsync(string pluginName, Dictionary<string, object>? parameters = null)
    {
        // Placeholder implementation
        // In Phase 3, this will execute the plugin with the provided parameters
        await Task.CompletedTask;
        return null;
    }
    private void SeedBuiltInShortcuts()
    {
        RegisterPlugin("Remote tools", "Remote Desktop");
        RegisterPlugin("Remote tools", "Remote Assistance");
        RegisterPlugin("Remote tools", "Computer Management");
        RegisterPlugin("Diagnostics", "Resource Explorer");
        RegisterPlugin("Diagnostics", "Status Message Viewer");
        RegisterPlugin("Diagnostics", "System Information");
        RegisterPlugin("Automation", "PowerShell Scripts");
        RegisterPlugin("Automation", "Enable PS Remoting");
    }

    private void RegisterPlugin(string category, string pluginName)
    {
        if (!_pluginsByCategory.TryGetValue(category, out var list))
        {
            list = new List<string>();
            _pluginsByCategory[category] = list;
        }

        if (!list.Contains(pluginName, StringComparer.OrdinalIgnoreCase))
        {
            list.Add(pluginName);
        }
    }
}
