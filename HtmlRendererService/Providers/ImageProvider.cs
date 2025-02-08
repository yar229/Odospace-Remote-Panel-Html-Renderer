using System.Net;
using Microsoft.Extensions.Options;
using PuppeteerSharp;
using Serilog.Events;
using SerilogTimings;
using YaR.Odospace.RemotePanel.HtmlRendererService.Configuration;

namespace YaR.Odospace.RemotePanel.HtmlRendererService.Providers;

public class ImageProvider : IDisposable
{
    private readonly ILogger<RemoteDisplayProvider> _logger;
    private readonly BrowserConfiguration _browserConfig;

    private IBrowser _browser;
    private IPage _page;
    private readonly ScreenshotOptions _screenshotOptions = new() { Type = ScreenshotType.Png, FullPage = false, OmitBackground = true, OptimizeForSpeed = true };

    public ImageProvider(ILogger<RemoteDisplayProvider> logger, IOptions<BrowserConfiguration> browserOptions)
    {
        _logger = logger;
        _browserConfig = browserOptions.Value;
    }

    public byte[] ImageBytes { get; private set; }

    public byte[] ImageStubBytes { get; private set; } = Array.Empty<byte>();

    public byte[] ImageStubErrorBytes { get; private set; } = Array.Empty<byte>();

    public Func<CancellationToken, Task>? OnImageLoaded { get; set; }

    public async Task StartAsync(CancellationToken ctx)
    {
        await InitializeAsync(ctx);

        while (!ctx.IsCancellationRequested)
        {
            try
            {
                bool gotPage ;
                using (var op = Operation.Begin("Page loading {Url}", _browserConfig.Url))
                {
                    var response = _page.Url != _browserConfig.Url
                        ? await _page.GoToAsync(_browserConfig.Url)
                        : await _page.ReloadAsync();

                    gotPage = response.Status is HttpStatusCode.OK or HttpStatusCode.NotModified;
                    op.Complete("Status", response.Status.ToString(),
                        gotPage ? LogEventLevel.Verbose : LogEventLevel.Error);
                }

                ImageBytes = gotPage
                    ? await _page.ScreenshotDataAsync(_screenshotOptions)
                    : ImageStubErrorBytes;
                var _ = OnImageLoaded?.Invoke(ctx);

                await Task.Delay(_browserConfig.ReloadDelay, ctx);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Cannot reload page {_browserConfig.Url}");
            }
        }
    }

    private async Task InitializeAsync(CancellationToken ctx)
    {
        ImageBytes = ImageStubBytes = File.Exists(_browserConfig.StubImage)
            ? await File.ReadAllBytesAsync(_browserConfig.StubImage, ctx)
            : Array.Empty<byte>();

        ImageStubErrorBytes = File.Exists(_browserConfig.StubImageError)
            ? await File.ReadAllBytesAsync(_browserConfig.StubImageError, ctx)
            : Array.Empty<byte>();

        var browserFetcher = new BrowserFetcher
        {
            Browser = _browserConfig.Browser
        };
        if (browserFetcher.GetInstalledBrowsers().All(br => br.Browser != _browserConfig.Browser))
        {
            _logger.LogInformation($"Downloading browser {_browserConfig.Browser}");
            await browserFetcher.DownloadAsync();
        }

        _logger.LogInformation($"Launching browser {_browserConfig.Browser}");
        _browser = await Puppeteer.LaunchAsync(new LaunchOptions { Headless = true, LogProcess = true, ProtocolTimeout = 10_000});
        _page = await _browser.NewPageAsync();
        _page.DefaultNavigationTimeout = 10_000;
        await _page.SetViewportAsync(new ViewPortOptions
        { 
            DeviceScaleFactor = _browserConfig.ScaleFactor,
            IsLandscape = _browserConfig.IsLandscape,
            IsMobile = _browserConfig.IsMobile
        });
    }

    public void Dispose()
    {
        _page.CloseAsync();
        _browser.CloseAsync();
    }
}