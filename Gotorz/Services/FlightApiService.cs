using System.Text.Json;
using Gotorz.DTOs;
using Microsoft.Extensions.Options;
using Gotorz.Config; // ← hvor din settings-klasse ligger

namespace Gotorz.Services
{
    public class FlightApiService
    {
        private readonly HttpClient _httpClient;
        private readonly FlightApiSettings _settings;

        public FlightApiService(HttpClient httpClient, IOptions<FlightApiSettings> options)
        {
            _httpClient = httpClient;
            _settings = options.Value;
        }

        public async Task<FlightResponseDto?> SearchFlightsAsync(
            string from, string to,
            DateTime departureDate, DateTime returnDate,
            string currency)
        {
            var url = $"{_settings.BaseUrl}" +
                      $"?engine=google_flights" +
                      $"&departure_id={from.ToUpperInvariant()}" +
                      $"&arrival_id={to.ToUpperInvariant()}" +
                      $"&outbound_date={departureDate:yyyy-MM-dd}" +
                      $"&return_date={returnDate:yyyy-MM-dd}" +
                      $"&currency={currency.ToUpperInvariant()}" +
                      $"&hl=en" +
                      $"&api_key={_settings.ApiKey}";

            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
                return null;

            var json = await response.Content.ReadAsStringAsync();
            var data = JsonSerializer.Deserialize<FlightResponseDto>(json);

            Console.WriteLine(json); // Debug: Se hele svaret
            Console.WriteLine(data?.OtherFlights?.Count); // Debug: Se om noget blev mappet

            return data;
        }
    }
}

