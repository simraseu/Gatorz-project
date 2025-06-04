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
    public class HotelService : IHotelService
    {
        private readonly IHttpClientFactory _factory;
        private readonly ITokenService _tokenService;
        private readonly ILogger<HotelService> _logger;

        // Airport code to city code mapping
        private readonly Dictionary<string, string> _airportToCityCode = new Dictionary<string, string>
        {
            // European airports
            { "CPH", "CPH" }, // Copenhagen
            { "BCN", "BCN" }, // Barcelona
            { "MAD", "MAD" }, // Madrid
            { "FCO", "ROM" }, // Rome Fiumicino -> Rome
            { "CIA", "ROM" }, // Rome Ciampino -> Rome
            { "CDG", "PAR" }, // Paris Charles de Gaulle -> Paris
            { "ORY", "PAR" }, // Paris Orly -> Paris
            { "LHR", "LON" }, // London Heathrow -> London
            { "LGW", "LON" }, // London Gatwick -> London
            { "STN", "LON" }, // London Stansted -> London
            { "AMS", "AMS" }, // Amsterdam
            { "FRA", "FRA" }, // Frankfurt
            { "MUC", "MUC" }, // Munich
            { "BER", "BER" }, // Berlin
            { "VIE", "VIE" }, // Vienna
            { "ZRH", "ZRH" }, // Zurich
            { "BRU", "BRU" }, // Brussels
            { "LIS", "LIS" }, // Lisbon
            { "ATH", "ATH" }, // Athens
            { "DUB", "DUB" }, // Dublin
            { "PRG", "PRG" }, // Prague
            { "WAW", "WAW" }, // Warsaw
            { "BUD", "BUD" }, // Budapest
            { "OSL", "OSL" }, // Oslo
            { "ARN", "STO" }, // Stockholm Arlanda -> Stockholm
            { "HEL", "HEL" }, // Helsinki
            // Middle East
            { "DXB", "DXB" }, // Dubai
            { "DOH", "DOH" }, // Doha
            { "IST", "IST" }, // Istanbul
            // North America
            { "JFK", "NYC" }, // New York JFK -> New York
            { "LGA", "NYC" }, // New York LaGuardia -> New York
            { "EWR", "NYC" }, // Newark -> New York
            { "LAX", "LAX" }, // Los Angeles
            { "ORD", "CHI" }, // Chicago O'Hare -> Chicago
            { "MDW", "CHI" }, // Chicago Midway -> Chicago
            { "SFO", "SFO" }, // San Francisco
            { "MIA", "MIA" }, // Miami
            { "BOS", "BOS" }, // Boston
            { "SEA", "SEA" }, // Seattle
            { "LAS", "LAS" }, // Las Vegas
            { "YYZ", "YTO" }, // Toronto Pearson -> Toronto
            // Asia
            { "HND", "TYO" }, // Tokyo Haneda -> Tokyo
            { "NRT", "TYO" }, // Tokyo Narita -> Tokyo
            { "PEK", "BJS" }, // Beijing Capital -> Beijing
            { "PVG", "SHA" }, // Shanghai Pudong -> Shanghai
            { "SHA", "SHA" }, // Shanghai Hongqiao -> Shanghai
            { "HKG", "HKG" }, // Hong Kong
            { "ICN", "SEL" }, // Seoul Incheon -> Seoul
            { "SIN", "SIN" }, // Singapore
            { "BKK", "BKK" }, // Bangkok
            { "DEL", "DEL" }, // Delhi
            { "BOM", "BOM" }, // Mumbai
            // Australia
            { "SYD", "SYD" }, // Sydney
            { "MEL", "MEL" }, // Melbourne
            // South America
            { "GRU", "SAO" }, // São Paulo Guarulhos -> São Paulo
            { "GIG", "RIO" }, // Rio de Janeiro Galeão -> Rio
            { "EZE", "BUE" }, // Buenos Aires Ezeiza -> Buenos Aires
        };

        public HotelService(IHttpClientFactory factory, ITokenService tokenService, ILogger<HotelService> logger)
        {
            _factory = factory;
            _tokenService = tokenService;
            _logger = logger;
        }

        private string ConvertAirportToCityCode(string code)
        {
            if (string.IsNullOrEmpty(code))
                return code;

            // Convert to uppercase for consistency
            code = code.ToUpper();

            // If we have a mapping, use it
            if (_airportToCityCode.TryGetValue(code, out var cityCode))
            {
                _logger.LogInformation($"Converted airport code {code} to city code {cityCode}");
                return cityCode;
            }

            // Otherwise, assume it's already a city code
            _logger.LogInformation($"No conversion needed for code {code}, using as-is");
            return code;
        }

        public async Task<List<HotelInfo>> SearchHotelsAsync(string cityCode, DateTime checkIn, DateTime checkOut)
        {
            cityCode = ConvertAirportToCityCode(cityCode);

            if (string.IsNullOrEmpty(cityCode))
            {
                _logger.LogWarning("SearchHotelsAsync called with empty cityCode. Using mock data.");
                return CreateMockHotels(cityCode ?? "BCN", checkIn, checkOut);
            }

            try
            {
                var token = await _tokenService.GetTokenAsync();
                var client = _factory.CreateClient("AmadeusAPI");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                // STEP 1: Get hotelIds from city
                var lookupUrl = $"/v1/reference-data/locations/hotels/by-city?cityCode={cityCode}";
                _logger.LogInformation($"Calling Amadeus hotel lookup: {lookupUrl}");
                var lookupResp = await client.GetAsync(lookupUrl);

                if (!lookupResp.IsSuccessStatusCode)
                {
                    _logger.LogError($"Hotel lookup failed: {await lookupResp.Content.ReadAsStringAsync()}");
                    return CreateMockHotels(cityCode, checkIn, checkOut);
                }

                var lookupJson = JObject.Parse(await lookupResp.Content.ReadAsStringAsync());
                var hotelIds = lookupJson["data"]?
                    .Select(h => h["hotelId"]?.ToString())
                    .Where(id => !string.IsNullOrEmpty(id))
                    .Distinct()
                    .Take(20) // limit for performance
                    .ToList();

                if (hotelIds == null || !hotelIds.Any())
                {
                    _logger.LogWarning("No hotelIds found for city, using mock data");
                    return CreateMockHotels(cityCode, checkIn, checkOut);
                }

                var idsParam = string.Join(",", hotelIds);

                // STEP 2: Call hotel-offers using hotelIds
                var offersUrl = $"/v3/shopping/hotel-offers?hotelIds={idsParam}&checkInDate={checkIn:yyyy-MM-dd}&checkOutDate={checkOut:yyyy-MM-dd}&adults=1&roomQuantity=1";
                _logger.LogInformation($"Calling Amadeus hotel offers: {offersUrl}");
                var offersResp = await client.GetAsync(offersUrl);

                if (!offersResp.IsSuccessStatusCode)
                {
                    _logger.LogError($"Hotel offers failed: {await offersResp.Content.ReadAsStringAsync()}");
                    return CreateMockHotels(cityCode, checkIn, checkOut);
                }

                var offersJson = JObject.Parse(await offersResp.Content.ReadAsStringAsync());
                var offers = offersJson["data"] as JArray;

                if (offers == null || !offers.Any())
                {
                    _logger.LogWarning("No offers returned, using mock data");
                    return CreateMockHotels(cityCode, checkIn, checkOut);
                }

                var results = new List<HotelInfo>();
                for (int i = 0; i < offers.Count && i < 10; i++)
                {
                    try
                    {
                        // Parse hotel price and convert with smart auto-correction
                        var rawPrice = ParseDecimal(GetNestedString(offers[i], "offers", 0, "price", "base"))
                            ?? ParseDecimal(GetNestedString(offers[i], "offers", 0, "price", "total"))
                            ?? 250000; // Default

                        // 🔍 DEBUGGING - Raw price info
                        _logger.LogWarning($"Hotel raw price: {rawPrice}");
                        _logger.LogWarning($"Hotel location: {GetNestedString(offers[i], "hotel", "address", "cityName")}");
                        _logger.LogWarning($"Hotel name: {GetNestedString(offers[i], "hotel", "name")}");

                        // Start with default division
                        var convertedPrice = rawPrice / 500;

                        // 🎯 SMART AUTO-CORRECTION with better thresholds
                        string correctionNote = "no correction";
                        if (convertedPrice > 300) // More than $300/night is probably too high for most hotels
                        {
                            convertedPrice = rawPrice / 1500; // Mid-level correction
                            correctionNote = "high price corrected (/1500)";

                            // Double-check if still too high
                            if (convertedPrice > 500) // Still more than $500/night
                            {
                                convertedPrice = rawPrice / 5000; // More aggressive
                                correctionNote = "very high price corrected (/5000)";
                            }
                        }
                        else if (convertedPrice < 15) // Less than $15/night is probably wrong
                        {
                            convertedPrice = rawPrice / 100; // Less aggressive division  
                            correctionNote = "low price corrected (/100)";
                        }

                        // 🔍 MORE DEBUGGING - Final price with correction info
                        _logger.LogWarning($"Hotel converted price (after auto-correction): {convertedPrice} - {correctionNote}");

                        var hotelInfo = new HotelInfo
                        {
                            Id = i + 1,
                            HotelName = GetNestedString(offers[i], "hotel", "name") ?? $"Hotel {i + 1}",
                            Address = GetAddressLine(offers[i]),
                            City = GetNestedString(offers[i], "hotel", "address", "cityName") ?? cityCode,
                            Country = GetNestedString(offers[i], "hotel", "address", "countryCode") ?? "Unknown",
                            StarRating = ParseInt(GetNestedString(offers[i], "hotel", "rating")) ?? 3,
                            CheckInDate = checkIn,
                            CheckOutDate = checkOut,
                            RoomType = GetNestedString(offers[i], "offers", 0, "room", "typeEstimated", "category")
                                ?? GetNestedString(offers[i], "offers", 0, "room", "type")
                                ?? "Standard Room",
                            PricePerNight = convertedPrice
                        };

                        results.Add(hotelInfo);
                        _logger.LogInformation($"Parsed hotel {hotelInfo.HotelName} with price ${hotelInfo.PricePerNight:F2}/night");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Error parsing hotel offer {i}: {ex.Message}");
                    }
                }

                return results.Any() ? results : CreateMockHotels(cityCode, checkIn, checkOut);
            }
            catch (Exception ex)
            {
                _logger.LogError($"SearchHotelsAsync error: {ex.Message}");
                return CreateMockHotels(cityCode, checkIn, checkOut);
            }
        }

        private decimal? ParseDecimal(string decimalString)
        {
            if (string.IsNullOrEmpty(decimalString))
                return null;
            if (decimal.TryParse(decimalString, out var result))
            {
                return result;
            }
            return null;
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
                return "Main Street 1";
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting address line: {ex.Message}");
                return "Main Street 1";
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
                        return null;
                    }
                }

                return current.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting nested string: {ex.Message}");
                return null;
            }
        }

        private int? ParseInt(string intString)
        {
            if (string.IsNullOrEmpty(intString))
                return null;

            if (int.TryParse(intString, out var result))
            {
                return result;
            }
            return null;
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
                { "ROM", ("Rome", "IT") },
                { "FCO", ("Rome", "IT") },
                { "PAR", ("Paris", "FR") },
                { "CDG", ("Paris", "FR") },
                { "LON", ("London", "UK") },
                { "LHR", ("London", "UK") },
                { "CPH", ("Copenhagen", "DK") },
                { "DXB", ("Dubai", "AE") },
                { "NYC", ("New York", "US") },
                { "JFK", ("New York", "US") },
                { "TYO", ("Tokyo", "JP") },
                { "BKK", ("Bangkok", "TH") },
                { "SIN", ("Singapore", "SG") },
                { "SYD", ("Sydney", "AU") }
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
                    Id = i + 1,
                    HotelName = hotelName,
                    Address = $"{random.Next(1, 200)} Main Street",
                    City = cityName,
                    Country = countryCode,
                    StarRating = starRating,
                    CheckInDate = checkIn,
                    CheckOutDate = checkOut,
                    RoomType = roomType,
                    PricePerNight = Math.Round(pricePerNight, 2)
                });
            }

            _logger.LogInformation($"Created {hotels.Count} mock hotels");
            return hotels;
        }
    }
}