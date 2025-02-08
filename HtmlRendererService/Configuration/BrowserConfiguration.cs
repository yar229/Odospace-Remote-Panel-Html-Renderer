using PuppeteerSharp;

namespace YaR.Odospace.RemotePanel.HtmlRendererService.Configuration;

public class BrowserConfiguration
{
    public const string SectionName = "Browser";

    public SupportedBrowser Browser { get; set; } = SupportedBrowser.Chrome;
    public string Url { get; set; } = "http://home.loc/osd";
    public TimeSpan ReloadDelay { get; set; } = TimeSpan.FromMinutes(1);
    public double ScaleFactor { get; set; } = 3;
    public bool IsLandscape { get; set; } = true;
    public bool IsMobile { get; set; } = true;
    public string StubImage { get; set; }
    public string StubImageError { get; set; }
}