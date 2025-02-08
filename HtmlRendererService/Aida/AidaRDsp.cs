using System.Runtime.InteropServices;

namespace YaR.Odospace.RemotePanel.HtmlRendererService.Aida;

public static class AidaRDsp
{
    /// <summary>
    ///  send_image(USHORT usPort, char * pszAddress, ULONG ulImageId, void * pImage, ULONG ulImageSize)
    /// </summary>
    [DllImport("aidardsp.dll", EntryPoint = "send_image")]
    public static extern uint SendImage(ushort port, nint address, uint imageId, nint imageContent, uint imageSize);
}