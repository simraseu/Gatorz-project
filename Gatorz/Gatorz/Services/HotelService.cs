using Gatorz.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Gatorz.Services
{
    public class HotelService : IHotelService
    {
        private readonly IHttpClientFactory _factory;
        private readonly ITokenService _tokenService;
        private readonly ILogger<HotelService> _logger;

        public HotelService(IHttpClientFactory factory, ITokenService tokenService, ILogger<HotelService> logger)
        {
            _factory = factory;
            _tokenService = tokenService;
            _logger = logger;
        }

        public async Task<List<HotelInfo>> SearchHotelsAsync(string cityCode, DateTime checkIn, DateTime checkOut)
        {
            // If cityCode is empty, return mock data
            if (string.IsNullOrEmpty(cityCode))
            {
                _logger.LogWarning("SearchHotelsAsync called with empty cityCode. Using mock data.");
                return CreateMockHotels(cityCode ?? "BCN", checkIn, checkOut);
            }

            try
            {
                _logger.LogInformation($"Getting token for Amadeus API");
                var token = await _tokenService.GetTokenAsync();
                var client = _factory.CreateClient("AmadeusAPI");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                // Format the URL with the correct parameters
                var url = $"/v2/shopping/hotel-offers?cityCode={cityCode}&checkInDate={checkIn:yyyy-MM-dd}&checkOutDate={checkOut:yyyy-MM-dd}&adults=1&roomQuantity=1";
                _logger.LogInformation($"Calling Amadeus API: {url}");

                var response = await client.GetAsync(url);

                // Log the response status code
                _logger.LogInformation($"Amadeus API response status: {response.StatusCode}");

                if (!response.IsSuccessStatusCode)
                {
                    string errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"Amadeus API error: {errorContent}");
                    return CreateMockHotels(cityCode, checkIn, checkOut);
                }

                var content = await response.Content.ReadAsStringAsync();
                _logger.LogInformation($"Amadeus API response received, length: {content.Length}");

                try
                {
                    var root = JObject.Parse(content);
                    var offers = root["data"] as JArray;

                    if (offers == null || !offers.Any())
                    {
                        _logger.LogWarning("No hotel offers found in API response");
                        return CreateMockHotels(cityCode, checkIn, checkOut);
                    }

                    _logger.LogInformation($"Found {offers.Count} hotel offers");

                    // Parse the hotel offers into HotelInfo objects
                    var results = new List<HotelInfo>();

                    for (int i = 0; i < offers.Count; i++)
                    {
                        try
                        {
                            var hotelInfo = new HotelInfo
                            {
                                Id = int.Parse(GetNestedString(offers[i], "hotel", "hotelId") ?? i.ToString()),
                                HotelName = GetNestedString(offers[i], "hotel", "name"),
                                Address = GetAddressLine(offers[i]),
                                City = GetNestedString(offers[i], "hotel", "address", "cityName"),
                                Country = GetNestedString(offers[i], "hotel", "address", "countryCode"),
                                StarRating = ParseInt(GetNestedString(offers[i], "hotel", "rating")),
                                CheckInDate = checkIn,
                                CheckOutDate = checkOut,
                                RoomType = GetNestedString(offers[i], "offers", 0, "room", "typeEstimated", "category")
                                    ?? GetNestedString(offers[i], "offers", 0, "room", "type")
                                    ?? "Standard Room",
                                PricePerNight = ParseDecimal(GetNestedString(offers[i], "offers", 0, "price", "base"))
                            };

                            results.Add(hotelInfo);
                            _logger.LogInformation($"Parsed hotel {hotelInfo.HotelName} successfully");
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError($"Error parsing hotel offer {i}: {ex.Message}");
                        }
                    }

                    if (results.Any())
                    {
                        return results;
                    }
                    else
                    {
                        _logger.LogWarning("Could not parse any hotel offers, using mock data");
                        return CreateMockHotels(cityCode, checkIn, checkOut);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error parsing API response: {ex.Message}");
                    return CreateMockHotels(cityCode, checkIn, checkOut);
                }
            }
            catch (Exception ex)
            {
                // Log the exception
                _logger.LogError($"Error searching hotels: {ex.Message}");

                // Return mock data for demonstration purposes
                return CreateMockHotels(cityCode, checkIn, checkOut);
            }
        }

        private string GetAddressLine(JToken hotelOffer)
        {
            try
            {
                var lines = hotelOffer["hotel"]?["address"]?["lines"] as JArray;
                if (lines != null && lines.Count > 0)
                {
                    return lines[0].ToString();
                }
                return string.Empty;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting address line: {ex.Message}");
                return string.Empty;
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

        private int ParseInt(string intString)
        {
            if (int.TryParse(intString, out var result))
            {
                return result;
            }
            return 0;
        }

        private decimal ParseDecimal(string decimalString)
        {
            if (decimal.TryParse(decimalString, out var result))
            {
                return result;
            }
            return 0;
        }

        public Task<HotelInfo> GetHotelDetailAsync(string hotelId)
        {
            // This method is not implemented in the current code
            // You could implement it later if needed
            throw new NotImplementedException();
        }

        // Create mock hotel data for testing when API fails
        private List<HotelInfo> CreateMockHotels(string cityCode, DateTime checkIn, DateTime checkOut)
        {
            _logger.LogInformation("Creating mock hotel data");
            var random = new Random();
            var hotels = new List<HotelInfo>();

            // Define city names and country codes mapping
            var cityMapping = new Dictionary<string, (string City, string Country)>
            {
                { "BCN", ("Barcelona", "ES") },
                { "FCO", ("Rome", "IT") },
                { "CDG", ("Paris", "FR") },
                { "LHR", ("London", "UK") },
                { "CPH", ("Copenhagen", "DK") },
                { "DXB", ("Dubai", "AE") }
            };

            // Get city name and country code or use defaults if not found
            var (cityName, countryCode) = cityMapping.TryGetValue(cityCode, out var cityInfo)
                ? cityInfo
                : ("Unknown City", "XX");

            string[] hotelNames = {
                $"Grand Hotel {cityName}",
                $"{cityName} Plaza",
                $"Royal {cityName}",
                $"{cityName} Hilton",
                $"Luxury {cityName}"
            };

            string[] roomTypes = { "Standard Room", "Double Room", "Superior Room", "Deluxe Room", "Suite" };

            // Create 3-5 mock hotels
            for (int i = 0; i < random.Next(3, 6); i++)
            {
                var hotelName = hotelNames[random.Next(hotelNames.Length)];
                var roomType = roomTypes[random.Next(roomTypes.Length)];
                var starRating = random.Next(3, 6); // 3-5 stars
                var pricePerNight = 50 + (decimal)(random.NextDouble() * 300); // $50-$350 per night

                hotels.Add(new HotelInfo
                {
                    Id = i,
                    HotelName = hotelName,
                    Address = $"{random.Next(1, 200)} Main Street",
                    City = cityName,
                    Country = countryCode,
                    StarRating = starRating,
                    CheckInDate = checkIn,
                    CheckOutDate = checkOut,
                    RoomType = roomType,
                    PricePerNight = pricePerNight
                });
            }

            _logger.LogInformation($"Created {hotels.Count} mock hotels");
            return hotels;
        }
    }
}