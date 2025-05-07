namespace Gotorz.Services
{
    using System.Text.Json;
    using global::Gotorz.Data;
    using global::Gotorz.ViewModel;
    using global::Gotorz.DTOs;

    using Microsoft.EntityFrameworkCore;


        public class HotelService
        {
            private readonly ApplicationDbContext _context;
        public HotelService(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
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
                ApiHotelId = h.ApiHotelId,
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
                    existingHotel.FromPrice = hotel.FromPrice; // ← vigtigt!
                    existingHotel.ApiHotelId = hotel.ApiHotelId;

                hotel.HotelName = hotel.Name;
                await _context.SaveChangesAsync();
                }
            }

            public async Task<Hotel?> GetHotelByApiIdAsync(string apiHotelId)
            {
                return await _context.Hotels.FirstOrDefaultAsync(h => h.ApiHotelId == apiHotelId);
            }

            public async Task<HotelPriceResult?> GetLiveHotelPriceAsync(string apiHotelId, DateTime checkIn, DateTime checkOut)
            {
                try
                {
                    var client = new HttpClient();

                    // 👇 Her bygger vi API-kaldet til hotel-pris
                    var url = $"https://api.makcorps.com/booking?country=us&hotelid={apiHotelId}&checkin={checkIn:yyyy-MM-dd}&checkout={checkOut:yyyy-MM-dd}&currency=USD&adults=1&kids=0&rooms=1&api_key=din_api_nøgle_her";

                    var response = await client.GetAsync(url);
                    response.EnsureSuccessStatusCode();

                    var json = await response.Content.ReadAsStringAsync();

                    var result = JsonSerializer.Deserialize<HotelPriceResponseDto>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });


                    if (result != null && result.Rooms.Any())
                    {
                        var cheapestRoom = result.Rooms.OrderBy(r => r.Price).FirstOrDefault();
                        if (cheapestRoom != null)
                        {
                            return new HotelPriceResult
                            {
                                Price = decimal.TryParse(cheapestRoom.Price?.Replace("US$", "").Replace(",", ""), out var p) ? p : 0,
                                RoomType = cheapestRoom.Room
                            };
                        }
                    }

                    return null;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Fejl ved hentning af hotelpris: {ex.Message}");
                    return null;
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
            if (_context?.Hotels == null)
                return new List<HotelViewModel>();

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
                })
                .ToListAsync();
        }

        public async Task AddHotelAsync(Hotel hotel)
        {
            try
            {
                // Sikkerhed for, at alle påkrævede felter har en værdi
                hotel.Name = string.IsNullOrWhiteSpace(hotel.Name) ? string.Empty : hotel.Name;
                hotel.Address = string.IsNullOrWhiteSpace(hotel.Address) ? string.Empty : hotel.Address;
                hotel.City = string.IsNullOrWhiteSpace(hotel.City) ? string.Empty : hotel.City;
                hotel.Country = string.IsNullOrWhiteSpace(hotel.Country) ? string.Empty : hotel.Country;
                hotel.ApiHotelId = string.IsNullOrWhiteSpace(hotel.ApiHotelId) ? "DEFAULT_ID" : hotel.ApiHotelId;

                // VIGTIGT: Sæt HotelName samme værdi som Name
                hotel.HotelName = hotel.Name;

                _context.Hotels.Add(hotel);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fejl i AddHotelAsync: {ex.Message}");
                throw; // Genkast fejlen, så den kan håndteres i UI
            }
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
        }
    }



