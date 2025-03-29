using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Gatorz.Data
{
    // Add profile data for application users by adding properties to the ApplicationUser class
    public class ApplicationUser : IdentityUser
    {
        [PersonalData]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;

        [PersonalData]
        [Display(Name = "Last Name")]
        public string LastName { get; set; } = string.Empty;

        [PersonalData]
        [Display(Name = "Address")]
        public string Address { get; set; } = string.Empty;

        [PersonalData]
        [Display(Name = "City")]
        public string City { get; set; } = string.Empty;

        [PersonalData]
        [Display(Name = "Country")]
        public string Country { get; set; } = string.Empty;

        [PersonalData]
        [Display(Name = "Date of Birth")]
        public DateTime? DateOfBirth { get; set; }

        [PersonalData]
        [Display(Name = "Profile Created")]
        public DateTime ProfileCreated { get; set; } = DateTime.UtcNow;
    }
}