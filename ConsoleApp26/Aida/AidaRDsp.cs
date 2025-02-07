using System.Runtime.InteropServices;

namespace YaR.Odospace.RemotePanel.HtmlRendererClient.Aida;

public static class AidaRDsp
{
    // send_image(USHORT usPort, char * pszAddress, ULONG ulImageId, void * pImage, ULONG ulImageSize)
    [DllImport("aidardsp.dll", EntryPoint = "send_image")]
    public static extern int SendImage(ushort port, nint address, uint imageId, nint imageContent, uint imageSize);
}