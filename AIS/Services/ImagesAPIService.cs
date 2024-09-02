using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace AIS.Services
{
    public class ImagesAPIService : IImagesAPIService
    {
        private readonly IConfiguration _configuration;
        private readonly HttpClient _client;

        public ImagesAPIService(IConfiguration configuration, HttpClient client)
        {
            _configuration = configuration;
            _client = client;
        }

        public async Task<List<string>> GetCountryImageUrl(string country)
        {
            List<string> imageUrls = new List<string>();
            try
            {
                string query = $"{country}+nature";
                string url = $"https://www.googleapis.com/customsearch/v1?q={query}&cx={_configuration["GoogleSearch:SearchEngineId"]}&searchType=image&key={_configuration["GoogleSearch:ApiKey"]}";

                var response = await _client.GetStringAsync(url);
                var jsonResponse = JObject.Parse(response);
                imageUrls = jsonResponse["items"]?.Select(i => i["link"]?.ToString()).ToList();

                if (imageUrls == null || !imageUrls.Any())
                {
                    return imageUrls;
                }

                return imageUrls;
            }
            catch (Exception)
            {
                return imageUrls;
            }
        }
    }
}
