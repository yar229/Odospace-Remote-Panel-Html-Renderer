using System.Runtime.InteropServices;
using YaR.Odospace.RemotePanel.HtmlRendererClient.Aida;

namespace YaR.Odospace.RemotePanel.HtmlRendererClient;

public class RemoteDisplayProvider
{
    private readonly IntPtr _ptrAddress;

    public RemoteDisplayProvider(string address, ushort port)
    {
        Address = address;
        Port = port;
        
        _ptrAddress = Marshal.StringToHGlobalAnsi(address);
    }

    public string Address { get; init; }

    public ushort Port { get; init; }

    public async Task DisplayAsync(uint pageNo, byte[] imageBytes, CancellationToken ctx)
    {
        Logger.Info("Sending image to OSD...");
        IntPtr ptrImage = IntPtr.Zero;
        try
        {
            await Task.Run(() =>
            {
                ptrImage = Marshal.AllocHGlobal(imageBytes.Length);
                Marshal.Copy(imageBytes, 0, ptrImage, imageBytes.Length);
                AidaRDsp.SendImage(Port, _ptrAddress, pageNo, ptrImage, (uint)imageBytes.Length);
            }, ctx);
        }
        finally
        {
            Marshal.FreeHGlobal(ptrImage);
            Logger.Info("Image sent.");
        }
    }
}