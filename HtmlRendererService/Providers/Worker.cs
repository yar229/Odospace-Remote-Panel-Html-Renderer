using Microsoft.Extensions.Options;
using YaR.Odospace.RemotePanel.HtmlRendererService.Configuration;

namespace YaR.Odospace.RemotePanel.HtmlRendererService.Providers;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly ImageProvider _imageProvider;
    private readonly RemoteDisplayProvider _displayProvider;

    private readonly DisplayConfiguration _displayConfig;

    public Worker(ILogger<Worker> logger,
        ImageProvider imageProvider,
        RemoteDisplayProvider displayProvider,
        IOptions<DisplayConfiguration> displayOptions)
    {
        _logger = logger;
        _imageProvider = imageProvider;
        _displayProvider = displayProvider;
        _displayConfig = displayOptions.Value;

        _imageProvider.OnImageLoaded = async ctx =>
            await displayProvider.DisplayAsync(_displayConfig.PageNo, imageProvider.ImageBytes, ctx);
    }

    protected override async Task ExecuteAsync(CancellationToken ctx)
    {
        var _ = _imageProvider.StartAsync(ctx);

        while (!ctx.IsCancellationRequested)
        {
            await _displayProvider.DisplayAsync(_displayConfig.PageNo, _imageProvider.ImageBytes, ctx);

            await Task.Delay(_displayConfig.Delay, ctx);
        }
    }
}