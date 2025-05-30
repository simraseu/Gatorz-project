using System;
using System.Net.Http;
using System.Text.Json;
using Gotorz.DTOs;


namespace Gotorz.Services
{
    public interface IHotelApiService
    {
        Task<HotelResponseDto> SearchHotelPricesAsync(
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
        private const string _apiKey = "2ea1ed99e4bc43f74bdd0de507ab01aebbd807d961e8c343e6bd808699ffb578";


        public HotelApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<HotelResponseDto> SearchHotelPricesAsync(
     string hotelName,
     DateTime checkIn,
     DateTime checkOut,
     string currency,
     int adults = 2,
     int kids = 0,
     int rooms = 1)
        {
            var checkInStr = checkIn.ToString("yyyy-MM-dd");
            var checkOutStr = checkOut.ToString("yyyy-MM-dd");

            // 1. Find hotellets property_token via søgning på navn
            var searchUrl = $"https://serpapi.com/search.json?engine=google_hotels&q={Uri.EscapeDataString(hotelName)}&api_key={_apiKey}";
            var searchResponse = await _httpClient.GetAsync(searchUrl);
            if (!searchResponse.IsSuccessStatusCode)
                return null;

            var searchJson = await searchResponse.Content.ReadAsStringAsync();
            var searchResult = JsonSerializer.Deserialize<HotelSearchResultDto>(searchJson);

            var token = searchResult?.Properties?.FirstOrDefault()?.PropertyToken;
            if (string.IsNullOrEmpty(token))
                return null;

            // 2. Brug token til at hente hotelpriser
            var priceUrl = $"https://serpapi.com/search.json?engine=google_hotels&property_token={token}&check_in_date={checkInStr}&check_out_date={checkOutStr}&currency={currency}&adults={adults}&children={kids}&rooms={rooms}&hl=en&gl=th&api_key={_apiKey}";

            var priceResponse = await _httpClient.GetAsync(priceUrl);
            if (!priceResponse.IsSuccessStatusCode)
                return null;

            var priceJson = await priceResponse.Content.ReadAsStringAsync();
            Console.WriteLine("📦 API Response:");
            Console.WriteLine(priceJson);

            var priceData = JsonSerializer.Deserialize<HotelResponseDto>(priceJson);
            return priceData;
        }

        public Task<HotelResponseDto> SearchHotelPricesAsync(string country, string hotelId, DateTime checkIn, DateTime checkOut, string currency, int adults = 2, int kids = 0, int rooms = 1)
        {
            throw new NotImplementedException();
        }
    }
}


