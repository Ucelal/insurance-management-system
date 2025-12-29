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
        public DbSet<Agent> Agents { get; set; }
        public DbSet<InsuranceType> InsuranceTypes { get; set; }
        public DbSet<Coverage> Coverages { get; set; }
        public DbSet<Offer> Offers { get; set; }
        public DbSet<SelectedCoverage> SelectedCoverages { get; set; }
        public DbSet<Policy> Policies { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Claim> Claims { get; set; }
        public DbSet<Document> Documents { get; set; }
        public DbSet<TokenBlacklist> TokenBlacklist { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User configuration
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.UserId);
                entity.Property(e => e.UserId).HasColumnName("User_Id");
                entity.Property(e => e.PasswordHash).HasColumnName("Password_Hash");
                entity.Property(e => e.CreatedAt).HasColumnName("Created_At");
            });

            // Customer configuration
            modelBuilder.Entity<Customer>(entity =>
            {
                entity.HasKey(e => e.CustomerId);
                entity.Property(e => e.CustomerId).HasColumnName("Customer_Id");
                entity.Property(e => e.IdNo).HasColumnName("Id_No");
                entity.Property(e => e.UserId).HasColumnName("User_Id");
                
                entity.HasOne(e => e.User)
                    .WithOne(e => e.Customer)
                    .HasForeignKey<Customer>(e => e.UserId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // Agent configuration
            modelBuilder.Entity<Agent>(entity =>
            {
                entity.HasKey(e => e.AgentId);
                entity.Property(e => e.AgentId).HasColumnName("Agent_Id");
                entity.Property(e => e.AgentCode).HasColumnName("Agent_Code");
                entity.Property(e => e.UserId).HasColumnName("User_Id");
                
                entity.HasOne(e => e.User)
                    .WithOne(e => e.Agent)
                    .HasForeignKey<Agent>(e => e.UserId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // InsuranceType configuration
            modelBuilder.Entity<InsuranceType>(entity =>
            {
                entity.HasKey(e => e.InsuranceTypeId);
                entity.Property(e => e.InsuranceTypeId).HasColumnName("Ins_Type_Id");
                entity.Property(e => e.BasePrice).HasPrecision(18, 2);
                entity.Property(e => e.ValidityPeriodDays).HasColumnName("Validity_Period_Days");
                entity.Property(e => e.CoverageDetails).HasColumnName("Coverage_Details");
                entity.Property(e => e.CreatedAt).HasColumnName("Created_At");
                entity.Property(e => e.UpdatedAt).HasColumnName("Updated_At");
                entity.Property(e => e.UserId).HasColumnName("User_Id");
            });

            // Coverage configuration
            modelBuilder.Entity<Coverage>(entity =>
            {
                entity.HasKey(e => e.CoverageId);
                entity.Property(e => e.CoverageId).HasColumnName("Coverage_Id");
                entity.Property(e => e.BasePremium).HasColumnName("Base_Premium").HasPrecision(18, 2);
                entity.Property(e => e.InsuranceTypeId).HasColumnName("Insurance_Type_Id");
                
                entity.HasOne(e => e.InsuranceType)
                    .WithMany(e => e.Coverages)
                    .HasForeignKey(e => e.InsuranceTypeId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // Offer configuration
            modelBuilder.Entity<Offer>(entity =>
            {
                entity.HasKey(e => e.OfferId);
                entity.Property(e => e.OfferId).HasColumnName("Offer_Id");
                entity.Property(e => e.BasePrice).HasColumnName("Base_Price").HasPrecision(18, 2);
                entity.Property(e => e.FinalPrice).HasColumnName("Final_Price").HasPrecision(18, 2);
                entity.Property(e => e.DiscountRate).HasColumnName("Discount_Rate").HasPrecision(5, 2);
                entity.Property(e => e.CoverageAmount).HasColumnName("Coverage_Amount").HasPrecision(18, 2);
                entity.Property(e => e.CustomerAdditionalInfo).HasColumnName("Customer_Additional_Info");
                entity.Property(e => e.RequestedStartDate).HasColumnName("Requested_Start_Date");
                entity.Property(e => e.ValidUntil).HasColumnName("Valid_Until");
                entity.Property(e => e.IsCustomerApproved).HasColumnName("Is_Customer_Approved");
                entity.Property(e => e.CustomerApprovedAt).HasColumnName("Customer_Approved_At");
                entity.Property(e => e.ReviewedAt).HasColumnName("Reviewed_At");
                entity.Property(e => e.ReviewedBy).HasColumnName("Reviewed_By");
                entity.Property(e => e.CreatedAt).HasColumnName("Created_At");
                entity.Property(e => e.UpdatedAt).HasColumnName("Updated_At");
                entity.Property(e => e.PolicyPdfUrl).HasColumnName("Policy_Pdf_Url");
                entity.Property(e => e.AdminNotes).HasColumnName("Admin_Notes");
                entity.Property(e => e.RejectionReason).HasColumnName("Rejection_Reason");
                entity.Property(e => e.CustomerId).HasColumnName("Customer_Id");
                entity.Property(e => e.AgentId).HasColumnName("Agent_Id");
                entity.Property(e => e.InsuranceTypeId).HasColumnName("Insurance_Type_Id");
                entity.Property(e => e.CreatedBy).HasColumnName("Created_By");
                
                entity.HasOne(e => e.Customer)
                    .WithMany(e => e.Offers)
                    .HasForeignKey(e => e.CustomerId)
                    .OnDelete(DeleteBehavior.SetNull);
                
                entity.HasOne(e => e.Agent)
                    .WithMany(e => e.Offers)
                    .HasForeignKey(e => e.AgentId)
                    .OnDelete(DeleteBehavior.SetNull);
                
                entity.HasOne(e => e.InsuranceType)
                    .WithMany(e => e.Offers)
                    .HasForeignKey(e => e.InsuranceTypeId)
                    .OnDelete(DeleteBehavior.SetNull);
                
                entity.HasOne(e => e.ReviewedByAgent)
                    .WithMany()
                    .HasForeignKey(e => e.ReviewedBy)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // SelectedCoverage configuration
            modelBuilder.Entity<SelectedCoverage>(entity =>
            {
                entity.HasKey(e => e.SelectedCoverageId);
                entity.Property(e => e.SelectedCoverageId).HasColumnName("Sel_Cov_Id");
                entity.Property(e => e.OfferId).HasColumnName("Offer_Id");
                entity.Property(e => e.CoverageId).HasColumnName("Coverage_Id");
                
                entity.HasOne(e => e.Offer)
                    .WithMany(e => e.SelectedCoverages)
                    .HasForeignKey(e => e.OfferId)
                    .OnDelete(DeleteBehavior.SetNull);
                
                entity.HasOne(e => e.Coverage)
                    .WithMany(e => e.SelectedCoverages)
                    .HasForeignKey(e => e.CoverageId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // Policy configuration
            modelBuilder.Entity<Policy>(entity =>
            {
                entity.HasKey(e => e.PolicyId);
                entity.Property(e => e.PolicyId).HasColumnName("Policy_Id");
                entity.Property(e => e.PolicyNumber).HasColumnName("Policy_Number");
                entity.Property(e => e.StartDate).HasColumnName("Start_Date");
                entity.Property(e => e.EndDate).HasColumnName("End_Date");
                entity.Property(e => e.TotalPremium).HasColumnName("Total_Premium").HasPrecision(18, 2);
                entity.Property(e => e.CreatedAt).HasColumnName("Created_At");
                entity.Property(e => e.UpdatedAt).HasColumnName("Updated_At");
                entity.Property(e => e.OfferId).HasColumnName("Offer_Id");
                entity.Property(e => e.InsuranceTypeId).HasColumnName("Ins_Type_Id");
                entity.Property(e => e.AgentId).HasColumnName("Agent_Id");
                
                entity.HasOne(e => e.Offer)
                    .WithMany(e => e.Policies)
                    .HasForeignKey(e => e.OfferId)
                    .OnDelete(DeleteBehavior.SetNull);
                
                entity.HasOne(e => e.InsuranceType)
                    .WithMany()
                    .HasForeignKey(e => e.InsuranceTypeId)
                    .OnDelete(DeleteBehavior.SetNull);
                
                entity.HasOne(e => e.Agent)
                    .WithMany()
                    .HasForeignKey(e => e.AgentId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // Payment configuration
            modelBuilder.Entity<Payment>(entity =>
            {
                entity.HasKey(e => e.PaymentId);
                entity.Property(e => e.PaymentId).HasColumnName("Payment_Id");
                entity.Property(e => e.Amount).HasColumnName("Amount").HasPrecision(18, 2);
                entity.Property(e => e.PaidAt).HasColumnName("Paid_At");
                entity.Property(e => e.CreatedAt).HasColumnName("Created_At");
                entity.Property(e => e.UpdatedAt).HasColumnName("Updated_At");
                entity.Property(e => e.PolicyId).HasColumnName("Policy_Id");
                
                entity.HasOne(e => e.Policy)
                    .WithMany(e => e.Payments)
                    .HasForeignKey(e => e.PolicyId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // Claim configuration
            modelBuilder.Entity<Claim>(entity =>
            {
                entity.HasKey(e => e.ClaimId);
                entity.Property(e => e.ClaimId).HasColumnName("Claim_Id");
                entity.Property(e => e.IncidentDate).HasColumnName("Incident_Date");
                entity.Property(e => e.ProcessedAt).HasColumnName("Processed_At");
                entity.Property(e => e.ApprovedAmount).HasColumnName("Approved_Amount").HasPrecision(18, 2);
                entity.Property(e => e.CreatedAt).HasColumnName("Created_At");
                entity.Property(e => e.UpdatedAt).HasColumnName("Updated_At");
                entity.Property(e => e.PolicyId).HasColumnName("Policy_Id");

                entity.HasOne(e => e.Policy)
                    .WithMany()
                    .HasForeignKey(e => e.PolicyId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // Document configuration
            modelBuilder.Entity<Document>(entity =>
            {
                entity.HasKey(e => e.DocumentId);
                entity.Property(e => e.DocumentId).HasColumnName("Document_Id");
                entity.Property(e => e.FileUrl).HasColumnName("File_Url");
                entity.Property(e => e.FileType).HasColumnName("File_Type");
                entity.Property(e => e.UploadedAt).HasColumnName("Uploaded_At");
                entity.Property(e => e.ExpiresAt).HasColumnName("Expires_At");
                entity.Property(e => e.UpdatedAt).HasColumnName("Updated_At");
                entity.Property(e => e.CustomerId).HasColumnName("Customer_Id");
                entity.Property(e => e.ClaimId).HasColumnName("Claim_Id");
                entity.Property(e => e.PolicyId).HasColumnName("Policy_Id");
                entity.Property(e => e.UploadedByUserId).HasColumnName("Uploaded_By_User_Id");
                
                entity.HasOne(e => e.Customer)
                    .WithMany(e => e.Documents)
                    .HasForeignKey(e => e.CustomerId)
                    .OnDelete(DeleteBehavior.SetNull);
                
                entity.HasOne(e => e.Claim)
                    .WithMany(e => e.Documents)
                    .HasForeignKey(e => e.ClaimId)
                    .OnDelete(DeleteBehavior.SetNull);
                
                entity.HasOne(e => e.Policy)
                    .WithMany(e => e.Documents)
                    .HasForeignKey(e => e.PolicyId)
                    .OnDelete(DeleteBehavior.SetNull);
                
                entity.HasOne(e => e.UploadedByUser)
                    .WithMany(e => e.Documents)
                    .HasForeignKey(e => e.UploadedByUserId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // TokenBlacklist configuration
            modelBuilder.Entity<TokenBlacklist>(entity =>
            {
                entity.HasKey(e => e.TokenBlacklistId);
                entity.Property(e => e.TokenBlacklistId).HasColumnName("Token_black_Id");
                entity.Property(e => e.CreatedAt).HasColumnName("Created_At");
            });
        }
    }
} 