using Gotorz.Models;
using Gotorz.ViewModel;

public interface ITravelPackageService
{
    Task SaveTravelPackageAsync(TravelPackageFormViewModel model);
    Task<List<TravelPackageViewModel>> GetAllPackagesAsync();

    Task<TravelPackageViewModel> CreatePackageAsync(string from, string to, DateTime departureDate, DateTime returnDate, string checkIn, string checkOut, string currency, string country, string hotelId, int adults, int kids, int rooms);
    Task<List<TravelPackageViewModel>> SearchPackagesAsync(string origin, string destination, DateTime departureDate, DateTime returnDate);
    Task<TravelPackageViewModel> GetPackageByIdAsync(string packageId);
}