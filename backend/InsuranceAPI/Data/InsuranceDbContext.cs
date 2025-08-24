using Microsoft.EntityFrameworkCore;
using InsuranceAPI.Models;

namespace InsuranceAPI.Data
{
    public class InsuranceDbContext : DbContext
    {
        public InsuranceDbContext(DbContextOptions<InsuranceDbContext> options) : base(options)
        {
        }

        // DbSet properties
        public DbSet<User> Users { get; set; }
        public DbSet<Agent> Agents { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<InsuranceType> InsuranceTypes { get; set; }
        public DbSet<Coverage> Coverages { get; set; }
        public DbSet<Offer> Offers { get; set; }
        public DbSet<SelectedCoverage> SelectedCoverages { get; set; }
        public DbSet<Policy> Policies { get; set; }
        public DbSet<Claim> Claims { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Document> Documents { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
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

            // Agent entity configuration
            modelBuilder.Entity<Agent>(entity =>
            {
                entity.ToTable("Agents");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.AgentCode).IsRequired().HasMaxLength(10);
                entity.HasIndex(e => e.AgentCode).IsUnique();
                entity.Property(e => e.Department).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Address).IsRequired().HasMaxLength(500);
                entity.Property(e => e.Phone).IsRequired().HasMaxLength(20);
                
                // Relationship with User
                entity.HasOne(e => e.User)
                      .WithOne(e => e.Agent)
                      .HasForeignKey<Agent>(e => e.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.Property(e => e.UserId).HasColumnName("User_Id");
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
                      .OnDelete(DeleteBehavior.Cascade);
                entity.Property(e => e.UserId).HasColumnName("User_Id");
            });

            // Offer entity configuration
            modelBuilder.Entity<Offer>(entity =>
            {
                entity.ToTable("Offers");
                entity.HasKey(e => e.Id);
                
                // Sigorta Bilgileri
                entity.Property(e => e.InsuranceTypeId).IsRequired();
                entity.HasIndex(e => e.InsuranceTypeId);
                
                // Teklif Detayları
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.BasePrice).HasColumnType("decimal(18,2)").IsRequired();
                entity.Property(e => e.DiscountRate).HasColumnType("decimal(5,2)").HasDefaultValue(0);
                entity.Property(e => e.FinalPrice).HasColumnType("decimal(18,2)").IsRequired();
                entity.Property(e => e.Status).IsRequired().HasMaxLength(50).HasDefaultValue("pending");
                entity.Property(e => e.ValidUntil).IsRequired();
                entity.Property(e => e.CreatedAt).IsRequired().HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.UpdatedAt);
                
                // Relationships
                entity.HasOne(e => e.Customer)
                      .WithMany(e => e.Offers)
                      .HasForeignKey(e => e.CustomerId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.Property(e => e.CustomerId).HasColumnName("Customer_Id");
                
                entity.HasOne(e => e.Agent)
                      .WithMany(e => e.Offers)
                      .HasForeignKey(e => e.AgentId)
                      .OnDelete(DeleteBehavior.NoAction);
                entity.Property(e => e.AgentId).HasColumnName("Agent_Id");
                
                entity.HasOne(e => e.InsuranceType)
                      .WithMany(e => e.Offers)
                      .HasForeignKey(e => e.InsuranceTypeId)
                      .OnDelete(DeleteBehavior.NoAction);
                entity.Property(e => e.InsuranceTypeId).HasColumnName("Insurance_Type_Id");
            });

            // InsuranceType entity configuration
            modelBuilder.Entity<InsuranceType>(entity =>
            {
                entity.ToTable("InsuranceTypes");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Category).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.IsActive).IsRequired().HasDefaultValue(true);
                entity.Property(e => e.BasePrice).HasColumnType("decimal(18,2)").IsRequired();
                entity.Property(e => e.CoverageDetails).HasMaxLength(2000);
                entity.Property(e => e.CreatedAt).IsRequired().HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.UpdatedAt);
                
                // Indexes
                entity.HasIndex(e => e.Category);
                entity.HasIndex(e => e.IsActive);
            });

            // Coverage entity configuration
            modelBuilder.Entity<Coverage>(entity =>
            {
                entity.ToTable("Coverages");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.Limit).HasColumnType("decimal(18,2)").IsRequired();
                entity.Property(e => e.Premium).HasColumnType("decimal(18,2)").IsRequired();
                entity.Property(e => e.IsOptional).IsRequired().HasDefaultValue(false);
                entity.Property(e => e.IsActive).IsRequired().HasDefaultValue(true);
                entity.Property(e => e.InsuranceTypeId).IsRequired();
                entity.Property(e => e.CreatedAt).IsRequired().HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.UpdatedAt);
                
                // Relationships
                entity.HasOne(e => e.InsuranceType)
                      .WithMany(e => e.Coverages)
                      .HasForeignKey(e => e.InsuranceTypeId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.Property(e => e.InsuranceTypeId).HasColumnName("Insurance_Type_Id");
                
                // Indexes
                entity.HasIndex(e => e.InsuranceTypeId);
                entity.HasIndex(e => e.IsActive);
            });

            // SelectedCoverage entity configuration
            modelBuilder.Entity<SelectedCoverage>(entity =>
            {
                entity.ToTable("SelectedCoverages");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.OfferId).IsRequired();
                entity.Property(e => e.CoverageId).IsRequired();
                entity.Property(e => e.SelectedLimit).HasColumnType("decimal(18,2)").IsRequired();
                entity.Property(e => e.Premium).HasColumnType("decimal(18,2)").IsRequired();
                entity.Property(e => e.IsSelected).IsRequired().HasDefaultValue(true);
                entity.Property(e => e.CreatedAt).IsRequired().HasDefaultValueSql("GETDATE()");
                
                // Relationships
                entity.HasOne(e => e.Offer)
                      .WithMany(e => e.SelectedCoverages)
                      .HasForeignKey(e => e.OfferId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.Property(e => e.OfferId).HasColumnName("Offer_Id");
                
                entity.HasOne(e => e.Coverage)
                      .WithMany(e => e.SelectedCoverages)
                      .HasForeignKey(e => e.CoverageId)
                      .OnDelete(DeleteBehavior.NoAction);
                entity.Property(e => e.CoverageId).HasColumnName("Coverage_Id");
                
                // Indexes
                entity.HasIndex(e => e.OfferId);
                entity.HasIndex(e => e.CoverageId);
            });

            // Policy entity configuration
            modelBuilder.Entity<Policy>(entity =>
            {
                entity.ToTable("Policies");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.StartDate).IsRequired();
                entity.Property(e => e.EndDate).IsRequired();
                entity.Property(e => e.PolicyNumber).IsRequired().HasMaxLength(100);
                entity.HasIndex(e => e.PolicyNumber).IsUnique();
                
                // Relationship with Offer
                entity.HasOne(e => e.Offer)
                      .WithOne(e => e.Policy)
                      .HasForeignKey<Policy>(e => e.OfferId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.Property(e => e.OfferId).HasColumnName("Offer_Id");
            });

            // Claim entity configuration
            modelBuilder.Entity<Claim>(entity =>
            {
                entity.ToTable("Claims");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Description).HasMaxLength(1000);
                entity.Property(e => e.Status).IsRequired().HasConversion<string>().HasDefaultValue(ClaimStatus.Pending);
                entity.Property(e => e.Type).IsRequired().HasConversion<string>().HasDefaultValue(ClaimType.Diger);
                entity.Property(e => e.Priority).IsRequired().HasConversion<string>().HasDefaultValue(ClaimPriority.Normal);
                entity.Property(e => e.ClaimAmount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.ApprovedAmount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.CreatedAt).IsRequired().HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.Notes).HasMaxLength(1000);
                
                // Relationships
                entity.HasOne(e => e.Policy)
                      .WithMany(e => e.Claims)
                      .HasForeignKey(e => e.PolicyId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.Property(e => e.PolicyId).HasColumnName("Policy_Id");
                
                entity.HasOne(e => e.CreatedByUser)
                      .WithMany(e => e.ReportedClaims)
                      .HasForeignKey(e => e.CreatedByUserId)
                      .OnDelete(DeleteBehavior.NoAction);
                entity.Property(e => e.CreatedByUserId).HasColumnName("Created_By_User_Id");
                
                entity.HasOne(e => e.ProcessedByUser)
                      .WithMany(e => e.ProcessedClaims)
                      .HasForeignKey(e => e.ProcessedByUserId)
                      .OnDelete(DeleteBehavior.NoAction);
                entity.Property(e => e.ProcessedByUserId).HasColumnName("Processed_By_User_Id");
            });

            // Payment entity configuration
            modelBuilder.Entity<Payment>(entity =>
            {
                entity.ToTable("Payments");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Amount).HasColumnType("decimal(18,2)").IsRequired();
                entity.Property(e => e.PaidAt).IsRequired().HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.Method).IsRequired().HasConversion<string>().HasDefaultValue(PaymentMethod.KrediKarti);
                entity.Property(e => e.Status).IsRequired().HasConversion<string>().HasDefaultValue(PaymentStatus.Beklemede);
                entity.Property(e => e.TransactionId).HasMaxLength(100);
                entity.Property(e => e.Notes).HasMaxLength(500);
                entity.Property(e => e.CreatedAt).IsRequired().HasDefaultValueSql("GETDATE()");
                
                // Relationship with Policy
                entity.HasOne(e => e.Policy)
                      .WithMany(e => e.Payments)
                      .HasForeignKey(e => e.PolicyId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.Property(e => e.PolicyId).HasColumnName("Policy_Id");
            });

            // Document entity configuration
            modelBuilder.Entity<Document>(entity =>
            {
                entity.ToTable("Documents");
                entity.HasKey(e => e.Id);
                
                // File Properties
                entity.Property(e => e.FileName).IsRequired().HasMaxLength(255);
                entity.Property(e => e.FileUrl).IsRequired().HasMaxLength(1000);
                entity.Property(e => e.FileType).IsRequired().HasMaxLength(100);
                entity.Property(e => e.FileSize).IsRequired();
                entity.Property(e => e.Category).IsRequired().HasConversion<string>().HasDefaultValue(DocumentCategory.Diger);
                entity.Property(e => e.Status).IsRequired().HasConversion<string>().HasDefaultValue(DocumentStatus.Aktif);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.Version).HasMaxLength(100).HasDefaultValue("1.0");
                
                // Timestamps
                entity.Property(e => e.UploadedAt).IsRequired().HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.UpdatedAt);
                entity.Property(e => e.ExpiresAt);
                
                // Relationships
                entity.HasOne(e => e.Customer)
                      .WithMany(e => e.Documents)
                      .HasForeignKey(e => e.CustomerId)
                      .OnDelete(DeleteBehavior.NoAction);
                entity.Property(e => e.CustomerId).HasColumnName("Customer_Id");
                
                entity.HasOne(e => e.Claim)
                      .WithMany()
                      .HasForeignKey(e => e.ClaimId)
                      .OnDelete(DeleteBehavior.SetNull);
                entity.Property(e => e.ClaimId).HasColumnName("Claim_Id");
                
                entity.HasOne(e => e.Policy)
                      .WithMany()
                      .HasForeignKey(e => e.PolicyId)
                      .OnDelete(DeleteBehavior.NoAction);
                entity.Property(e => e.PolicyId).HasColumnName("Policy_Id");
                
                entity.HasOne(e => e.UploadedByUser)
                      .WithMany()
                      .HasForeignKey(e => e.UploadedByUserId)
                      .OnDelete(DeleteBehavior.NoAction);
                entity.Property(e => e.UploadedByUserId).HasColumnName("Uploaded_By_User_Id");
            });
        }
    }
} 