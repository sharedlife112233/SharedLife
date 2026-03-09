using Microsoft.EntityFrameworkCore;
using SharedLife.Models.Entities;

namespace SharedLife.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) 
        : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<Donor> Donors { get; set; }
    public DbSet<Recipient> Recipients { get; set; }
    public DbSet<Hospital> Hospitals { get; set; }
    public DbSet<DonationRequest> DonationRequests { get; set; }
    public DbSet<DonorRequest> DonorRequests { get; set; }
    public DbSet<ChatMessage> ChatMessages { get; set; }
    public DbSet<DonorOffer> DonorOffers { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
            entity.Property(e => e.PasswordHash).IsRequired();
            entity.Property(e => e.FullName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.PhoneNumber).HasMaxLength(20);
            entity.Property(e => e.Address).HasMaxLength(500);
            entity.Property(e => e.Role).HasConversion<string>();
            entity.Property(e => e.BloodGroup).HasConversion<string>();
        });

        // RefreshToken configuration
        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Token).IsRequired();
            entity.HasOne(e => e.User)
                .WithMany(u => u.RefreshTokens)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Donor configuration
        modelBuilder.Entity<Donor>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.UserId).IsUnique();
            entity.Property(e => e.BloodGroup).HasConversion<string>();
            entity.Property(e => e.Status).HasConversion<string>();
            entity.Property(e => e.PledgedOrgans).HasMaxLength(500);
            entity.Property(e => e.ChronicDiseaseDetails).HasMaxLength(1000);
            entity.Property(e => e.Medications).HasMaxLength(1000);
            entity.Property(e => e.Allergies).HasMaxLength(500);
            entity.Property(e => e.AvailabilityNotes).HasMaxLength(500);
            entity.Property(e => e.EmergencyContactName).HasMaxLength(100);
            entity.Property(e => e.EmergencyContactPhone).HasMaxLength(20);
            entity.Property(e => e.EmergencyContactRelation).HasMaxLength(50);
            
            entity.HasOne(e => e.User)
                .WithOne()
                .HasForeignKey<Donor>(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Recipient configuration
        modelBuilder.Entity<Recipient>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.UserId).IsUnique();
            entity.Property(e => e.BloodGroup).HasConversion<string>();
            entity.Property(e => e.MedicalCondition).HasMaxLength(1000);
            entity.Property(e => e.HospitalName).HasMaxLength(200);
            entity.Property(e => e.HospitalAddress).HasMaxLength(500);
            entity.Property(e => e.City).HasMaxLength(100);
            entity.Property(e => e.EmergencyContactName).HasMaxLength(100);
            entity.Property(e => e.EmergencyContactPhone).HasMaxLength(20);
            entity.Property(e => e.EmergencyContactRelation).HasMaxLength(50);
            
            entity.HasOne(e => e.User)
                .WithOne()
                .HasForeignKey<Recipient>(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // DonationRequest configuration
        modelBuilder.Entity<DonationRequest>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.BloodGroup).HasConversion<string>();
            entity.Property(e => e.DonationType).HasConversion<string>();
            entity.Property(e => e.UrgencyLevel).HasConversion<string>();
            entity.Property(e => e.Status).HasConversion<string>();
            entity.Property(e => e.HospitalName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.HospitalLocation).IsRequired().HasMaxLength(500);
            entity.Property(e => e.City).IsRequired().HasMaxLength(100);
            entity.Property(e => e.ContactName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.ContactPhone).IsRequired().HasMaxLength(20);
            entity.Property(e => e.MedicalNotes).HasMaxLength(1000);
            entity.Property(e => e.AdditionalRequirements).HasMaxLength(500);
            
            entity.HasOne(e => e.Recipient)
                .WithMany(r => r.DonationRequests)
                .HasForeignKey(e => e.RecipientId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // DonorRequest configuration
        modelBuilder.Entity<DonorRequest>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Status).HasConversion<string>();
            entity.Property(e => e.ResponseNotes).HasMaxLength(500);
            
            entity.HasOne(e => e.DonationRequest)
                .WithMany(dr => dr.DonorRequests)
                .HasForeignKey(e => e.DonationRequestId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasOne(e => e.Donor)
                .WithMany()
                .HasForeignKey(e => e.DonorId)
                .OnDelete(DeleteBehavior.Restrict);
            
            // Ensure a donor can only respond once to a request
            entity.HasIndex(e => new { e.DonationRequestId, e.DonorId }).IsUnique();
        });

        // Hospital configuration
        modelBuilder.Entity<Hospital>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.UserId).IsUnique();
            entity.HasIndex(e => e.RegistrationNumber).IsUnique();
            entity.Property(e => e.HospitalName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.RegistrationNumber).IsRequired().HasMaxLength(100);
            entity.Property(e => e.HospitalType).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Address).IsRequired().HasMaxLength(500);
            entity.Property(e => e.City).IsRequired().HasMaxLength(100);
            entity.Property(e => e.State).IsRequired().HasMaxLength(100);
            entity.Property(e => e.PinCode).IsRequired().HasMaxLength(10);
            entity.Property(e => e.ContactEmail).IsRequired().HasMaxLength(255);
            entity.Property(e => e.ContactPhone).IsRequired().HasMaxLength(20);
            entity.Property(e => e.AlternatePhone).HasMaxLength(20);
            entity.Property(e => e.Website).HasMaxLength(200);
            entity.Property(e => e.OperatingHours).HasMaxLength(200);
            entity.Property(e => e.VerificationNotes).HasMaxLength(500);
            
            entity.HasOne(e => e.User)
                .WithOne()
                .HasForeignKey<Hospital>(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ChatMessage configuration
        modelBuilder.Entity<ChatMessage>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Content).IsRequired().HasMaxLength(1000);
            
            entity.HasOne(e => e.DonorRequest)
                .WithMany()
                .HasForeignKey(e => e.DonorRequestId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasOne(e => e.SenderUser)
                .WithMany()
                .HasForeignKey(e => e.SenderUserId)
                .OnDelete(DeleteBehavior.Restrict);
            
            entity.HasIndex(e => e.DonorRequestId);
            entity.HasIndex(e => new { e.DonorRequestId, e.CreatedAt });
        });

        // DonorOffer configuration
        modelBuilder.Entity<DonorOffer>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.DonationType).HasConversion<string>();
            entity.Property(e => e.Status).HasConversion<string>();
            entity.Property(e => e.HospitalName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.HospitalLocation).IsRequired().HasMaxLength(500);
            entity.Property(e => e.City).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Notes).HasMaxLength(500);
            
            entity.HasOne(e => e.Donor)
                .WithMany()
                .HasForeignKey(e => e.DonorId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
