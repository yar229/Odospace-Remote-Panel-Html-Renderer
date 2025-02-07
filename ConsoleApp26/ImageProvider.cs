using YaR.Odospace.RemotePanel.HtmlRendererClient;
using PuppeteerSharp;

namespace YaR.Odospace.RemotePanel.HtmlRendererClient;

public class ImageProvider
{
    public ImageProvider(string url, byte[]? stubImageBytes = null)
    {
        Url = url;

        ImageBytes = stubImageBytes 
            ?? Properties.Resources.VaultBoy
            ?? Array.Empty<byte>();
    }

    public string Url { get; init; }

    public SupportedBrowser Browser { get; init; } = SupportedBrowser.Chrome;

    public byte[] ImageBytes { get; private set; }

    public TimeSpan RefreshRate { get; set; } = TimeSpan.FromMinutes(1);

    public ViewPortOptions ViewPortOptions { get; init; }

    public Func<Task> OnImageLoaded { get; set; }

    public async Task StartAsync(CancellationToken ctx)
    {
        var browserFetcher = new BrowserFetcher
        {
            Browser = Browser
        };
        if (browserFetcher.GetInstalledBrowsers().All(br => br.Browser != Browser))
        {
            Logger.Info($"Downloading browser {Browser}");
            await browserFetcher.DownloadAsync();
        }

        Logger.Info($"Launching browser {Browser}");
        await using var browser = await Puppeteer.LaunchAsync(new LaunchOptions { Headless = true });
        await using var page = await browser.NewPageAsync();
        await page.SetViewportAsync(ViewPortOptions);
        await page.GoToAsync(Url);

        var screenshotOptions = new ScreenshotOptions { Type = ScreenshotType.Png, FullPage = false, OmitBackground = true, OptimizeForSpeed = true };
        while (!ctx.IsCancellationRequested)
        {
            Logger.Info($"Reloading page {page.Url}");

            try
            {
                await page.ReloadAsync();
                ImageBytes = await page.ScreenshotDataAsync(screenshotOptions);
                await OnImageLoaded.Invoke();
                await Task.Delay(RefreshRate, ctx);
            }
            catch (Exception e)
            {
                Logger.Error("Error getting image", e);
            }
        }

        await page.CloseAsync();
        await browser.CloseAsync();
    }
}