using Gatorz.Models;

namespace Gatorz.Services
{
    public interface ITravelPackageService
    {
        Task<List<TravelPackageViewModel>> SearchPackagesAsync(string origin, string destination, DateTime departureDate, DateTime returnDate);
        Task<TravelPackageViewModel> GetPackageByIdAsync(string packageId);
    }
}