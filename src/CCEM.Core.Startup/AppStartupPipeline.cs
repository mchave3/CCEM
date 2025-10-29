using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CCEM.Core.Logger;

namespace CCEM.Core.Startup;

/// <summary>
/// Orchestrates application startup by executing a series of operations with splash screen integration.
/// </summary>
public sealed class AppStartupPipeline
{
    private readonly IReadOnlyList<StartupOperation> _criticalOperations;
    private readonly IReadOnlyList<StartupOperation> _backgroundOperations;
    private readonly ISplashScreenHost _splashHost;
    private readonly IServiceProvider _serviceProvider;
    private readonly StartupPipelineOptions _options;
    private readonly IAppLogger? _logger;

    internal AppStartupPipeline(
        IReadOnlyList<StartupOperation> criticalOperations,
        IReadOnlyList<StartupOperation> backgroundOperations,
        ISplashScreenHost splashHost,
        IServiceProvider serviceProvider,
        StartupPipelineOptions options,
        IAppLogger? logger)
    {
        _criticalOperations = criticalOperations ?? throw new ArgumentNullException(nameof(criticalOperations));
        _backgroundOperations = backgroundOperations ?? throw new ArgumentNullException(nameof(backgroundOperations));
        _splashHost = splashHost ?? throw new ArgumentNullException(nameof(splashHost));
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _logger = logger;
    }

    /// <summary>
    /// Executes the configured startup pipeline.
    /// </summary>
    /// <param name="cancellationToken">Propagation token.</param>
    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        _splashHost.ShowSplash();
        var context = new StartupContext(_serviceProvider, _splashHost, _logger, cancellationToken);

        if (!string.IsNullOrWhiteSpace(_options.InitialStatusMessage))
        {
            UpdateStatusSafe(_options.InitialStatusMessage!);
        }

        foreach (StartupOperation operation in _criticalOperations)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!string.IsNullOrWhiteSpace(operation.StatusMessage))
            {
                UpdateStatusSafe(operation.StatusMessage!);
            }

            try
            {
                _logger?.Information("Starting startup operation {Operation}", operation.Name);
                await operation.ExecuteAsync(context).ConfigureAwait(false);
                _logger?.Information("Completed startup operation {Operation}", operation.Name);
            }
            catch (OperationCanceledException)
            {
                _logger?.Warning("Startup operation {Operation} was cancelled", operation.Name);
                throw;
            }
            catch (Exception ex)
            {
                _logger?.Fatal(ex, "Startup operation {Operation} failed", operation.Name);
                throw;
            }
        }

        if (!string.IsNullOrWhiteSpace(_options.CompletionStatusMessage))
        {
            UpdateStatusSafe(_options.CompletionStatusMessage!);
        }

        await _splashHost.EnterShellAsync(cancellationToken).ConfigureAwait(false);

        foreach (StartupOperation operation in _backgroundOperations)
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    _logger?.Information("Starting background startup operation {Operation}", operation.Name);
                    await operation.ExecuteAsync(context).ConfigureAwait(false);
                    _logger?.Information("Completed background startup operation {Operation}", operation.Name);
                }
                catch (OperationCanceledException)
                {
                    _logger?.Warning("Background startup operation {Operation} was cancelled", operation.Name);
                }
                catch (Exception ex)
                {
                    _logger?.Error(ex, "Background startup operation {Operation} failed", operation.Name);
                }
            }, context.CancellationToken);
        }
    }

    private void UpdateStatusSafe(string message)
    {
        try
        {
            _splashHost.UpdateStatus(message);
        }
        catch (Exception ex)
        {
            // Updating the status should never bring down the app. Log and continue.
            _logger?.Error(ex, "Failed to update splash status");
        }
    }
}

/// <summary>
/// Helps construct an <see cref="AppStartupPipeline"/>.
/// </summary>
public sealed class StartupPipelineBuilder
{
    private readonly List<StartupOperation> _criticalOperations = [];
    private readonly List<StartupOperation> _backgroundOperations = [];

    /// <summary>
    /// Adds an operation that must complete before the main shell is displayed.
    /// </summary>
    public StartupPipelineBuilder AddCriticalStep(
        string name,
        string? statusMessage,
        Func<StartupContext, CancellationToken, Task> action)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(action);

        _criticalOperations.Add(new StartupOperation(name, statusMessage, action));
        return this;
    }

    /// <summary>
    /// Adds an operation that runs in the background once the shell is visible.
    /// </summary>
    public StartupPipelineBuilder AddBackgroundStep(
        string name,
        Func<StartupContext, CancellationToken, Task> action)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(action);

        _backgroundOperations.Add(new StartupOperation(name, statusMessage: null, action));
        return this;
    }

    /// <summary>
    /// Materializes the startup pipeline with the provided collaborators.
    /// </summary>
    public AppStartupPipeline Build(
        ISplashScreenHost splashHost,
        IServiceProvider serviceProvider,
        StartupPipelineOptions? options = null,
        IAppLogger? logger = null)
    {
        var critical = _criticalOperations.Count == 0
            ? Array.Empty<StartupOperation>()
            : _criticalOperations.ToArray();
        var background = _backgroundOperations.Count == 0
            ? Array.Empty<StartupOperation>()
            : _backgroundOperations.ToArray();

        return new AppStartupPipeline(critical, background, splashHost, serviceProvider, options ?? new StartupPipelineOptions(), logger);
    }
}

/// <summary>
/// Provides configuration options for the startup pipeline.
/// </summary>
public sealed class StartupPipelineOptions
{
    /// <summary>
    /// Text displayed before the first critical operation executes.
    /// </summary>
    public string? InitialStatusMessage { get; set; }

    /// <summary>
    /// Text displayed after the final critical operation completes, right before entering the shell.
    /// </summary>
    public string? CompletionStatusMessage { get; set; }
}

/// <summary>
/// Context object passed to startup operations.
/// </summary>
public sealed class StartupContext
{
    private readonly IServiceProvider _serviceProvider;

    internal StartupContext(IServiceProvider serviceProvider, ISplashScreenHost splashHost, IAppLogger? logger, CancellationToken cancellationToken)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        SplashHost = splashHost ?? throw new ArgumentNullException(nameof(splashHost));
        Logger = logger;
        CancellationToken = cancellationToken;
    }

    /// <summary>
    /// Provides access to the splash host for advanced scenarios.
    /// </summary>
    public ISplashScreenHost SplashHost { get; }

    /// <summary>
    /// Gets the logger instance configured for startup orchestration.
    /// </summary>
    public IAppLogger? Logger { get; }

    /// <summary>
    /// Gets the cancellation token passed to the pipeline.
    /// </summary>
    public CancellationToken CancellationToken { get; }

    /// <summary>
    /// Retrieves a required service from the service provider.
    /// </summary>
    public T GetRequiredService<T>() where T : notnull
    {
        return (T)(GetService(typeof(T)) ?? throw new InvalidOperationException($"Service of type {typeof(T)} is not registered."));
    }

    /// <summary>
    /// Retrieves an optional service from the service provider.
    /// </summary>
    public T? GetService<T>() where T : class
    {
        return (T?)GetService(typeof(T));
    }

    private object? GetService(Type serviceType)
    {
        return _serviceProvider.GetService(serviceType);
    }
}

internal sealed class StartupOperation
{
    private readonly Func<StartupContext, CancellationToken, Task> _action;

    internal StartupOperation(string name, string? statusMessage, Func<StartupContext, CancellationToken, Task> action)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Name = name;
        StatusMessage = statusMessage;
        _action = action ?? throw new ArgumentNullException(nameof(action));
    }

    public string Name { get; }

    public string? StatusMessage { get; }

    public Task ExecuteAsync(StartupContext context)
    {
        return _action(context, context.CancellationToken);
    }
}
