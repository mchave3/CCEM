using CCEM.SCCM.Models;

namespace CCEM.SCCM.Services;

/// <summary>
/// Service interface for retrieving SCCM client data
/// </summary>
public interface ISCCMDataService
{
    /// <summary>
    /// Gets the list of SCCM client components
    /// </summary>
    /// <returns>List of component models</returns>
    Task<IReadOnlyList<ComponentModel>> GetComponentsAsync();

    /// <summary>
    /// Gets the list of installed applications
    /// </summary>
    /// <returns>List of application models</returns>
    Task<IReadOnlyList<ApplicationModel>> GetApplicationsAsync();

    /// <summary>
    /// Gets the list of software updates
    /// </summary>
    /// <returns>List of update models</returns>
    Task<IReadOnlyList<UpdateModel>> GetUpdatesAsync();

    /// <summary>
    /// Gets the list of SCCM services
    /// </summary>
    /// <returns>List of service models</returns>
    Task<IReadOnlyList<ServiceModel>> GetServicesAsync();

    /// <summary>
    /// Gets the list of policies
    /// </summary>
    /// <returns>List of policy models</returns>
    Task<IReadOnlyList<PolicyModel>> GetPoliciesAsync();

    /// <summary>
    /// Triggers a specific SCCM client action
    /// </summary>
    /// <param name="actionId">The action identifier</param>
    /// <returns>True if action triggered successfully</returns>
    Task<bool> TriggerActionAsync(string actionId);

    /// <summary>
    /// Refreshes the cache for a specific data type
    /// </summary>
    /// <param name="dataType">The type of data to refresh</param>
    Task RefreshCacheAsync(string dataType);
}
