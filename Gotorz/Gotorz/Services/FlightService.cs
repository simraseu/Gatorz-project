using Gotorz.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Gotorz.Services
{
    public class FlightService : IFlightService
    {
        // Mock implementation until you have a real API
        public async Task<List<FlightInfo>> SearchFlightsAsync(string origin, string destination, DateTime departureDate)
        {
            // Simulates API call
            await Task.Delay(500);

            // Returns mock data
            return new List<FlightInfo>
            {
                new FlightInfo
                {
                    Id = 1,
                    FlightNumber = "SK123",
                    Airline = "SAS",
                    DepartureAirport = origin,
                    ArrivalAirport = destination,
                    DepartureTime = departureDate.AddHours(9),
                    ArrivalTime = departureDate.AddHours(11),
                    Price = 999
                },
                new FlightInfo
                {
                    Id = 2,
                    FlightNumber = "DY456",
                    Airline = "Norwegian",
                    DepartureAirport = origin,
                    ArrivalAirport = destination,
                    DepartureTime = departureDate.AddHours(14),
                    ArrivalTime = departureDate.AddHours(16),
                    Price = 899
                }
            };
        }

        public async Task<FlightInfo> GetFlightDetailAsync(string flightId)
        {
            // Simulates API call
            await Task.Delay(300);

            // Returns mock data
            return new FlightInfo
            {
                Id = int.Parse(flightId),
                FlightNumber = "SK123",
                Airline = "SAS",
                DepartureAirport = "CPH",
                ArrivalAirport = "LHR",
                DepartureTime = DateTime.Now.AddDays(10).AddHours(9),
                ArrivalTime = DateTime.Now.AddDays(10).AddHours(11),
                Price = 999
            };
        }
    }
}