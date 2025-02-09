using GroupAllocator.DAL.Entities;
using GroupAllocator.Models.Constants;
using Microsoft.EntityFrameworkCore;

namespace GroupAllocator.DAL.Context
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<Group> Groups { get; set; } = null!;

        public DbSet<TelegramUser> TelegramUsers { get; set; } = null!;

        public DbSet<User> Users { get; set; } = null!;

        public DbSet<UserRole> UserRoles { get; set; } = null!;


        public ApplicationDbContext(DbContextOptions options) : base(options)
        {
            Database.EnsureCreated();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserRole>()
                .HasData(new UserRole { Id = Guid.NewGuid(), Name = Role.ADMIN, Description = string.Empty });

            modelBuilder.Entity<UserRole>()
                .HasData(new UserRole { Id = Guid.NewGuid(), Name = Role.OWNER, Description = string.Empty});

            modelBuilder.Entity<UserRole>()
                .HasData(new UserRole { Id = Guid.NewGuid(), Name = Role.PARTICIPANT, Description = string.Empty });

            base.OnModelCreating(modelBuilder); 
        }
    }
}