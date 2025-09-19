using QRCoder;

namespace Kont.backend.Services;

public interface IQrCodeService
{
    byte[] GeneratePng(string contentUrl, int pixelsPerModule = 8);
    string GenerateBase64Png(string contentUrl, int pixelsPerModule = 8);
}

public class QrCodeService : IQrCodeService
{
    public byte[] GeneratePng(string contentUrl, int pixelsPerModule = 8)
    {
        using var qrGenerator = new QRCodeGenerator();
        using var qrCodeData = qrGenerator.CreateQrCode(contentUrl, QRCodeGenerator.ECCLevel.Q);
        using var qrCode = new PngByteQRCode(qrCodeData);
        
        return qrCode.GetGraphic(pixelsPerModule);
    }

    public string GenerateBase64Png(string contentUrl, int pixelsPerModule = 8)
    {
        var pngBytes = GeneratePng(contentUrl, pixelsPerModule);
        return Convert.ToBase64String(pngBytes);
    }
}
