using System.Runtime.InteropServices;
using Microsoft.Extensions.Options;
using Serilog.Events;
using SerilogTimings;
using YaR.Odospace.RemotePanel.HtmlRendererService.Aida;
using YaR.Odospace.RemotePanel.HtmlRendererService.Configuration;

namespace YaR.Odospace.RemotePanel.HtmlRendererService.Providers;

public class RemoteDisplayProvider
{
    private readonly ILogger<RemoteDisplayProvider> _logger;
    private readonly OdospaceServerConfiguration _odospaceServerConfig;
    private readonly nint _ptrAddress;

    public RemoteDisplayProvider(ILogger<RemoteDisplayProvider> logger, IOptions<OdospaceServerConfiguration> odospaceServerOptions)
    {
        _logger = logger;
        _odospaceServerConfig = odospaceServerOptions.Value;

        _ptrAddress = Marshal.StringToHGlobalAnsi(_odospaceServerConfig.Address);
    }

    public async Task DisplayAsync(uint pageNo, byte[]? imageBytes, CancellationToken ctx)
    {
        if (null == imageBytes)
            return;

        using (var oper = Operation.Begin("Sending image to OSD"))
        {
            var ptrImage = nint.Zero;
            try
            {
                await Task.Run(() =>
                {
                    ptrImage = Marshal.AllocHGlobal(imageBytes.Length);
                    Marshal.Copy(imageBytes, 0, ptrImage, imageBytes.Length);
                    AidaRDsp.SendImage(_odospaceServerConfig.Port, _ptrAddress, pageNo, ptrImage, (uint)imageBytes.Length);
                    oper
                        .EnrichWith("Bytes", imageBytes.Length)
                        .Complete(LogEventLevel.Verbose);
                }, ctx);
            }
            catch (Exception ex)
            {
                oper
                    .EnrichWith("Exception", ex)
                    .Complete(LogEventLevel.Error);
            }
            finally
            {
                Marshal.FreeHGlobal(ptrImage);
            }
        }
    }
}