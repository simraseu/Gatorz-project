using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Gatorz.Models;

namespace Gatorz.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSets for our entities
        public DbSet<User> AppUsers { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<TravelPackage> TravelPackages { get; set; }
        public DbSet<FlightInfo> FlightInfos { get; set; }
        public DbSet<HotelInfo> HotelInfos { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; }
        public DbSet<ActivityLog> ActivityLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configure decimal precision for price fields
            builder.Entity<Booking>()
                .Property(b => b.TotalPrice)
                .HasPrecision(18, 2);

            builder.Entity<TravelPackage>()
                .Property(tp => tp.Price)
                .HasPrecision(18, 2);

            builder.Entity<FlightInfo>()
                .Property(f => f.Price)
                .HasPrecision(18, 2);

            builder.Entity<HotelInfo>()
                .Property(h => h.PricePerNight)
                .HasPrecision(18, 2);

            // Configure relationships
            builder.Entity<Booking>()
                .HasOne(b => b.User)
                .WithMany(u => u.Bookings)
                .HasForeignKey(b => b.UserId);

            builder.Entity<TravelPackage>()
                .HasOne(tp => tp.Flight)
                .WithOne(f => f.TravelPackage)
                .HasForeignKey<FlightInfo>(f => f.TravelPackageId);

            builder.Entity<TravelPackage>()
                .HasOne(tp => tp.Hotel)
                .WithOne(h => h.TravelPackage)
                .HasForeignKey<HotelInfo>(h => h.TravelPackageId);

            // Configure indexes for performance
            builder.Entity<ActivityLog>()
                .HasIndex(a => a.UserId);

            builder.Entity<ActivityLog>()
                .HasIndex(a => a.Timestamp);

            builder.Entity<ActivityLog>()
                .HasIndex(a => a.Action);

            builder.Entity<ChatMessage>()
                .HasIndex(c => c.Timestamp);

            builder.Entity<ChatMessage>()
                .HasIndex(c => c.Destination);

            // Rename the default Users table to AppUsers to avoid confusion with Identity
            builder.Entity<User>().ToTable("AppUsers");
        }
    }
}