using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SharedLife.Data;
using SharedLife.Models.Responses;
using SharedLife.Services;
using SharedLife.Services.Interfaces;
using SharedLife.Utilities;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

// Configure OpenAPI/Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "SharedLife API", 
        Version = "v1",
        Description = "Donation Management System API"
    });
    
    // Add JWT Authentication to Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter 'Bearer' [space] and then your token in the text input below.\n\nExample: \"Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...\""
    });
    
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Configure MySQL Database
var connectionString = ResolveMySqlConnectionString(builder.Configuration);
var mySqlServerVersion = ResolveMySqlServerVersion(builder.Configuration);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(connectionString, new MySqlServerVersion(mySqlServerVersion)));

// Configure JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

// Configure CORS
var allowedOrigins = ResolveAllowedOrigins(builder.Configuration, builder.Environment);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// Register Services
builder.Services.AddScoped<JwtHelper>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IDonorService, DonorService>();
builder.Services.AddScoped<IRecipientService, RecipientService>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<IHospitalService, HospitalService>();
builder.Services.AddScoped<IChatService, ChatService>();

var app = builder.Build();

var startupLogger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("Startup");
startupLogger.LogInformation("Using MySQL server version {Version}", mySqlServerVersion);
startupLogger.LogInformation("CORS allowed origins: {Origins}", string.Join(", ", allowedOrigins));

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "SharedLife API v1");
        c.RoutePrefix = string.Empty; // Swagger at root
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowFrontend");
app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (Exception ex)
    {
        var logger = context.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger("GlobalException");
        logger.LogError(ex, "Unhandled exception for {Method} {Path}", context.Request.Method, context.Request.Path);

        if (context.Response.HasStarted)
        {
            throw;
        }

        context.Response.Clear();
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(ApiResponse<object>.ErrorResponse("An internal server error occurred."));
    }
});

// Serve uploaded documents
var uploadsPath = Path.Combine(app.Environment.ContentRootPath, "uploads");
if (!Directory.Exists(uploadsPath))
    Directory.CreateDirectory(uploadsPath);

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(uploadsPath),
    RequestPath = "/uploads"
});

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Apply migrations and seed admin user on startup
using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("DatabaseInitialization");
    try
    {
        await DbInitializer.InitializeAsync(app.Services);
        logger.LogInformation("Database initialization completed.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Database initialization failed. Authentication endpoints may fail until database connectivity is restored.");
    }
}

await app.RunAsync();

static string ResolveMySqlConnectionString(IConfiguration configuration)
{
    var configuredConnection = configuration.GetConnectionString("DefaultConnection");
    var host = configuration["MYSQLHOST"];
    var port = configuration["MYSQLPORT"];
    var database = configuration["MYSQLDATABASE"];
    var user = configuration["MYSQLUSER"];
    var password = configuration["MYSQLPASSWORD"];

    var hasRailwayMySqlVars =
        !string.IsNullOrWhiteSpace(host) &&
        !string.IsNullOrWhiteSpace(port) &&
        !string.IsNullOrWhiteSpace(database) &&
        !string.IsNullOrWhiteSpace(user);

    if (hasRailwayMySqlVars)
    {
        return $"Server={host};Port={port};Database={database};User={user};Password={password};SslMode=Preferred;";
    }

    if (!string.IsNullOrWhiteSpace(configuredConnection))
    {
        return configuredConnection;
    }

    throw new InvalidOperationException("No MySQL connection string configured. Set ConnectionStrings__DefaultConnection or Railway MYSQL* environment variables.");
}

static Version ResolveMySqlServerVersion(IConfiguration configuration)
{
    var configuredVersion = configuration["MySql:ServerVersion"];
    if (Version.TryParse(configuredVersion, out var parsed))
    {
        return parsed;
    }

    return new Version(8, 0, 36);
}

static string[] ResolveAllowedOrigins(IConfiguration configuration, IWebHostEnvironment environment)
{
    var defaultProductionOrigins = new[]
    {
        "https://sharedlife.me",
        "https://www.sharedlife.me",
        "https://sharedlife-virid.vercel.app"
    };

    var defaultDevelopmentOrigins = new[]
    {
        "http://localhost:3000",
        "http://localhost:3001",
        "http://localhost:3002",
        "http://localhost:5173",
        "http://localhost:5014"
    };

    var configuredAllowedOrigins = configuration.GetSection("AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();

    // Railway/hosted platforms commonly provide comma-separated env vars.
    var envAllowedOriginsRaw =
        Environment.GetEnvironmentVariable("AllowedOrigins") ??
        Environment.GetEnvironmentVariable("ALLOWED_ORIGINS") ??
        string.Empty;

    var envAllowedOrigins = envAllowedOriginsRaw
        .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    var defaults = environment.IsDevelopment()
        ? defaultProductionOrigins.Concat(defaultDevelopmentOrigins)
        : defaultProductionOrigins;

    var allowedOrigins = configuredAllowedOrigins
        .Concat(envAllowedOrigins)
        .Concat(defaults)
        .Where(origin => !string.IsNullOrWhiteSpace(origin))
        .Select(origin => origin.Trim().TrimEnd('/'))
        .Distinct(StringComparer.OrdinalIgnoreCase)
        .ToArray();

    if (allowedOrigins.Length == 0)
    {
        throw new InvalidOperationException("No CORS origins configured. Set AllowedOrigins in configuration or ALLOWED_ORIGINS in environment.");
    }

    return allowedOrigins;
}