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
            // Ensure database is created and migrations are applied
            await context.Database.MigrateAsync();

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
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        context.Users.Add(adminUser);
        await context.SaveChangesAsync();

        logger.LogInformation("Admin user created successfully: {Email}", adminEmail);
    }
}
