using Gotorz.Data;
using Gotorz.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Gotorz.Services
{
    public interface IBookingService
    {
        Task<Booking> CreateBookingAsync(string userEmail, TravelPackageViewModel package);
        Task<List<Booking>> GetUserBookingsAsync(string userEmail);
        Task<Booking> GetBookingByIdAsync(int bookingId);
        Task<Booking> GetBookingByIdForUserAsync(int bookingId, string userEmail);
        Task<bool> CancelBookingAsync(int bookingId, string userEmail);
        Task<List<Booking>> GetAllBookingsAsync(int skip = 0, int take = 50);
    }

    public class BookingService : IBookingService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<BookingService> _logger;
        private readonly IActivityLogService _activityLogService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public BookingService(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            ILogger<BookingService> logger,
            IActivityLogService activityLogService,
        IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
             _activityLogService = activityLogService;
        _httpContextAccessor = httpContextAccessor;
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

                // Create the booking first
                var booking = new Booking
                {
                    UserId = user.Id,
                    BookingDate = DateTime.UtcNow,
                    TotalPrice = package.Price,
                    Status = "Confirmed"
                };

                _context.Bookings.Add(booking);
                await _context.SaveChangesAsync(); // Save to get booking ID

                // Create the travel package entity linked to the booking
                var travelPackage = new TravelPackage
                {
                    Destination = package.Destination,
                    StartDate = package.StartDate,
                    EndDate = package.EndDate,
                    Price = package.Price,
                    Description = package.Description ?? $"Travel package to {package.Destination}",
                    BookingId = booking.Id
                };

                _context.TravelPackages.Add(travelPackage);
                await _context.SaveChangesAsync(); // Save to get the travel package ID

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

                // ** AUDIT LOG - Booking Created **
                await _activityLogService.LogActivityAsync(
                    identityUser.Id,
                    "Booking Created",
                    $"Created booking for {package.Destination} from {package.StartDate:yyyy-MM-dd} to {package.EndDate:yyyy-MM-dd} - Total: ${package.Price}",
                    _httpContextAccessor.HttpContext
                );

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

        public async Task<bool> CancelBookingAsync(int bookingId, string userEmail)
        {
            try
            {
                var booking = await GetBookingByIdForUserAsync(bookingId, userEmail);
                if (booking == null)
                {
                    _logger.LogWarning($"Booking {bookingId} not found for user {userEmail}");
                    return false;
                }

                // Check if booking can be cancelled (business logic)
                var tripStartDate = booking.TravelPackages.FirstOrDefault()?.StartDate;
                if (tripStartDate.HasValue && tripStartDate.Value <= DateTime.Now.AddDays(1))
                {
                    _logger.LogWarning($"Cannot cancel booking {bookingId} - trip starts too soon");
                    return false;
                }

                booking.Status = "Cancelled";

                // ** AUDIT LOG - Booking Cancelled **
                var identityUser = await _userManager.FindByEmailAsync(userEmail);
                if (identityUser != null)
                {
                    var destination = booking.TravelPackages.FirstOrDefault()?.Destination ?? "Unknown";
                    await _activityLogService.LogActivityAsync(
                        identityUser.Id,
                        "Booking Cancelled",
                        $"Cancelled booking {bookingId} for {destination} - Originally scheduled for {tripStartDate:yyyy-MM-dd}",
                        _httpContextAccessor.HttpContext
                    );
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation($"Successfully cancelled booking {bookingId} for user {userEmail}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error cancelling booking {bookingId} for user {userEmail}: {ex.Message}");
                throw;
            }
        }

        public async Task<List<Booking>> GetAllBookingsAsync(int skip = 0, int take = 50)
        {
            try
            {
                return await _context.Bookings
                    .Include(b => b.TravelPackages)
                        .ThenInclude(tp => tp.Flight)
                    .Include(b => b.TravelPackages)
                        .ThenInclude(tp => tp.Hotel)
                    .Include(b => b.User)
                    .OrderByDescending(b => b.BookingDate)
                    .Skip(skip)
                    .Take(take)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting all bookings: {ex.Message}");
                throw;
            }
        }

        // Helper method for business logic validations
        private bool CanBookPackage(TravelPackageViewModel package)
        {
            // Check if trip is in the future
            if (package.StartDate <= DateTime.Now.Date)
            {
                return false;
            }

            // Check if booking is made at least 24 hours in advance
            if (package.StartDate <= DateTime.Now.Date.AddDays(1))
            {
                return false;
            }

            // Add other business logic validations here
            return true;
        }

        private decimal CalculateRefundAmount(Booking booking)
        {
            var tripStartDate = booking.TravelPackages.FirstOrDefault()?.StartDate;
            if (!tripStartDate.HasValue)
            {
                return 0;
            }

            var daysUntilTrip = (tripStartDate.Value - DateTime.Now.Date).Days;

            return daysUntilTrip switch
            {
                >= 30 => booking.TotalPrice * 0.9m, // 90% refund
                >= 15 => booking.TotalPrice * 0.5m, // 50% refund
                _ => 0 // No refund
            };
        }
    }
}