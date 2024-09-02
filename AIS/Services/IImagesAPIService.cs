using System.Collections.Generic;
using System.Threading.Tasks;

namespace AIS.Services
{
    public interface IImagesAPIService
    {
        Task<List<string>> GetCountryImageUrl(string country);
    }
}
