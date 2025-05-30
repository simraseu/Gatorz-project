using Gatorz.Data;
using Gatorz.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Gatorz.Services
{
    public interface IBookingService
    {
        Task<Booking> CreateBookingAsync(string userId, TravelPackageViewModel package);
        Task<List<Booking>> GetUserBookingsAsync(string userId);
        Task<Booking> GetBookingByIdAsync(int bookingId);
    }

    public class BookingService : IBookingService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<BookingService> _logger;

        public BookingService(ApplicationDbContext context, ILogger<BookingService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Booking> CreateBookingAsync(string userId, TravelPackageViewModel package)
        {
            try
            {
                // First, we need to find or create the user in our User table
                var user = await _context.AppUsers.FirstOrDefaultAsync(u => u.Email == userId);

                if (user == null)
                {
                    // Create a new user entry
                    user = new User
                    {
                        Email = userId,
                        Username = userId.Split('@')[0],
                        Role = "Customer"
                    };
                    _context.AppUsers.Add(user);
                    await _context.SaveChangesAsync();
                }

                // Create the travel package entity
                var travelPackage = new TravelPackage
                {
                    Destination = package.Destination,
                    StartDate = package.StartDate,
                    EndDate = package.EndDate,
                    Price = package.Price,
                    Description = package.Description ?? $"Travel package to {package.Destination}"
                };

                _context.TravelPackages.Add(travelPackage);
                await _context.SaveChangesAsync();

                // Create flight info
                if (package.Flight != null)
                {
                    var flight = new FlightInfo
                    {
                        FlightNumber = package.Flight.FlightNumber,
                        Airline = package.Flight.Airline,
                        DepartureAirport = package.Flight.DepartureAirport,
                        ArrivalAirport = package.Flight.ArrivalAirport,
                        DepartureTime = package.Flight.DepartureTime,
                        ArrivalTime = package.Flight.ArrivalTime,
                        Price = package.Flight.Price,
                        TravelPackageId = travelPackage.Id
                    };
                    _context.FlightInfos.Add(flight);
                }

                // Create hotel info
                if (package.Hotel != null)
                {
                    var hotel = new HotelInfo
                    {
                        HotelName = package.Hotel.HotelName,
                        Address = package.Hotel.Address,
                        City = package.Hotel.City,
                        Country = package.Hotel.Country,
                        StarRating = package.Hotel.StarRating,
                        CheckInDate = package.Hotel.CheckInDate,
                        CheckOutDate = package.Hotel.CheckOutDate,
                        RoomType = package.Hotel.RoomType,
                        PricePerNight = package.Hotel.PricePerNight,
                        TravelPackageId = travelPackage.Id
                    };
                    _context.HotelInfos.Add(hotel);
                }

                await _context.SaveChangesAsync();

                // Create the booking
                var booking = new Booking
                {
                    UserId = user.Id,
                    BookingDate = DateTime.UtcNow,
                    TotalPrice = package.Price,
                    Status = "Confirmed",
                    TravelPackages = new List<TravelPackage> { travelPackage }
                };

                _context.Bookings.Add(booking);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Created booking {booking.Id} for user {userId}");

                return booking;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error creating booking: {ex.Message}");
                throw;
            }
        }

        public async Task<List<Booking>> GetUserBookingsAsync(string userId)
        {
            try
            {
                var user = await _context.AppUsers.FirstOrDefaultAsync(u => u.Email == userId);
                if (user == null)
                    return new List<Booking>();

                return await _context.Bookings
                    .Include(b => b.TravelPackages)
                        .ThenInclude(tp => tp.Flight)
                    .Include(b => b.TravelPackages)
                        .ThenInclude(tp => tp.Hotel)
                    .Where(b => b.UserId == user.Id)
                    .OrderByDescending(b => b.BookingDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting user bookings: {ex.Message}");
                throw;
            }
        }

        public async Task<Booking> GetBookingByIdAsync(int bookingId)
        {
            try
            {
                return await _context.Bookings
                    .Include(b => b.TravelPackages)
                        .ThenInclude(tp => tp.Flight)
                    .Include(b => b.TravelPackages)
                        .ThenInclude(tp => tp.Hotel)
                    .Include(b => b.User)
                    .FirstOrDefaultAsync(b => b.Id == bookingId);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting booking by ID: {ex.Message}");
                throw;
            }
        }
    }
}