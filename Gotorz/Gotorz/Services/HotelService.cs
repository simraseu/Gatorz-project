using Gotorz.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Gotorz.Services
{
    public class HotelService : IHotelService
    {
        // Mock implementation until you have a real API
        public async Task<List<HotelInfo>> SearchHotelsAsync(string location, DateTime checkIn, DateTime checkOut)
        {
            // Simulates API call
            await Task.Delay(500);

            // Returns mock data
            return new List<HotelInfo>
            {
                new HotelInfo
                {
                    Id = 1,
                    HotelName = "Grand Hotel",
                    Address = "Main Street 123",
                    City = location,
                    Country = "Denmark",
                    StarRating = 4,
                    CheckInDate = checkIn,
                    CheckOutDate = checkOut,
                    RoomType = "Double Room",
                    PricePerNight = 899
                },
                new HotelInfo
                {
                    Id = 2,
                    HotelName = "Seaside Resort",
                    Address = "Beach Road 45",
                    City = location,
                    Country = "Denmark",
                    StarRating = 5,
                    CheckInDate = checkIn,
                    CheckOutDate = checkOut,
                    RoomType = "Deluxe Suite",
                    PricePerNight = 1499
                },
                new HotelInfo
                {
                    Id = 3,
                    HotelName = "City Budget Hotel",
                    Address = "Station Square 7",
                    City = location,
                    Country = "Denmark",
                    StarRating = 3,
                    CheckInDate = checkIn,
                    CheckOutDate = checkOut,
                    RoomType = "Single Room",
                    PricePerNight = 599
                }
            };
        }

        public async Task<HotelInfo> GetHotelDetailAsync(string hotelId)
        {
            // Simulates API call
            await Task.Delay(300);

            // Returns mock data
            return new HotelInfo
            {
                Id = int.Parse(hotelId),
                HotelName = "Grand Hotel",
                Address = "Main Street 123",
                City = "Copenhagen",
                Country = "Denmark",
                StarRating = 4,
                CheckInDate = DateTime.Now.AddDays(30),
                CheckOutDate = DateTime.Now.AddDays(37),
                RoomType = "Double Room",
                PricePerNight = 899
            };
        }
    }
}