using AIS.Data;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace AIS.Services
{
    public class CountriesAPIService : ICountriesAPIService
    {
        private readonly HttpClient _httpClient;

        public CountriesAPIService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<Country>> GetCountriesAsync()
        {
            using (_httpClient)
            {
                _httpClient.BaseAddress = new Uri("https://restcountries.com");
                var response = await _httpClient.GetAsync("/v3.1/all");

                var result = await response.Content.ReadAsStringAsync();
                response.EnsureSuccessStatusCode();

                return JsonConvert.DeserializeObject<List<Country>>(result);
            }
        }
    }
}
