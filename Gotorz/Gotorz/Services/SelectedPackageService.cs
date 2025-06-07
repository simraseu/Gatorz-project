
using Gotorz.Models;

namespace Gotorz.Services
{
    public class SelectedPackageService
    {
        private TravelPackageViewModel _selectedPackage;

        public void SetPackage(TravelPackageViewModel package)
        {
            _selectedPackage = package;
        }

        public TravelPackageViewModel GetPackage()
        {
            return _selectedPackage;
        }

        public void Clear() => _selectedPackage = null;
    }
}