using Gotorz.Data;
using Gotorz.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Gotorz.Services
{
    public interface IActivityLogService
    {
        Task LogActivityAsync(string userId, string action, string details, HttpContext? httpContext = null);
        Task<List<ActivityLog>> GetActivityLogsAsync(int skip = 0, int take = 100);
        Task<List<ActivityLog>> GetUserActivityLogsAsync(string userId, int skip = 0, int take = 50);
        Task<int> GetTotalActivityCountAsync();
        Task<List<ActivityLog>> SearchActivityLogsAsync(string searchTerm, int skip = 0, int take = 100);
    }

    public class ActivityLogService : IActivityLogService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<ActivityLogService> _logger;

        public ActivityLogService(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            ILogger<ActivityLogService> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        public async Task LogActivityAsync(string userId, string action, string details, HttpContext? httpContext = null)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                var log = new ActivityLog
                {
                    UserId = userId,
                    UserName = user != null ? $"{user.FirstName} {user.LastName}" : "Unknown User",
                    Action = action,
                    Details = details,
                    IPAddress = httpContext?.Connection.RemoteIpAddress?.ToString() ?? "Unknown",
                    UserAgent = httpContext?.Request.Headers["User-Agent"].ToString() ?? "Unknown",
                    Timestamp = DateTime.UtcNow
                };

                _context.ActivityLogs.Add(log);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Activity logged: {action} by {log.UserName}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Activity logging error: {ex.Message}");
                // Don't throw - logging failures shouldn't break the main application flow
            }
        }

        public async Task<List<ActivityLog>> GetActivityLogsAsync(int skip = 0, int take = 100)
        {
            return await _context.ActivityLogs
                .OrderByDescending(a => a.Timestamp)
                .Skip(skip)
                .Take(take)
                .ToListAsync();
        }

        public async Task<List<ActivityLog>> GetUserActivityLogsAsync(string userId, int skip = 0, int take = 50)
        {
            return await _context.ActivityLogs
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.Timestamp)
                .Skip(skip)
                .Take(take)
                .ToListAsync();
        }

        public async Task<int> GetTotalActivityCountAsync()
        {
            return await _context.ActivityLogs.CountAsync();
        }

        public async Task<List<ActivityLog>> SearchActivityLogsAsync(string searchTerm, int skip = 0, int take = 100)
        {
            var query = _context.ActivityLogs.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(a =>
                    a.UserName.Contains(searchTerm) ||
                    a.Action.Contains(searchTerm) ||
                    a.Details.Contains(searchTerm));
            }

            return await query
                .OrderByDescending(a => a.Timestamp)
                .Skip(skip)
                .Take(take)
                .ToListAsync();
        }
    }
}