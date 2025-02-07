namespace YaR.Odospace.RemotePanel.HtmlRendererService.Configuration;

public class OdospaceServerConfiguration
{
    public const string SectionName = "OdospaceServer";

    public string Address { get; set; } = "127.0.0.1";
    public ushort Port { get; set; } = 38_000;
}