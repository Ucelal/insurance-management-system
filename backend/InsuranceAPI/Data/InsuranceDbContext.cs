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
        public DbSet<Offer> Offers { get; set; }
        public DbSet<Policy> Policies { get; set; }
        public DbSet<Claim> Claims { get; set; }
        public DbSet<Payment> Payments { get; set; }
        
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
            
            // Offer entity configuration
            modelBuilder.Entity<Offer>(entity =>
            {
                entity.ToTable("Offers");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.CustomerId).HasColumnName("Customer_Id");
                entity.Property(e => e.InsuranceType).HasColumnName("Insurance_Type").IsRequired().HasMaxLength(100);
                entity.Property(e => e.Price).IsRequired().HasColumnType("decimal(18,2)");
                entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
                
                // Relationship with Customer
                entity.HasOne(e => e.Customer)
                      .WithMany()
                      .HasForeignKey(e => e.CustomerId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
            
            // Policy entity configuration
            modelBuilder.Entity<Policy>(entity =>
            {
                entity.ToTable("Policies");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.OfferId).HasColumnName("Offer_Id");
                entity.Property(e => e.StartDate).HasColumnName("Start_Date").IsRequired();
                entity.Property(e => e.EndDate).HasColumnName("End_Date").IsRequired();
                entity.Property(e => e.PolicyNumber).HasColumnName("Policy_Number").IsRequired().HasMaxLength(100);
                entity.HasIndex(e => e.PolicyNumber).IsUnique();
                
                // Relationship with Offer
                entity.HasOne(e => e.Offer)
                      .WithOne(e => e.Policy)
                      .HasForeignKey<Policy>(e => e.OfferId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Claim entity configuration
            modelBuilder.Entity<Claim>(entity =>
            {
                entity.ToTable("Claims");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.PolicyId).HasColumnName("Policy_Id");
                entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
                entity.Property(e => e.CreatedAt).HasColumnName("Created_At").HasDefaultValueSql("GETDATE()");

                entity.HasOne(e => e.Policy)
                      .WithMany(e => e.Claims)
                      .HasForeignKey(e => e.PolicyId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Payment entity configuration
            modelBuilder.Entity<Payment>(entity =>
            {
                entity.ToTable("Payments");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.PolicyId).HasColumnName("Policy_Id");
                entity.Property(e => e.Amount).HasColumnType("decimal(18,2)").IsRequired();
                entity.Property(e => e.PaidAt).HasColumnName("Paid_At").HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.Method).IsRequired().HasMaxLength(50);

                entity.HasOne(e => e.Policy)
                      .WithMany(e => e.Payments)
                      .HasForeignKey(e => e.PolicyId)
                      .OnDelete(DeleteBehavior.Cascade);
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