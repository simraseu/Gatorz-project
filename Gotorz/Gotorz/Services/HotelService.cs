using Gotorz.Data;
using Gotorz.Models;
using Gotorz.ViewModel;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Gotorz.Services
{
    public class HotelService : IHotelService
    {
        private readonly ApplicationDbContext _context;

        public HotelService(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<List<HotelViewModel>> GetAllHotelsAsync(string? country = null, string? city = null)
        {
            var query = _context.Hotels.AsQueryable();

            if (!string.IsNullOrEmpty(country))
                query = query.Where(h => h.Country == country);

            if (!string.IsNullOrEmpty(city))
                query = query.Where(h => h.City == city);

            var hotels = await query.ToListAsync();

            return hotels.Select(h => new HotelViewModel
            {
                Id = h.Id,
                Name = h.Name,
                Address = h.Address,
                Country = h.Country,
                City = h.City
            }).ToList();
        }

        public async Task<Hotel?> GetHotelByIdAsync(int id)
        {
            return await _context.Hotels.FindAsync(id);
        }
        public async Task<HotelViewModel?> GetHotelDetailsAsync(int id)
        {
            var hotel = await _context.Hotels.FindAsync(id);
            if (hotel == null) return null;

            return new HotelViewModel
            {
                Id = hotel.Id,
                Name = hotel.Name,
                Address = hotel.Address,
                City = hotel.City,
                Country = hotel.Country,
                ApiHotelId = hotel.ApiHotelId,
                FromPrice = hotel.FromPrice // Denne skal være i databasen!
            };
        }
        // I HotelService.cs

        public async Task UpdateHotelAsync(Hotel hotel)
        {
            var existingHotel = await _context.Hotels.FindAsync(hotel.Id);
            if (existingHotel != null)
            {
                existingHotel.Name = hotel.Name;
                existingHotel.Address = hotel.Address;
                existingHotel.Country = hotel.Country;
                existingHotel.City = hotel.City;


                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<string>> GetAllCountriesAsync()
        {
            return await _context.Hotels
                .Select(h => h.Country)
                .Where(c => !string.IsNullOrEmpty(c))
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();
        }

        public async Task<List<string>> GetCitiesByCountryAsync(string country)
        {
            return await _context.Hotels
                .Where(h => h.Country == country && !string.IsNullOrEmpty(h.City))
                .Select(h => h.City)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();
        }

        public async Task<List<HotelViewModel>> GetHotelsByCountryAndCityAsync(string country, string city)
        {
            return await _context.Hotels
                .Where(h => h.Country == country && h.City == city)
                .Select(h => new HotelViewModel
                {
                    Id = h.Id,
                    Name = h.Name,
                    Address = h.Address,
                    ApiHotelId = h.ApiHotelId,
                    Country = h.Country,
                    City = h.City
                }).ToListAsync();
        }

        public async Task AddHotelAsync(Hotel hotel)
        {
            _context.Hotels.Add(hotel);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteHotelAsync(int id)
        {
            var hotel = await _context.Hotels.FindAsync(id);
            if (hotel != null)
            {
                _context.Hotels.Remove(hotel);
                await _context.SaveChangesAsync();
            }
        }
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