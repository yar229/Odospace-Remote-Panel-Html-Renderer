using YaR.Odospace.RemotePanel.HtmlRendererService.Configuration;
using YaR.Odospace.RemotePanel.HtmlRendererService.Providers;

namespace YaR.Odospace.RemotePanel.HtmlRendererService;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);

        builder.Services.Configure<BrowserConfiguration>(builder.Configuration.GetSection(BrowserConfiguration.SectionName));
        builder.Services.Configure<OdospaceServerConfiguration>(builder.Configuration.GetSection(OdospaceServerConfiguration.SectionName));
        builder.Services.Configure<DisplayConfiguration>(builder.Configuration.GetSection(DisplayConfiguration.SectionName));

        builder.Services.AddSingleton<ImageProvider>();
        builder.Services.AddSingleton<RemoteDisplayProvider>();
        builder.Services.AddHostedService<Worker>();

        var host = builder.Build();
        host.Run();
    }
}