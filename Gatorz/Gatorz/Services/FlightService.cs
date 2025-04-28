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
    public class FlightService : IFlightService
    {
        private readonly IHttpClientFactory _factory;
        private readonly ITokenService _tokenService;

        public FlightService(IHttpClientFactory factory, ITokenService tokenService)
        {
            _factory = factory;
            _tokenService = tokenService;
        }

        public async Task<List<FlightInfo>> SearchFlightsAsync(string origin, string destination, DateTime departureDate)
        {
            var token = await _tokenService.GetTokenAsync();
            var client = _factory.CreateClient("AmadeusAPI");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var url = $"/v2/shopping/flight-offers?originLocationCode={origin}&destinationLocationCode={destination}&departureDate={departureDate:yyyy-MM-dd}&adults=1&max=5";
            var resp = await client.GetAsync(url);
            resp.EnsureSuccessStatusCode();
            var root = JObject.Parse(await resp.Content.ReadAsStringAsync());
            var offers = root["data"] as JArray;
            if (offers == null) return new List<FlightInfo>();

            return offers.Select(o => new FlightInfo
            {
                Id = (int)o["id"],
                FlightNumber = (string)o["itineraries"][0]["segments"][0]["carrierCode"] + (string)o["itineraries"][0]["segments"][0]["number"],
                Airline = (string)o["itineraries"][0]["segments"][0]["carrierCode"],
                DepartureAirport = (string)o["itineraries"][0]["segments"][0]["departure"]["iataCode"],
                ArrivalAirport = (string)o["itineraries"][0]["segments"][0]["arrival"]["iataCode"],
                DepartureTime = DateTime.Parse((string)o["itineraries"][0]["segments"][0]["departure"]["at"]),
                ArrivalTime = DateTime.Parse((string)o["itineraries"][0]["segments"][0]["arrival"]["at"]),
                Price = decimal.Parse((string)o["price"]["total"])
            }).ToList();
        }

        public Task<FlightInfo> GetFlightDetailAsync(string flightId)
        {
            // Could reuse cached Search results or implement separately
            throw new NotImplementedException();
        }
    }
}