using Microsoft.EntityFrameworkCore;
using InsuranceAPI.Models;

namespace InsuranceAPI.Data
{
    public class InsuranceDbContext : DbContext
    {
        public InsuranceDbContext(DbContextOptions<InsuranceDbContext> options) : base(options)
        {
        }
        
        public DbSet<User> Users { get; set; }
        public DbSet<Customer> Customers { get; set; }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // User entity configuration
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("Users");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
                entity.HasIndex(e => e.Email).IsUnique();
                entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Role).IsRequired().HasMaxLength(50);
                entity.Property(e => e.PasswordHash).HasColumnName("Password_Hash").IsRequired();
                entity.Property(e => e.CreatedAt).HasColumnName("Created_At").HasDefaultValueSql("GETDATE()");
            });
            
            // Customer entity configuration
            modelBuilder.Entity<Customer>(entity =>
            {
                entity.ToTable("Customers");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Type).IsRequired().HasMaxLength(50);
                entity.Property(e => e.IdNo).HasColumnName("Id_No").IsRequired().HasMaxLength(50);
                entity.HasIndex(e => e.IdNo).IsUnique();
                entity.Property(e => e.Address).HasMaxLength(1000);
                entity.Property(e => e.Phone).HasMaxLength(20);
                
                // Relationship with User
                entity.HasOne(e => e.User)
                      .WithOne(e => e.Customer)
                      .HasForeignKey<Customer>(e => e.UserId)
                      .OnDelete(DeleteBehavior.SetNull);
                entity.Property(e => e.UserId).HasColumnName("User_Id");
            });
            
            // Seed data for admin user
            modelBuilder.Entity<User>().HasData(new User
            {
                Id = 1,
                Name = "Admin User",
                Email = "admin@insurance.com",
                Role = "Admin",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
                CreatedAt = DateTime.UtcNow
            });
        }
    }
} 