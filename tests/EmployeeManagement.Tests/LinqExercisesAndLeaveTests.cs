using EmployeeManagement.Domain.Entities;
using EmployeeManagement.Domain.Enums;
using EmployeeManagement.Domain.Exceptions;
using EmployeeManagement.Domain.ValueObjects;
using EmployeeManagement.Infrastructure.Repositories;
using EmployeeManagement.Services.Calculators;
using EmployeeManagement.Services.Implementations;
using EmployeeManagement.Services.Practice;
using Xunit;

namespace EmployeeManagement.Tests;

public class LinqExercisesAndLeaveTests
{
    private readonly List<Employee> _sampleEmployees;
    private readonly Guid _deptEng = Guid.NewGuid();
    private readonly Guid _deptSales = Guid.NewGuid();

    public LinqExercisesAndLeaveTests()
    {
        _sampleEmployees = new List<Employee>
        {
            new FullTimeEmployee("Alice", "Brown", "alice@enterprise.io", _deptEng, RoleType.SeniorEngineer, new DateOnly(2021, 1, 1), new Compensation(120_000m), address: new Address("123 Main St", "Seattle", "WA", "98101")),
            new FullTimeEmployee("Bob", "Clark", "bob@enterprise.io", _deptEng, RoleType.SoftwareEngineer, new DateOnly(2022, 6, 1), new Compensation(90_000m), address: new Address("456 Pine St", "Seattle", "WA", "98101")),
            new FullTimeEmployee("Charlie", "Adams", "charlie@enterprise.io", _deptSales, RoleType.MarketingSpecialist, new DateOnly(2020, 3, 15), new Compensation(75_000m), address: new Address("789 Oak Ave", "Austin", "TX", "78701")),
            new Manager("Dana", "Evans", "dana@enterprise.io", _deptEng, RoleType.EngineeringManager, new DateOnly(2019, 1, 1), new Compensation(150_000m), address: new Address("100 Market St", "San Francisco", "CA", "94105"))
        };
    }

    #region Leave Management Tests

    [Fact]
    public void FullTimeEmployee_RequestLeave_DeductsDaysAndSetsStatusToOnLeave()
    {
        // Arrange
        var emp = new FullTimeEmployee("Eve", "Taylor", "eve@enterprise.io", _deptEng, RoleType.SoftwareEngineer, new DateOnly(2023, 1, 1), new Compensation(85_000m), ptoDays: 20);

        // Act
        emp.RequestLeave(5);

        // Assert
        Assert.Equal(15, emp.RemainingPtoDays);
        Assert.Equal(EmploymentStatus.OnLeave, emp.Status);
    }

    [Fact]
    public void FullTimeEmployee_RequestLeave_ExceedingBalance_ThrowsInsufficientLeaveBalanceException()
    {
        // Arrange
        var emp = new FullTimeEmployee("Eve", "Taylor", "eve@enterprise.io", _deptEng, RoleType.SoftwareEngineer, new DateOnly(2023, 1, 1), new Compensation(85_000m), ptoDays: 10);

        // Act & Assert
        var ex = Assert.Throws<InsufficientLeaveBalanceException>(() => emp.RequestLeave(12));
        Assert.Equal(12, ex.RequestedDays);
        Assert.Equal(10, ex.RemainingDays);
    }

    [Fact]
    public void FullTimeEmployee_ReturnFromLeave_ResetsStatusToActive()
    {
        // Arrange
        var emp = new FullTimeEmployee("Eve", "Taylor", "eve@enterprise.io", _deptEng, RoleType.SoftwareEngineer, new DateOnly(2023, 1, 1), new Compensation(85_000m), ptoDays: 20);
        emp.RequestLeave(3);
        Assert.Equal(EmploymentStatus.OnLeave, emp.Status);

        // Act
        emp.ReturnFromLeave();

        // Assert
        Assert.Equal(EmploymentStatus.Active, emp.Status);
    }

    [Fact]
    public async Task EmployeeService_RequestLeaveAsync_UpdatesRepositoryAndThrowsOnContractor()
    {
        // Arrange
        var empRepo = new InMemoryEmployeeRepository();
        var deptRepo = new InMemoryDepartmentRepository();
        var calc = new BonusCalculator();
        var service = new EmployeeService(empRepo, deptRepo, calc);

        var ft = new FullTimeEmployee("Frank", "Miller", "frank@enterprise.io", _deptEng, RoleType.SoftwareEngineer, new DateOnly(2023, 1, 1), new Compensation(90_000m));
        var contractor = new ContractEmployee("Grace", "Hopper", "grace@consulting.com", _deptEng, RoleType.SoftwareEngineer, new DateOnly(2023, 1, 1), 60m, new DateOnly(2024, 1, 1), "StaffingCo");

        await empRepo.AddAsync(ft);
        await empRepo.AddAsync(contractor);

        // Act: Full-time requests 4 days
        await service.RequestLeaveAsync(ft.Id, 4);
        var stored = await empRepo.GetByIdAsync(ft.Id);
        Assert.NotNull(stored);
        var updatedFt = Assert.IsType<FullTimeEmployee>(stored);
        Assert.Equal(16, updatedFt.RemainingPtoDays);
        Assert.Equal(EmploymentStatus.OnLeave, updatedFt.Status);

        // Assert: Contractor requesting leave throws ValidationException
        await Assert.ThrowsAsync<ValidationException>(() => service.RequestLeaveAsync(contractor.Id, 2));
    }

    #endregion

    #region Dedicated LINQ Exercises Tests (8 Queries)

    [Fact]
    public void Linq_Query1_FilterActiveByDepartment_ReturnsMatchingOnly()
    {
        var result = LinqExercises.FilterActiveByDepartment(_sampleEmployees, _deptEng);
        Assert.Equal(3, result.Count);
        Assert.All(result, e => Assert.Equal(_deptEng, e.DepartmentId));
    }

    [Fact]
    public void Linq_Query2_ProjectFormattedBadges_CreatesCorrectStrings()
    {
        var result = LinqExercises.ProjectFormattedEmployeeBadges(_sampleEmployees);
        Assert.Equal(4, result.Count);
        Assert.Contains("[BROWN, ALICE] - SeniorEngineer", result);
        Assert.Contains("[EVANS, DANA] - EngineeringManager", result);
    }

    [Fact]
    public void Linq_Query3_SortBySalaryDescendingThenName_SortsCorrectly()
    {
        var result = LinqExercises.SortBySalaryDescendingThenName(_sampleEmployees);
        Assert.Equal("Dana", result[0].FirstName);   // 150k
        Assert.Equal("Alice", result[1].FirstName);  // 120k
        Assert.Equal("Bob", result[2].FirstName);    // 90k
        Assert.Equal("Charlie", result[3].FirstName);// 75k
    }

    [Fact]
    public void Linq_Query4_GroupAndCountByRole_CountsAccurately()
    {
        var roleCounts = LinqExercises.GroupAndCountByRole(_sampleEmployees);
        Assert.Equal(1, roleCounts[RoleType.EngineeringManager]);
        Assert.Equal(1, roleCounts[RoleType.SeniorEngineer]);
        Assert.Equal(1, roleCounts[RoleType.SoftwareEngineer]);
        Assert.Equal(1, roleCounts[RoleType.MarketingSpecialist]);
    }

    [Fact]
    public void Linq_Query5_CalculateSalaryMetrics_CalculatesAggregates()
    {
        var (total, avg, min, max) = LinqExercises.CalculateSalaryMetrics(_sampleEmployees);
        // 120k + 90k + 75k + 150k = 435k
        Assert.Equal(435_000m, total);
        Assert.Equal(108_750m, avg);
        Assert.Equal(75_000m, min);
        Assert.Equal(150_000m, max);
    }

    [Fact]
    public void Linq_Query6_HasHighEarner_DetectsThresholdCorrectly()
    {
        Assert.True(LinqExercises.HasHighEarner(_sampleEmployees, 150_000m));
        Assert.False(LinqExercises.HasHighEarner(_sampleEmployees, 200_000m));
    }

    [Fact]
    public void Linq_Query7_AllHaveCorporateEmail_ValidatesCorrectly()
    {
        Assert.True(LinqExercises.AllHaveCorporateEmail(_sampleEmployees, "@enterprise.io"));

        var mixedList = new List<Employee>(_sampleEmployees)
        {
            new FullTimeEmployee("Zack", "Guest", "zack@external.org", _deptSales, RoleType.JuniorEngineer, new DateOnly(2023, 1, 1), new Compensation(50_000m))
        };
        Assert.False(LinqExercises.AllHaveCorporateEmail(mixedList, "@enterprise.io"));
    }

    [Fact]
    public void Linq_Query8_GetDistinctCities_ExtractsUniqueSortedCities()
    {
        var cities = LinqExercises.GetDistinctCities(_sampleEmployees);
        // Austin, San Francisco, Seattle
        Assert.Equal(3, cities.Count);
        Assert.Equal("Austin", cities[0]);
        Assert.Equal("San Francisco", cities[1]);
        Assert.Equal("Seattle", cities[2]);
    }

    #endregion
}
