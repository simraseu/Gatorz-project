using Gatorz.Data;
using Gatorz.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Gatorz.Services
{
    public interface IBookingService
    {
        Task<Booking> CreateBookingAsync(string userEmail, TravelPackageViewModel package);
        Task<List<Booking>> GetUserBookingsAsync(string userEmail);
        Task<Booking> GetBookingByIdAsync(int bookingId);
        Task<Booking> GetBookingByIdForUserAsync(int bookingId, string userEmail);
    }

    public class BookingService : IBookingService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<BookingService> _logger;

        public BookingService(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            ILogger<BookingService> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<Booking> CreateBookingAsync(string userEmail, TravelPackageViewModel package)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                _logger.LogInformation($"Creating booking for user {userEmail}, package {package.Id}");

                // Find the Identity user
                var identityUser = await _userManager.FindByEmailAsync(userEmail);
                if (identityUser == null)
                {
                    throw new InvalidOperationException($"Identity user with email {userEmail} not found");
                }

                // Find or create the corresponding User entity in our custom table
                var user = await _context.AppUsers.FirstOrDefaultAsync(u => u.Email == userEmail);
                if (user == null)
                {
                    _logger.LogInformation($"Creating new user record for {userEmail}");
                    user = new User
                    {
                        Email = userEmail,
                        Username = identityUser.UserName ?? userEmail.Split('@')[0],
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
                await _context.SaveChangesAsync(); // Save to get the ID

                // Create flight info if available
                if (package.Flight != null)
                {
                    var flight = new FlightInfo
                    {
                        FlightNumber = package.Flight.FlightNumber ?? "N/A",
                        Airline = package.Flight.Airline ?? package.Airline,
                        DepartureAirport = package.Flight.DepartureAirport ?? package.OriginCity,
                        ArrivalAirport = package.Flight.ArrivalAirport ?? package.Destination,
                        DepartureTime = package.Flight.DepartureTime != default ? package.Flight.DepartureTime : package.FlightDepartureTime,
                        ArrivalTime = package.Flight.ArrivalTime != default ? package.Flight.ArrivalTime : package.FlightArrivalTime,
                        Price = package.Flight.Price,
                        TravelPackageId = travelPackage.Id
                    };
                    _context.FlightInfos.Add(flight);
                }

                // Create hotel info if available
                if (package.Hotel != null)
                {
                    var hotel = new HotelInfo
                    {
                        HotelName = package.Hotel.HotelName ?? package.HotelName,
                        Address = package.Hotel.Address ?? "Address not specified",
                        City = package.Hotel.City ?? package.Destination,
                        Country = package.Hotel.Country ?? "Country not specified",
                        StarRating = package.Hotel.StarRating > 0 ? package.Hotel.StarRating : package.HotelRating,
                        CheckInDate = package.Hotel.CheckInDate != default ? package.Hotel.CheckInDate : package.StartDate,
                        CheckOutDate = package.Hotel.CheckOutDate != default ? package.Hotel.CheckOutDate : package.EndDate,
                        RoomType = package.Hotel.RoomType ?? "Standard Room",
                        PricePerNight = package.Hotel.PricePerNight,
                        TravelPackageId = travelPackage.Id
                    };
                    _context.HotelInfos.Add(hotel);
                }

                await _context.SaveChangesAsync(); // Save flight and hotel info

                // Create the booking
                var booking = new Booking
                {
                    UserId = user.Id,
                    BookingDate = DateTime.UtcNow,
                    TotalPrice = package.Price,
                    Status = "Confirmed"
                };

                _context.Bookings.Add(booking);
                await _context.SaveChangesAsync(); // Save to get booking ID

                // Link the travel package to the booking
                travelPackage.BookingId = booking.Id;
                _context.TravelPackages.Update(travelPackage);
                await _context.SaveChangesAsync();

                // Commit the transaction
                await transaction.CommitAsync();

                // Load the complete booking with related data
                var completeBooking = await _context.Bookings
                    .Include(b => b.TravelPackages)
                        .ThenInclude(tp => tp.Flight)
                    .Include(b => b.TravelPackages)
                        .ThenInclude(tp => tp.Hotel)
                    .Include(b => b.User)
                    .FirstOrDefaultAsync(b => b.Id == booking.Id);

                _logger.LogInformation($"Successfully created booking {booking.Id} for user {userEmail}");

                return completeBooking ?? booking;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError($"Error creating booking for user {userEmail}: {ex.Message}");
                _logger.LogError($"Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        public async Task<List<Booking>> GetUserBookingsAsync(string userEmail)
        {
            try
            {
                var user = await _context.AppUsers.FirstOrDefaultAsync(u => u.Email == userEmail);
                if (user == null)
                {
                    _logger.LogWarning($"No user found with email {userEmail}");
                    return new List<Booking>();
                }

                return await _context.Bookings
                    .Include(b => b.TravelPackages)
                        .ThenInclude(tp => tp.Flight)
                    .Include(b => b.TravelPackages)
                        .ThenInclude(tp => tp.Hotel)
                    .Include(b => b.User)
                    .Where(b => b.UserId == user.Id)
                    .OrderByDescending(b => b.BookingDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting user bookings for {userEmail}: {ex.Message}");
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
                _logger.LogError($"Error getting booking by ID {bookingId}: {ex.Message}");
                throw;
            }
        }

        public async Task<Booking> GetBookingByIdForUserAsync(int bookingId, string userEmail)
        {
            try
            {
                var user = await _context.AppUsers.FirstOrDefaultAsync(u => u.Email == userEmail);
                if (user == null)
                {
                    return null;
                }

                return await _context.Bookings
                    .Include(b => b.TravelPackages)
                        .ThenInclude(tp => tp.Flight)
                    .Include(b => b.TravelPackages)
                        .ThenInclude(tp => tp.Hotel)
                    .Include(b => b.User)
                    .FirstOrDefaultAsync(b => b.Id == bookingId && b.UserId == user.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting booking {bookingId} for user {userEmail}: {ex.Message}");
                throw;
            }
        }
    }
}