using Microsoft.EntityFrameworkCore;
using SharedLife.Models.Entities;
using SharedLife.Models.Enums;
using SharedLife.Utilities;

namespace SharedLife.Data;

public static class DbInitializer
{
    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<ApplicationDbContext>>();

        try
        {
            // Try to apply migrations, but handle case where tables already exist
            try
            {
                await context.Database.MigrateAsync();
            }
            catch (Exception migrateEx) when (migrateEx.Message.Contains("already exists") || migrateEx.Message.Contains("Duplicate column"))
            {
                logger.LogWarning("Some tables/columns already exist. Ensuring missing tables are created via raw SQL.");
                // Create DonorOffers table if missing
                try
                {
                    await context.Database.ExecuteSqlRawAsync(@"
                        CREATE TABLE IF NOT EXISTS `DonorOffers` (
                            `Id` int NOT NULL AUTO_INCREMENT,
                            `DonorId` int NOT NULL,
                            `DonationType` varchar(50) NOT NULL,
                            `Quantity` int NOT NULL DEFAULT 1,
                            `HospitalName` varchar(200) NOT NULL,
                            `HospitalLocation` varchar(500) NOT NULL,
                            `City` varchar(100) NOT NULL,
                            `PreferredDate` datetime(6) NOT NULL,
                            `Notes` varchar(500) NULL,
                            `Status` varchar(50) NOT NULL,
                            `CreatedAt` datetime(6) NOT NULL,
                            `UpdatedAt` datetime(6) NULL,
                            PRIMARY KEY (`Id`),
                            CONSTRAINT `FK_DonorOffers_Donors_DonorId` FOREIGN KEY (`DonorId`) REFERENCES `Donors` (`Id`) ON DELETE CASCADE
                        ) CHARACTER SET utf8mb4;
                    ");
                }
                catch (Exception tableEx) when (tableEx.Message.Contains("already exists"))
                {
                    logger.LogInformation("DonorOffers table already exists");
                }
            }

            // Seed admin user
            await SeedAdminUserAsync(context, logger);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while seeding the database");
            throw;
        }
    }

    private static async Task SeedAdminUserAsync(ApplicationDbContext context, ILogger logger)
    {
        const string adminEmail = "admin.admin@gmail.com";
        const string adminPassword = "Admin@123";
        const string adminFullName = "System Administrator";

        // Check if admin already exists
        var existingAdmin = await context.Users.FirstOrDefaultAsync(u => u.Email == adminEmail);
        
        if (existingAdmin != null)
        {
            logger.LogInformation("Admin user already exists: {Email}", adminEmail);
            return;
        }

        // Create admin user
        var adminUser = new User
        {
            Email = adminEmail,
            PasswordHash = PasswordHasher.HashPassword(adminPassword),
            FullName = adminFullName,
            PhoneNumber = "1234567890",
            Role = UserRole.Admin,
            IsActive = true,
            IsVerified = true,
            CreatedAt = TimeHelper.Now,
            UpdatedAt = TimeHelper.Now
        };

        context.Users.Add(adminUser);
        await context.SaveChangesAsync();

        logger.LogInformation("Admin user created successfully: {Email}", adminEmail);
    }
}
