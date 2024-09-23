using System.IO;

namespace AIS.Helpers
{
    public interface IQrCodeHelper
    {
        MemoryStream GenerateQrCode(string qrContent);
    }
}
