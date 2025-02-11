﻿using System.Runtime.InteropServices;
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

        using (var op = Operation.Begin("Sending image to OSD ({Bytes} b)", imageBytes.Length))
        {
            var ptrImage = nint.Zero;
            try
            {
                await Task.Run(() =>
                {
                    ptrImage = Marshal.AllocHGlobal(imageBytes.Length);
                    Marshal.Copy(imageBytes, 0, ptrImage, imageBytes.Length);
                    AidaRDsp.SendImage(_odospaceServerConfig.Port, _ptrAddress, pageNo, ptrImage, (uint)imageBytes.Length);
                    op.Complete(LogEventLevel.Verbose);
                }, ctx);
            }
            catch (Exception ex)
            {
                op.Complete(LogEventLevel.Error);
                _logger.LogError(ex, "Error sending image to OSD server");
            }
            finally
            {
                Marshal.FreeHGlobal(ptrImage);
            }
        }
    }
}