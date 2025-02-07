using PuppeteerSharp;

namespace YaR.Odospace.RemotePanel.HtmlRendererClient;

internal class Program
{
    private const string ConfigAddress = "127.0.0.1";
    private const ushort ConfigPort = 38_000;

    private const SupportedBrowser ConfigBrowser = SupportedBrowser.Chrome;
    private const string ConfigBrowserUrl = "http://home.loc/osd";
    private static readonly TimeSpan ConfigBrowserReloadDelay = TimeSpan.FromMinutes(1);
    private static readonly double ConfigBrowserScaleFactor = 3;
    private const bool ConfigBrowserIsLandscape = true;
    private const bool ConfigBrowserIsMobile = true;

    private const uint ConfigDisplayPageNo = 2;
    private const int ConfigDisplayDelay = 10_000;

    static async Task Main(string[] args)
    {
        var cts = new CancellationTokenSource();
        Console.CancelKeyPress += (_, e) =>
        {
            Logger.Info("Canceling...");
            cts.Cancel();
            e.Cancel = true;
        };

        var displayProvider = new RemoteDisplayProvider(ConfigAddress, ConfigPort);

        var imageProvider = new ImageProvider(ConfigBrowserUrl)
        {
            Browser = ConfigBrowser,
            RefreshRate = ConfigBrowserReloadDelay,
            ViewPortOptions = new ViewPortOptions
            {
                DeviceScaleFactor = ConfigBrowserScaleFactor,
                IsLandscape = ConfigBrowserIsLandscape,
                IsMobile = ConfigBrowserIsMobile
            }
        };
        imageProvider.OnImageLoaded = async () =>
            await displayProvider.DisplayAsync(ConfigDisplayPageNo, imageProvider.ImageBytes, cts.Token);
        var _ = imageProvider.StartAsync(cts.Token);

        while (!cts.Token.IsCancellationRequested)
        {
            await displayProvider.DisplayAsync(ConfigDisplayPageNo, imageProvider.ImageBytes, cts.Token);

            await Task.Delay(ConfigDisplayDelay, cts.Token);
        }
    }
}