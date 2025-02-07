namespace YaR.Odospace.RemotePanel.HtmlRendererService.Configuration;

public class DisplayConfiguration
{
    public const string SectionName = "Display";

    public uint PageNo { get; set; } = 2;
    public int Delay { get; set; } = 10_000;
}