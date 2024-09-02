using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace AIS.Helpers
{
    public interface IImageHelper
    {
        Task<string> UploadImageAsync(IFormFile imageFile, string model, string folder);

        void DeleteImage(string imageUrl);
    }
}
