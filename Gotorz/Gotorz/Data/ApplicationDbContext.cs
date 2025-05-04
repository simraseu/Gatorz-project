using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Gotorz.Models;
using Gotorz.Components.Account;

namespace Gotorz.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole, string>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> AppUsers { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<TravelPackage> TravelPackages { get; set; }
        public DbSet<FlightInfo> FlightInfos { get; set; }
        public DbSet<HotelInfo> HotelInfos { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Decimal precision configuration
            modelBuilder.Entity<Booking>()
                .Property(b => b.TotalPrice)
                .HasPrecision(18, 2);

            modelBuilder.Entity<FlightInfo>()
                .Property(f => f.Price)
                .HasPrecision(18, 2);

            modelBuilder.Entity<HotelInfo>()
                .Property(h => h.PricePerNight)
                .HasPrecision(18, 2);

            modelBuilder.Entity<TravelPackage>()
                .Property(tp => tp.Price)
                .HasPrecision(18, 2);

            // Eksisterende relationer
            modelBuilder.Entity<Booking>()
                .HasOne(b => b.User)
                .WithMany(u => u.Bookings)
                .HasForeignKey(b => b.UserId);

            modelBuilder.Entity<TravelPackage>()
                .HasOne(tp => tp.Flight)
                .WithOne(f => f.TravelPackage)
                .HasForeignKey<FlightInfo>(f => f.TravelPackageId);

            modelBuilder.Entity<TravelPackage>()
                .HasOne(tp => tp.Hotel)
                .WithOne(h => h.TravelPackage)
                .HasForeignKey<HotelInfo>(h => h.TravelPackageId);
        }
    }
}