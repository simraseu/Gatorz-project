namespace Gotorz.Services
{
    using System;
   
    using System.Net.Http;
    using System.Text.Json;
    using System.Threading.Tasks;
    using global::Gotorz.DTOs;

    public interface IHotelApiService
        {
            Task<HotelPriceResponseDto> SearchHotelPricesAsync(
                string country,
                string hotelId,
                DateTime checkIn,
                DateTime checkOut,
                string currency,
                int adults = 2,
                int kids = 0,
                int rooms = 1);
        }

        public class HotelApiService : IHotelApiService
        {
            private readonly HttpClient _httpClient;
            private const string _apiKey = "6810a76bf4557a186c9dfeec";


            public HotelApiService(HttpClient httpClient)
            {
                _httpClient = httpClient;
            }

            public async Task<HotelPriceResponseDto> SearchHotelPricesAsync(
                string country,
                string hotelId,
                DateTime checkIn,
                DateTime checkOut,
                string currency,
                int adults = 2,
                int kids = 0,
                int rooms = 1)
            {
                // 1) Formatér datoerne
                var checkInStr = checkIn.ToString("yyyy-MM-dd");
                var checkOutStr = checkOut.ToString("yyyy-MM-dd");

                // 2) Byg URL
                var url = $"https://api.makcorps.com/hotel" +
                          $"?country={country}" +
                          $"&hotelid={hotelId}" +
                          $"&checkin={checkInStr}" +
                          $"&checkout={checkOutStr}" +
                          $"&currency={currency}" +
                          $"&adults={adults}" +
                          $"&kids={kids}" +
                          $"&rooms={rooms}" +
                          $"&api_key={_apiKey}";


                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                    return null;

                var json = await response.Content.ReadAsStringAsync();

                // 🛎️ Ny linje: Log JSON-svar til console
                Console.WriteLine("=== Hotel API Response ===");
                Console.WriteLine(json);
                Console.WriteLine("===========================");

                var data = JsonSerializer.Deserialize<HotelPriceResponseDto>(json);
                return data;

                /*
                            // 3) Hent rå JSON
                            var resp = await _httpClient.GetAsync(url);
                            resp.EnsureSuccessStatusCode();
                            var json = await resp.Content.ReadAsStringAsync();

                            // 4) Parse til JsonDocument og brug din FromJson-factory
                            using var doc = JsonDocument.Parse(json);
                            return JsonSerializer.Deserialize<HotelPriceResponseDto>(json, new JsonSerializerOptions
                            {
                                PropertyNameCaseInsensitive = true
                            });
                        }*/
            }
        }
    }
