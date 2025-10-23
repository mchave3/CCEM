using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using CCEM.Core.Velopack.Models;
using CCEM.Core.Velopack.Services;
using CCEM.Core.Velopack.Tests.TestInfrastructure;
using NuGet.Versioning;
using Velopack;
using Velopack.Logging;
using Velopack.Locators;
using Velopack.Sources;

namespace CCEM.Core.Velopack.Tests.Integration;

public sealed class VelopackUpdateServiceIntegrationTests : IDisposable
{
    private readonly string _packagesDir;
    private readonly TestVelopackLocator _locator;
    private readonly FakeUpdateSource _updateSource;
    private readonly VelopackUpdateService _service;

    public VelopackUpdateServiceIntegrationTests()
    {
        _packagesDir = Path.Combine(
            Path.GetTempPath(),
            "CCEM.Core.Velopack.Tests",
            "Integration",
            Guid.NewGuid().ToString("N"));

        _locator = VelopackTestContext.ConfigureTestLocator("1.0.0", _packagesDir);
        _updateSource = new FakeUpdateSource();
        _service = CreateService();
    }

    [Fact]
    public async Task CheckForUpdatesAsync_ReturnsExpectedUpdateMetadata()
    {
        var result = await _service.CheckForUpdatesAsync();

        Assert.True(result.IsUpdateAvailable);
        Assert.Equal("1.0.0", result.CurrentVersion);
        Assert.Equal("1.1.0", result.AvailableVersion);
        Assert.Equal(_updateSource.ReleaseAsset.NotesMarkdown, result.ReleaseNotesMarkdown);
        Assert.Equal(_updateSource.ReleaseAsset.NotesHTML, result.ReleaseNotesHtml);
        Assert.Equal("ccem.tests", _updateSource.LastAppId);
        Assert.Equal("stable", _updateSource.LastChannel);
        Assert.False(result.IsDowngrade);
    }

    [Fact]
    public async Task DownloadUpdatesAsync_DownloadsPackageAndReportsProgress()
    {
        var update = await _service.CheckForUpdatesAsync();
        var observedProgress = new List<int>();

        await _service.DownloadUpdatesAsync(update, observedProgress.Add);

        Assert.Contains(100, observedProgress);
        Assert.Equal(1, _updateSource.DownloadInvocations);
        Assert.NotNull(_updateSource.LastDownloadPath);
        var downloadedFiles = Directory.EnumerateFiles(_packagesDir, "*.nupkg*", SearchOption.AllDirectories).ToList();
        Assert.NotEmpty(downloadedFiles);

        var finalPackage = downloadedFiles.FirstOrDefault(path =>
            string.Equals(
                Path.GetFileName(path),
                _updateSource.ReleaseAsset.FileName,
                StringComparison.OrdinalIgnoreCase)) ?? downloadedFiles.First();

        Assert.True(new FileInfo(finalPackage).Length > 0);
    }

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(_packagesDir))
            {
                Directory.Delete(_packagesDir, recursive: true);
            }
        }
        catch
        {
            // Ignore cleanup failures in tests.
        }
    }

    private VelopackUpdateService CreateService()
    {
        var configuration = new VelopackUpdateConfiguration("https://github.com/example/repo")
        {
            AccessToken = "token"
        };

        return new VelopackUpdateService(
            configuration,
            (_, channel) =>
            {
                var options = new UpdateOptions
                {
                    ExplicitChannel = channel switch
                    {
                        VelopackChannel.Stable => "stable",
                        VelopackChannel.Nightly => "nightly",
                        _ => null
                    },
                    AllowVersionDowngrade = true
                };

                return new UpdateManager(_updateSource, options, locator: _locator);
            });
    }

    private sealed class FakeUpdateSource : IUpdateSource
    {
        private readonly VelopackAssetFeed _feed;
        private readonly byte[] _dummyContent;

        public FakeUpdateSource()
        {
            _dummyContent = CreateDummyPayload();
            var sha256 = Convert.ToHexString(SHA256.HashData(_dummyContent));
            var sha1 = Convert.ToHexString(SHA1.HashData(_dummyContent));

            ReleaseAsset = new VelopackAsset
            {
                PackageId = "ccem.tests",
                Version = SemanticVersion.Parse("1.1.0"),
                Type = VelopackAssetType.Full,
                FileName = "ccem.tests-1.1.0-full.nupkg",
                SHA1 = sha1,
                SHA256 = sha256,
                Size = _dummyContent.Length,
                NotesMarkdown = "## Changelog",
                NotesHTML = "<h2>Changelog</h2>"
            };

            _feed = new VelopackAssetFeed
            {
                Assets = new[] { ReleaseAsset }
            };
        }

        public VelopackAsset ReleaseAsset { get; }
        public string? LastAppId { get; private set; }
        public string? LastChannel { get; private set; }
        public int DownloadInvocations { get; private set; }
        public string? LastDownloadPath { get; private set; }

        public Task<VelopackAssetFeed> GetReleaseFeed(
            IVelopackLogger logger,
            string? appId,
            string channel,
            Guid? stagingId = null,
            VelopackAsset? latestLocalRelease = null!)
        {
            LastAppId = appId;
            LastChannel = channel;
            return Task.FromResult(_feed);
        }

        public Task DownloadReleaseEntry(
            IVelopackLogger logger,
            VelopackAsset asset,
            string targetFile,
            Action<int>? progress,
            CancellationToken cancellationToken)
        {
            DownloadInvocations++;
            LastDownloadPath = targetFile;

            Directory.CreateDirectory(Path.GetDirectoryName(targetFile)!);

            progress?.Invoke(25);
            cancellationToken.ThrowIfCancellationRequested();

            File.WriteAllBytes(targetFile, _dummyContent);

            progress?.Invoke(100);
            return Task.CompletedTask;
        }

        private static byte[] CreateDummyPayload()
        {
            var buffer = new byte[256];
            RandomNumberGenerator.Fill(buffer);
            return buffer;
        }
    }
}
