using Gotorz.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Gotorz.Services
{
    public class FlightService : IFlightService
    {
        private readonly IHttpClientFactory _factory;
        private readonly ITokenService _tokenService;
        private readonly ILogger<FlightService> _logger;

        public FlightService(IHttpClientFactory factory, ITokenService tokenService, ILogger<FlightService> logger)
        {
            _factory = factory;
            _tokenService = tokenService;
            _logger = logger;
        }

        public async Task<List<FlightInfo>> SearchFlightsAsync(string origin, string destination, DateTime departureDate)
        {
            // If either origin or destination is empty, return mock data
            if (string.IsNullOrEmpty(origin) || string.IsNullOrEmpty(destination))
            {
                _logger.LogWarning("SearchFlightsAsync called with empty origin or destination. Using mock data.");
                return CreateMockFlights(origin ?? "CPH", destination ?? "BCN", departureDate);
            }

            try
            {
                _logger.LogInformation($"Getting token for Amadeus API");
                var token = await _tokenService.GetTokenAsync();
                var client = _factory.CreateClient("AmadeusAPI");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                // Format the URL with the correct parameters
                var url = $"/v2/shopping/flight-offers?originLocationCode={origin}&destinationLocationCode={destination}&departureDate={departureDate:yyyy-MM-dd}&adults=1&max=5";
                _logger.LogInformation($"Calling Amadeus API: {url}");

                var response = await client.GetAsync(url);

                // Log the response status code
                _logger.LogInformation($"Amadeus API response status: {response.StatusCode}");

                if (!response.IsSuccessStatusCode)
                {
                    string errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"Amadeus API error: {errorContent}");
                    return CreateMockFlights(origin, destination, departureDate);
                }

                var content = await response.Content.ReadAsStringAsync();
                _logger.LogInformation($"Amadeus API response received, length: {content.Length}");

                try
                {
                    var root = JObject.Parse(content);
                    var offers = root["data"] as JArray;

                    if (offers == null || !offers.Any())
                    {
                        _logger.LogWarning("No flight offers found in API response");
                        return CreateMockFlights(origin, destination, departureDate);
                    }

                    _logger.LogInformation($"Found {offers.Count} flight offers");

                    // Parse the flight offers into FlightInfo objects
                    var results = new List<FlightInfo>();

                    for (int i = 0; i < offers.Count; i++)
                    {
                        try
                        {
                            var flightInfo = new FlightInfo
                            {
                                Id = i,
                                FlightNumber = GetNestedString(offers[i], "itineraries", 0, "segments", 0, "carrierCode") +
                                               GetNestedString(offers[i], "itineraries", 0, "segments", 0, "number"),
                                Airline = GetNestedString(offers[i], "itineraries", 0, "segments", 0, "carrierCode"),
                                DepartureAirport = GetNestedString(offers[i], "itineraries", 0, "segments", 0, "departure", "iataCode"),
                                ArrivalAirport = GetNestedString(offers[i], "itineraries", 0, "segments", 0, "arrival", "iataCode"),
                                DepartureTime = ParseDateTime(GetNestedString(offers[i], "itineraries", 0, "segments", 0, "departure", "at")),
                                ArrivalTime = ParseDateTime(GetNestedString(offers[i], "itineraries", 0, "segments", 0, "arrival", "at")),
                                Price = ParseDecimal(GetNestedString(offers[i], "price", "total"))
                            };

                            results.Add(flightInfo);
                            _logger.LogInformation($"Parsed flight {flightInfo.FlightNumber} successfully");
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError($"Error parsing flight offer {i}: {ex.Message}");
                        }
                    }

                    if (results.Any())
                    {
                        return results;
                    }
                    else
                    {
                        _logger.LogWarning("Could not parse any flight offers, using mock data");
                        return CreateMockFlights(origin, destination, departureDate);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error parsing API response: {ex.Message}");
                    return CreateMockFlights(origin, destination, departureDate);
                }
            }
            catch (Exception ex)
            {
                // Log the exception
                _logger.LogError($"Error searching flights: {ex.Message}");

                // Return mock data for demonstration purposes
                return CreateMockFlights(origin, destination, departureDate);
            }
        }

        // Helper methods for safer JSON parsing
        private string GetNestedString(JToken token, params object[] path)
        {
            try
            {
                JToken current = token;

                foreach (var segment in path)
                {
                    if (segment is string propertyName)
                    {
                        current = current[propertyName];
                    }
                    else if (segment is int index)
                    {
                        current = current[index];
                    }

                    if (current == null)
                    {
                        return string.Empty;
                    }
                }

                return current.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting nested string: {ex.Message}");
                return string.Empty;
            }
        }

        private DateTime ParseDateTime(string dateString)
        {
            if (DateTime.TryParse(dateString, out var result))
            {
                return result;
            }
            return DateTime.Now;
        }

        private decimal ParseDecimal(string decimalString)
        {
            if (decimal.TryParse(decimalString, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var result))
            {
                return result;
            }
            return 0;
        }


        public Task<FlightInfo> GetFlightDetailAsync(string flightId)
        {
            // This method is not implemented in the current code
            // You could implement it later if needed
            throw new NotImplementedException();
        }

        // Create mock flight data for testing when API fails
        private List<FlightInfo> CreateMockFlights(string origin, string destination, DateTime departureDate)
        {
            _logger.LogInformation("Creating mock flight data");
            var random = new Random();
            var flights = new List<FlightInfo>();

            string[] airlines = { "SAS", "Norwegian", "Lufthansa", "KLM", "Air France" };

            // Create 3-5 mock flights
            for (int i = 0; i < random.Next(3, 6); i++)
            {
                var airline = airlines[random.Next(airlines.Length)];
                var flightNumber = $"{airline}{100 + random.Next(900)}";
                var departureTime = departureDate.AddHours(7 + random.Next(12)); // Between 7AM and 7PM
                var flightDuration = TimeSpan.FromHours(2 + random.Next(5)); // 2-7 hours

                flights.Add(new FlightInfo
                {
                    Id = i,
                    FlightNumber = flightNumber,
                    Airline = airline,
                    DepartureAirport = origin,
                    ArrivalAirport = destination,
                    DepartureTime = departureTime,
                    ArrivalTime = departureTime + flightDuration,
                    Price = 100 + (decimal)(random.NextDouble() * 900) // $100-$1000
                });
            }

            _logger.LogInformation($"Created {flights.Count} mock flights");
            return flights;
        }
    }
}