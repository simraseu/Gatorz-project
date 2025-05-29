using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Gatorz.Models;
using Gatorz.Components.Account;

namespace Gatorz.Data
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
        public DbSet<ChatMessage> ChatMessages { get; set; } //Tilføjet for nyligt krav om chatfunktion
        public DbSet<ActivityLog> ActivityLogs { get; set; } //Tilføjet for nyligt krav om adgangslogning

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Decimal precision konfiguration
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

            // ADD CHAT MESSAGE CONFIGURATION // Tilføjet for nyligt 
            modelBuilder.Entity<ChatMessage>()
                .HasIndex(c => c.Timestamp);

            modelBuilder.Entity<ChatMessage>()
                .HasIndex(c => c.Destination);

            // ADD THIS FOR ACTIVITY LOGS: //Tilføjet for nyligt
            modelBuilder.Entity<ActivityLog>()
                .HasIndex(a => a.Timestamp);

            modelBuilder.Entity<ActivityLog>()
                .HasIndex(a => a.UserId);

            modelBuilder.Entity<ActivityLog>()
                .HasIndex(a => a.Action);
        }
    }
}