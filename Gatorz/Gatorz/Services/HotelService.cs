using Gatorz.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Gatorz.Services
{
    public class HotelService : IHotelService
    {
        private readonly IHttpClientFactory _factory;
        private readonly ITokenService _tokenService;

        public HotelService(IHttpClientFactory factory, ITokenService tokenService)
        {
            _factory = factory;
            _tokenService = tokenService;
        }

        public async Task<List<HotelInfo>> SearchHotelsAsync(string cityCode, DateTime checkIn, DateTime checkOut)
        {
            var token = await _tokenService.GetTokenAsync();
            var client = _factory.CreateClient("AmadeusAPI");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var url = $"/v2/shopping/hotel-offers?cityCode={cityCode}&checkInDate={checkIn:yyyy-MM-dd}&checkOutDate={checkOut:yyyy-MM-dd}&adults=1&roomQuantity=1";
            var resp = await client.GetAsync(url);
            resp.EnsureSuccessStatusCode();
            var root = JObject.Parse(await resp.Content.ReadAsStringAsync());
            var offers = root["data"] as JArray;
            if (offers == null) return new List<HotelInfo>();

            return offers.Select(o => new HotelInfo
            {
                Id = (int)o["hotel"]["hotelId"],
                HotelName = (string)o["hotel"]["name"],
                Address = ((JArray)o["hotel"]["address"]["lines"]).FirstOrDefault()?.ToString() ?? string.Empty,
                City = (string)o["hotel"]["address"]["cityName"],
                Country = (string)o["hotel"]["address"]["countryCode"],
                StarRating = (int?)o["hotel"]["rating"] ?? 0,
                CheckInDate = checkIn,
                CheckOutDate = checkOut,
                RoomType = (string)o["offers"][0]["room"],
                PricePerNight = decimal.Parse((string)o["offers"][0]["price"]["base"])
            }).ToList();
        }

        public Task<HotelInfo> GetHotelDetailAsync(string hotelId)
        {
            throw new NotImplementedException();
        }
    }
}