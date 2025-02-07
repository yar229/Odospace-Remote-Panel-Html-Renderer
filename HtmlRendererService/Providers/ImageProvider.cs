using Microsoft.Extensions.Options;
using PuppeteerSharp;
using YaR.Odospace.RemotePanel.HtmlRendererService.Configuration;

namespace YaR.Odospace.RemotePanel.HtmlRendererService.Providers;

public class ImageProvider
{
    private readonly ILogger<RemoteDisplayProvider> _logger;
    private readonly BrowserConfiguration _browserConfig;

    public ImageProvider(ILogger<RemoteDisplayProvider> logger, IOptions<BrowserConfiguration> browserOptions)
    {
        _logger = logger;
        _browserConfig = browserOptions.Value;

        ImageBytes = File.Exists(_browserConfig.StubImage)
            ? File.ReadAllBytes(_browserConfig.StubImage)
            :Array.Empty<byte>();
    }

    public byte[] ImageBytes { get; private set; }

    public Func<CancellationToken, Task>? OnImageLoaded { get; set; }

    public async Task StartAsync(CancellationToken ctx)
    {
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
        await using var browser = await Puppeteer.LaunchAsync(new LaunchOptions { Headless = true });
        await using var page = await browser.NewPageAsync();
        await page.SetViewportAsync(new ViewPortOptions
        {
            DeviceScaleFactor = _browserConfig.ScaleFactor,
            IsLandscape = _browserConfig.IsLandscape,
            IsMobile = _browserConfig.IsMobile
        });
        await page.GoToAsync(_browserConfig.Url);

        var screenshotOptions = new ScreenshotOptions { Type = ScreenshotType.Png, FullPage = false, OmitBackground = true, OptimizeForSpeed = true };
        try
        {
            while (!ctx.IsCancellationRequested)
            {
                _logger.LogInformation($"Reloading page {page.Url}");

                await page.ReloadAsync();
                ImageBytes = await page.ScreenshotDataAsync(screenshotOptions);
                var _ = OnImageLoaded?.Invoke(ctx);
                await Task.Delay(_browserConfig.ReloadDelay, ctx);
            }
        }
        finally
        {
            await page.CloseAsync();
            await browser.CloseAsync();
        }
    }
}