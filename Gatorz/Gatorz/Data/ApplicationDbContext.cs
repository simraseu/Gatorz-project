using Microsoft.EntityFrameworkCore;
using Gatorz.Models;

namespace Gatorz.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<TravelPackage> TravelPackages { get; set; }
        public DbSet<FlightInfo> FlightInfos { get; set; }
        public DbSet<HotelInfo> HotelInfos { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Her kan du definere relationer, constraints, osv.
            // Eksempel:
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