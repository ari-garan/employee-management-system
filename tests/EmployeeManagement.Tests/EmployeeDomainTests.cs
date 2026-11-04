using EmployeeManagement.Domain.Entities;
using EmployeeManagement.Domain.Enums;
using EmployeeManagement.Domain.Exceptions;
using EmployeeManagement.Domain.ValueObjects;
using Xunit;

namespace EmployeeManagement.Tests;

public class EmployeeDomainTests
{
    [Fact]
    public void FullTimeEmployee_CalculateAnnualBonus_UsesTargetPercentageAndMultiplier()
    {
        // Arrange
        var emp = new FullTimeEmployee(
            "Alice",
            "Smith",
            "alice.smith@enterprise.io",
            Guid.NewGuid(),
            RoleType.SeniorEngineer,
            new DateOnly(2022, 1, 1),
            new Compensation(100_000m),
            targetBonusPercentage: 0.10m);

        // Act
        var bonus = emp.CalculateAnnualBonus(companyMultiplier: 1.2m);

        // Assert: 100,000 * 0.10 * 1.2 = 12,000
        Assert.Equal(12_000m, bonus);
    }

    [Fact]
    public void Manager_CalculateAnnualBonus_IncludesAllowanceAndTeamIncentive()
    {
        // Arrange
        var mgr = new Manager(
            "Sarah",
            "Connor",
            "sarah.connor@enterprise.io",
            Guid.NewGuid(),
            RoleType.EngineeringManager,
            new DateOnly(2021, 1, 1),
            new Compensation(120_000m),
            targetBonusPercentage: 0.20m,
            managementAllowance: 10_000m);

        mgr.AddDirectReport(Guid.NewGuid());
        mgr.AddDirectReport(Guid.NewGuid());

        // Act: (120,000 * 0.20 * 1.0) + 10,000 + (2 * 500) = 24,000 + 10,000 + 1,000 = 35,000
        var bonus = mgr.CalculateAnnualBonus(companyMultiplier: 1.0m);

        // Assert
        Assert.Equal(35_000m, bonus);
    }

    [Fact]
    public void Contractor_CalculateAnnualBonus_IsFlatAndIgnoresCompanyMultiplier()
    {
        // Arrange
        var contractor = new ContractEmployee(
            "David",
            "Kim",
            "david.kim@consulting.com",
            Guid.NewGuid(),
            RoleType.SoftwareEngineer,
            new DateOnly(2024, 1, 1),
            hourlyRate: 50.00m,
            contractEndDate: new DateOnly(2024, 12, 31),
            agencyName: "TechStaff");

        // Annual base = 50 * 2080 = 104,000. Flat 3% = 3,120
        var bonusWithStandardMultiplier = contractor.CalculateAnnualBonus(1.0m);
        var bonusWithHighMultiplier = contractor.CalculateAnnualBonus(2.0m);

        // Assert
        Assert.Equal(3_120m, bonusWithStandardMultiplier);
        Assert.Equal(3_120m, bonusWithHighMultiplier);
    }

    [Fact]
    public void Employee_InvalidEmail_ThrowsValidationException()
    {
        // Act & Assert
        Assert.Throws<ValidationException>(() => new FullTimeEmployee(
            "John",
            "Doe",
            "invalid-email-string",
            Guid.NewGuid(),
            RoleType.JuniorEngineer,
            DateOnly.FromDateTime(DateTime.UtcNow),
            new Compensation(60_000m)));
    }

    [Fact]
    public void Department_AssignEmployee_ExceedingBudget_ThrowsBudgetExceededException()
    {
        // Arrange: Budget = 100,000
        var dept = new Department("Small Dept", DepartmentCode.Marketing, 100_000m);
        var emp = new FullTimeEmployee(
            "Bob",
            "BigEarner",
            "bob@enterprise.io",
            dept.Id,
            RoleType.Director,
            DateOnly.FromDateTime(DateTime.UtcNow),
            new Compensation(120_000m));

        // Act & Assert
        Assert.Throws<BudgetExceededException>(() => dept.AssignEmployee(emp));
    }

    [Fact]
    public void Address_Record_SupportsValueEquality()
    {
        // Arrange
        var addr1 = new Address("123 Main St", "Austin", "TX", "78701");
        var addr2 = new Address("123 Main St", "Austin", "TX", "78701");
        var addr3 = new Address("456 Other St", "Austin", "TX", "78701");

        // Assert: C# records provide value equality out-of-the-box
        Assert.Equal(addr1, addr2);
        Assert.NotEqual(addr1, addr3);
    }
}

