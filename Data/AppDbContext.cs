using EventManagement.API.Models;
using Microsoft.EntityFrameworkCore;

namespace EventManagement.API.Data
{
        public class AppDbContext : DbContext
        {
            public AppDbContext(DbContextOptions<AppDbContext> options)
                : base(options)
            {
            }

            public DbSet<User> Users { get; set; }
            public DbSet<Event> Events => Set<Event>();
            public DbSet<Venue> Venues => Set<Venue>();
            public DbSet<EventRequest> EventRequests { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<EventRequest>()
                .HasOne(er => er.User)
                .WithMany()
                .HasForeignKey(er => er.UserId)
                .OnDelete(DeleteBehavior.NoAction); // 🔥 KEY FIX

            modelBuilder.Entity<EventRequest>()
                .HasOne(er => er.Event)
                .WithMany()
                .HasForeignKey(er => er.EventId)
                .OnDelete(DeleteBehavior.Cascade);
        }

    }
}

