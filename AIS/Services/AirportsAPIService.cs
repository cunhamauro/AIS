using AIS.Data.Classes;
using AIS.Data.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace AIS.Services
{
    public class AirportsAPIService : IAirportsAPIService
    {
        public async Task<List<Airport>> GetAirportsAsync()
        {
            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = await client.GetAsync("https://gist.githubusercontent.com/tdreyno/4278655/raw/7b0762c09b519f40397e4c3e100b097d861f5588/airports.json");
                response.EnsureSuccessStatusCode();
                string json = await response.Content.ReadAsStringAsync();

                // ApiAirport properties: code, state, country
                // Airport properties: IATA, City, Country
                List<AirportAPI> apiAirport = new List<AirportAPI>();
                apiAirport = JsonSerializer.Deserialize<List<AirportAPI>>(json);

                List<Airport> convertedAirports = new List<Airport>();

                // Convert from class ApiAirport to Airport
                convertedAirports = apiAirport.Where(apiAirport => !string.IsNullOrWhiteSpace(apiAirport.state) && !string.IsNullOrWhiteSpace(apiAirport.country)) // Some of the APIs airports come with empty states, etc
                .Select(apiAirport => new Airport
                {
                    IATA = apiAirport.code,
                    Country = apiAirport.country,
                    City = apiAirport.state
                }).ToList();

                return convertedAirports;
            }
        }
    }
}
