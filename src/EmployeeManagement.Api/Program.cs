using System.Text;
using EmployeeManagement.Api.Auth;
using EmployeeManagement.Api.Middleware;
using EmployeeManagement.Domain.Interfaces;
using EmployeeManagement.Infrastructure.Data;
using EmployeeManagement.Infrastructure.Persistence;
using EmployeeManagement.Infrastructure.Repositories;
using EmployeeManagement.Services.Calculators;
using EmployeeManagement.Services.Implementations;
using EmployeeManagement.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// ============================================================================
// 1. DATABASE & PERSISTENCE (Day 9 Deliverable: EF Core SQLite Integration)
// ============================================================================
builder.Services.AddDbContext<EmployeeManagementDbContext>(options =>
{
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") 
        ?? "Data Source=employeemanagement.db");
});

// Repositories backed by EF Core (Scoped to HTTP request)
builder.Services.AddScoped<IEmployeeRepository, EfEmployeeRepository>();
builder.Services.AddScoped<IDepartmentRepository, EfDepartmentRepository>();

// ============================================================================
// 2. DEPENDENCY INJECTION LIFETIMES (Day 7 Deliverable)
// ============================================================================

// --- SCOPED LIFETIME ---
builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddScoped<IDepartmentService, DepartmentService>();

// --- TRANSIENT LIFETIME ---
builder.Services.AddTransient<IBonusCalculator, BonusCalculator>();

// --- SINGLETON / SECURITY LIFETIME ---
builder.Services.AddSingleton<IJwtTokenService, JwtTokenService>();

// ============================================================================
// 3. API SECURITY: JWT BEARER AUTHENTICATION & RBAC (Day 10 Deliverable)
// ============================================================================
var jwtSettings = builder.Configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>() ?? new JwtSettings();
builder.Services.AddSingleton(jwtSettings);

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
        ValidIssuer = jwtSettings.Issuer,
        ValidAudience = jwtSettings.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey))
    };
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("ManagerOrAdmin", policy => policy.RequireRole("Admin", "Manager"));
});

// ============================================================================
// 4. CONTROLLERS & SWAGGER / OPENAPI SPECIFICATION (Days 6 & 10)
// ============================================================================
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Employee Management Web API",
        Version = "v1",
        Description = "Production ASP.NET Core Web API with EF Core & JWT Security (.NET Engineering Bridge Week 2)",
        Contact = new OpenApiContact
        {
            Name = "Engineering Enablement Team",
            Email = "engineering-enablement@enterprise.io"
        }
    });

    // Add JWT Bearer Security Definition in Swagger UI
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Format: 'Bearer {token}'",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
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

var app = builder.Build();

// ============================================================================
// 5. MIDDLEWARE PIPELINE ORDERING (Day 7 Deliverable)
// ============================================================================

// 1. Centralized exception handling
app.UseGlobalExceptionHandler();

// 2. Custom request & response metrics logging
app.UseRequestLogging();

// 3. API documentation
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Employee Management API v1");
        c.RoutePrefix = string.Empty; // Swagger UI at root "/"
    });
}

app.UseHttpsRedirection();

// 4. Authentication (Who are you?) MUST be before Authorization (What can you do?)
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// ============================================================================
// 6. DATABASE MIGRATION & SEEDING ON STARTUP (Day 9 Deliverable)
// ============================================================================
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();

    try
    {
        var dbContext = services.GetRequiredService<EmployeeManagementDbContext>();
        logger.LogInformation("Ensuring relational database schema is created...");
        await dbContext.Database.EnsureCreatedAsync();

        var deptRepo = services.GetRequiredService<IDepartmentRepository>();
        var empRepo = services.GetRequiredService<IEmployeeRepository>();

        logger.LogInformation("Seeding initial corporate departments and employees...");
        await DataSeeder.SeedAsync(deptRepo, empRepo);
        logger.LogInformation("Database migration & seeding completed successfully.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred during database migration/seeding on startup.");
    }
}

app.Run();

// Partial class declaration for WebApplicationFactory integration testing (Week 3 readiness)
public partial class Program { }
