using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using CCEM.Core.Velopack.Models;
using CCEM.Core.Velopack.Services;
using Velopack;
using Velopack.Locators;
using Velopack.Sources;

namespace CCEM.Core.Velopack.Tests;

public sealed class VelopackUpdateConfigurationTests
{
    [Fact]
    public void Constructor_Throws_WhenRepositoryUrlIsNullOrWhitespace()
    {
        Assert.Throws<ArgumentException>(() => new VelopackUpdateConfiguration(" "));
    }

    [Fact]
    public void Constructor_SetsRepositoryUrl()
    {
        const string repositoryUrl = "https://github.com/example/repo";
        var configuration = new VelopackUpdateConfiguration(repositoryUrl);

        Assert.Equal(repositoryUrl, configuration.RepositoryUrl);
    }
}

public sealed class VelopackUpdateServiceTests
{
    [Fact]
    public void Constructor_Throws_WhenConfigurationIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => new VelopackUpdateService(null!));
    }

    [Fact]
    public void CurrentChannel_IsStableByDefault()
    {
        var service = CreateService();

        Assert.Equal(VelopackChannel.Stable, service.CurrentChannel);
    }

    [Fact]
    public void SetChannel_UpdatesCurrentChannel()
    {
        var service = CreateService();

        service.SetChannel(VelopackChannel.Nightly);

        Assert.Equal(VelopackChannel.Nightly, service.CurrentChannel);
    }

    [Fact]
    public void CreateManager_ConfiguresStableChannelAndDowngrade()
    {
        var service = CreateService();
        var manager = InvokeCreateManager(service);
        var source = GetGithubSource(manager);
        var channel = GetMemberValue<string?>(manager, "Channel");
        var allowDowngrade = GetMemberValue<bool>(manager, "ShouldAllowVersionDowngrade");

        Assert.True(allowDowngrade);
        Assert.Equal("stable", channel);
        Assert.False(source.Prerelease);
    }

    [Fact]
    public void CreateManager_ConfiguresNightlyChannelAndPrerelease()
    {
        var service = CreateService();
        service.SetChannel(VelopackChannel.Nightly);

        var manager = InvokeCreateManager(service);
        var source = GetGithubSource(manager);
        var channel = GetMemberValue<string?>(manager, "Channel");
        var allowDowngrade = GetMemberValue<bool>(manager, "ShouldAllowVersionDowngrade");

        Assert.True(allowDowngrade);
        Assert.Equal("nightly", channel);
        Assert.True(source.Prerelease);
    }

    [Fact]
    public async Task CheckForUpdatesAsync_HonorsCancellation()
    {
        var service = CreateService();
        var token = new CancellationToken(canceled: true);

        await Assert.ThrowsAsync<OperationCanceledException>(() => service.CheckForUpdatesAsync(token));
    }

    [Fact]
    public async Task DownloadUpdatesAsync_Throws_WhenUpdateIsNull()
    {
        var service = CreateService();

        await Assert.ThrowsAsync<ArgumentNullException>(() => service.DownloadUpdatesAsync(null!));
    }

    [Fact]
    public async Task DownloadUpdatesAsync_Throws_WhenUpdateInfoMissing()
    {
        var service = CreateService();
        var update = CreateUpdate();

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.DownloadUpdatesAsync(update));
    }

    [Fact]
    public void ApplyUpdatesAndRestart_Throws_WhenUpdateIsNull()
    {
        var service = CreateService();

        Assert.Throws<ArgumentNullException>(() => service.ApplyUpdatesAndRestart(null!));
    }

    [Fact]
    public void ApplyUpdatesAndRestart_Throws_WhenTargetReleaseMissing()
    {
        var service = CreateService();
        var update = CreateUpdate();

        Assert.Throws<InvalidOperationException>(() => service.ApplyUpdatesAndRestart(update));
    }

    [Fact]
    public async Task WaitExitThenApplyUpdatesAsync_Throws_WhenUpdateIsNull()
    {
        var service = CreateService();

        await Assert.ThrowsAsync<ArgumentNullException>(() => service.WaitExitThenApplyUpdatesAsync(null!));
    }

    [Fact]
    public async Task WaitExitThenApplyUpdatesAsync_Throws_WhenTargetReleaseMissing()
    {
        var service = CreateService();
        var update = CreateUpdate();

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.WaitExitThenApplyUpdatesAsync(update));
    }

    private static VelopackUpdateService CreateService()
    {
        EnsureTestLocator();

        var configuration = new VelopackUpdateConfiguration("https://github.com/example/repo")
        {
            AccessToken = "token"
        };

        return new VelopackUpdateService(configuration);
    }

    private static VelopackUpdateCheckResult CreateUpdate(UpdateInfo? updateInfo = null)
    {
        return new VelopackUpdateCheckResult(
            isUpdateAvailable: true,
            currentVersion: "1.0.0",
            availableVersion: "1.1.0",
            releaseNotesMarkdown: null,
            releaseNotesHtml: null,
            isDowngrade: false,
            updateInfo: updateInfo);
    }

    private static UpdateManager InvokeCreateManager(VelopackUpdateService service)
    {
        var method = typeof(VelopackUpdateService).GetMethod(
            "CreateManager",
            BindingFlags.Instance | BindingFlags.NonPublic);

        return (UpdateManager)method!.Invoke(service, null)!;
    }

    private static GithubSource GetGithubSource(UpdateManager manager)
    {
        var managerType = typeof(UpdateManager);

        var property = managerType
            .GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .FirstOrDefault(p => typeof(IUpdateSource).IsAssignableFrom(p.PropertyType));

        if (property?.GetValue(manager) is GithubSource propertyValue)
        {
            return propertyValue;
        }

        var field = managerType
            .GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .FirstOrDefault(f => typeof(IUpdateSource).IsAssignableFrom(f.FieldType));

        if (field?.GetValue(manager) is GithubSource fieldValue)
        {
            return fieldValue;
        }

        throw new InvalidOperationException("Unable to discover Velopack UpdateManager source via reflection.");
    }

    private static T GetMemberValue<T>(object instance, string memberName)
    {
        var instanceType = instance.GetType();

        var property = instanceType.GetProperty(
            memberName,
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        if (property is not null && property.PropertyType == typeof(T))
        {
            var propertyValue = property.GetValue(instance);
            return propertyValue is null ? default! : (T)propertyValue;
        }

        var backingField = instanceType.GetField(
            $"<{memberName}>k__BackingField",
            BindingFlags.Instance | BindingFlags.NonPublic);

        if (backingField is not null && backingField.FieldType == typeof(T))
        {
            var fieldValue = backingField.GetValue(instance);
            return fieldValue is null ? default! : (T)fieldValue;
        }

        throw new InvalidOperationException($"Unable to read member '{memberName}' on '{instanceType.FullName}'.");
    }

    private static void EnsureTestLocator()
    {
        if (_locatorInitialized)
        {
            return;
        }

        lock (LocatorInitLock)
        {
            if (_locatorInitialized)
            {
                return;
            }

            var packagesDir = Directory.CreateDirectory(
                Path.Combine(Path.GetTempPath(), "CCEM.Core.Velopack.Tests", Guid.NewGuid().ToString()));

            var locator = new TestVelopackLocator(
                appId: "ccem.tests",
                version: "1.0.0",
                packagesDir: packagesDir.FullName,
                logger: null);

            var setLocatorMethod = typeof(VelopackLocator).GetMethod(
                "SetCurrentLocator",
                BindingFlags.Static | BindingFlags.NonPublic);

            setLocatorMethod!.Invoke(null, new object[] { locator });
            _locatorInitialized = true;
        }
    }

    private static readonly object LocatorInitLock = new();
    private static bool _locatorInitialized;
}
