using Gotorz.DTOs;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace Gotorz.Services
{
    public class FlightApiService
    {

        private readonly HttpClient _httpClient;
        private const string _apiKey =  "ad6e28e5e89e0929053fdaf5c5b418cda8a295b899b67c5e5d9ac4707d2d0c13";

        public FlightApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }
        public async Task<FlightResponseDto?> SearchFlightsAsync(string from, string to, DateTime departureDate, DateTime returnDate, string currency)
        {
            var url = $"https://serpapi.com/search.json" +
    $"?engine=google_flights" +
    $"&departure_id={from.ToUpperInvariant()}" +
    $"&arrival_id={to.ToUpperInvariant()}" +
    $"&outbound_date={departureDate:yyyy-MM-dd}" +
    $"&return_date={returnDate:yyyy-MM-dd}" +
    $"&currency={currency.ToUpperInvariant()}" +
    $"&hl=en" +
    $"&api_key={_apiKey}";




            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
                return null;

            var json = await response.Content.ReadAsStringAsync();
            var data = JsonSerializer.Deserialize<FlightResponseDto>(json);
            Console.WriteLine(json); // ← se hele JSON-svaret fra API
            Console.WriteLine(data?.OtherFlights?.Count); // ← se om noget blev mappet


            return data;
        }

    }
}
