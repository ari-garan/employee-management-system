using EmployeeManagement.Domain.Entities;
using EmployeeManagement.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagement.Infrastructure.Persistence;

/// <summary>
/// Day 9 Deliverable: Entity Framework Core DbContext managing relational database persistence,
/// entity mapping with Fluent API, Table-Per-Hierarchy (TPH) inheritance, and owned Value Objects.
/// </summary>
public class EmployeeManagementDbContext : DbContext
{
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<FullTimeEmployee> FullTimeEmployees => Set<FullTimeEmployee>();
    public DbSet<Manager> Managers => Set<Manager>();
    public DbSet<ContractEmployee> ContractEmployees => Set<ContractEmployee>();

    public EmployeeManagementDbContext(DbContextOptions<EmployeeManagementDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ====================================================================
        // 1. DEPARTMENT CONFIGURATION
        // ====================================================================
        modelBuilder.Entity<Department>(builder =>
        {
            builder.ToTable("Departments");
            builder.HasKey(d => d.Id);

            builder.Property(d => d.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(d => d.Code)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(20);

            builder.Property(d => d.AnnualBudget)
                .HasColumnType("decimal(18,2)");

            // 1-to-many relationship: Department has many Employees
            builder.HasMany(d => d.Employees)
                .WithOne()
                .HasForeignKey(e => e.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Navigation(d => d.Employees)
                .UsePropertyAccessMode(PropertyAccessMode.Field);
        });

        // ====================================================================
        // 2. EMPLOYEE HIERARCHY (Table-Per-Hierarchy / TPH)
        // ====================================================================
        modelBuilder.Entity<Employee>(builder =>
        {
            builder.ToTable("Employees");
            builder.HasKey(e => e.Id);

            builder.Property(e => e.FirstName)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(e => e.LastName)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(e => e.Email)
                .IsRequired()
                .HasMaxLength(150);

            builder.HasIndex(e => e.Email).IsUnique();

            builder.Property(e => e.Role)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(30);

            builder.Property(e => e.Status)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(20);

            // TPH Discriminator configuration
            builder.HasDiscriminator<string>("Discriminator")
                .HasValue<FullTimeEmployee>("FullTime")
                .HasValue<Manager>("Manager")
                .HasValue<ContractEmployee>("Contractor");

            // Value Object 1: Compensation (readonly record struct)
            builder.OwnsOne(e => e.Compensation, compBuilder =>
            {
                compBuilder.Property(c => c.BaseSalary)
                    .HasColumnName("BaseSalary")
                    .HasColumnType("decimal(18,2)");

                compBuilder.Property(c => c.Currency)
                    .HasColumnName("Currency")
                    .HasMaxLength(10)
                    .HasDefaultValue("USD");
            });

            // Value Object 2: ResidentialAddress (record)
            builder.OwnsOne(e => e.ResidentialAddress, addrBuilder =>
            {
                addrBuilder.Property(a => a.Street).HasColumnName("Street").HasMaxLength(150);
                addrBuilder.Property(a => a.City).HasColumnName("City").HasMaxLength(60);
                addrBuilder.Property(a => a.State).HasColumnName("State").HasMaxLength(40);
                addrBuilder.Property(a => a.PostalCode).HasColumnName("PostalCode").HasMaxLength(20);
                addrBuilder.Property(a => a.Country).HasColumnName("Country").HasMaxLength(50);
            });
        });

        modelBuilder.Entity<FullTimeEmployee>(builder =>
        {
            builder.Property(f => f.TargetBonusPercentage)
                .HasColumnType("decimal(5,4)");

            builder.Property(f => f.PaidTimeOffDays);
            builder.Property(f => f.RemainingPtoDays);
        });

        modelBuilder.Entity<Manager>(builder =>
        {
            builder.Property(m => m.ManagementAllowance)
                .HasColumnType("decimal(18,2)");

            builder.Ignore(m => m.DirectReportIds);
        });

        modelBuilder.Entity<ContractEmployee>(builder =>
        {
            builder.Property(c => c.HourlyRate)
                .HasColumnType("decimal(18,2)");

            builder.Property(c => c.AgencyName)
                .HasMaxLength(100);

            builder.Property(c => c.ContractEndDate);
        });
    }
}
