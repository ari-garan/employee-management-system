using EmployeeManagement.Domain.Entities;
using EmployeeManagement.Domain.Enums;
using EmployeeManagement.Domain.Exceptions;
using EmployeeManagement.Domain.ValueObjects;
using EmployeeManagement.Infrastructure.Repositories;
using EmployeeManagement.Services.Calculators;
using EmployeeManagement.Services.DTOs;
using EmployeeManagement.Services.Implementations;
using Xunit;

namespace EmployeeManagement.Tests;

public class LinqAndServiceTests
{
    private readonly InMemoryDepartmentRepository _deptRepo;
    private readonly InMemoryEmployeeRepository _empRepo;
    private readonly EmployeeService _empService;
    private readonly DepartmentService _deptService;
    private readonly Department _engineering;
    private readonly Department _hr;

    public LinqAndServiceTests()
    {
        _deptRepo = new InMemoryDepartmentRepository();
        _empRepo = new InMemoryEmployeeRepository();
        var bonusCalculator = new BonusCalculator();

        _deptService = new DepartmentService(_deptRepo, _empRepo);
        _empService = new EmployeeService(_empRepo, _deptRepo, bonusCalculator);

        _engineering = new Department("Engineering", DepartmentCode.Engineering, 1_000_000m);
        _hr = new Department("Human Resources", DepartmentCode.HumanResources, 300_000m);

        _deptRepo.AddAsync(_engineering).GetAwaiter().GetResult();
        _deptRepo.AddAsync(_hr).GetAwaiter().GetResult();

        // Seed 3 employees
        var e1 = new FullTimeEmployee("Alice", "Brown", "alice@test.com", _engineering.Id, RoleType.SeniorEngineer, new DateOnly(2022, 1, 1), new Compensation(120_000m));
        var e2 = new FullTimeEmployee("Bob", "Clark", "bob@test.com", _engineering.Id, RoleType.SoftwareEngineer, new DateOnly(2023, 1, 1), new Compensation(90_000m));
        var e3 = new FullTimeEmployee("Charlie", "Adams", "charlie@test.com", _hr.Id, RoleType.HrSpecialist, new DateOnly(2021, 1, 1), new Compensation(70_000m));

        _empRepo.AddAsync(e1).GetAwaiter().GetResult();
        _empRepo.AddAsync(e2).GetAwaiter().GetResult();
        _empRepo.AddAsync(e3).GetAwaiter().GetResult();
    }

    [Fact]
    public async Task GetEmployeesAsync_FilterByDepartment_ReturnsOnlyMatchingEmployees()
    {
        // Act
        var result = await _empService.GetEmployeesAsync(new EmployeeFilterDto(DepartmentId: _engineering.Id));

        // Assert
        Assert.Equal(2, result.TotalCount);
        Assert.All(result.Items, e => Assert.Equal("Engineering", e.DepartmentName));
    }

    [Fact]
    public async Task GetEmployeesAsync_SortBySalaryDescending_ReturnsHighestFirst()
    {
        // Act
        var result = await _empService.GetEmployeesAsync(new EmployeeFilterDto(SortBy: "salary", IsDescending: true));

        // Assert
        Assert.Equal(3, result.TotalCount);
        Assert.Equal("Alice Brown", result.Items[0].FullName); // 120,000
        Assert.Equal("Bob Clark", result.Items[1].FullName);   // 90,000
        Assert.Equal("Charlie Adams", result.Items[2].FullName); // 70,000
    }

    [Fact]
    public async Task GetEmployeesAsync_Pagination_ComputesPagesCorrectly()
    {
        // Act
        var page1 = await _empService.GetEmployeesAsync(new EmployeeFilterDto(PageNumber: 1, PageSize: 2));

        // Assert
        Assert.Equal(3, page1.TotalCount);
        Assert.Equal(2, page1.Items.Count);
        Assert.Equal(2, page1.TotalPages);
        Assert.True(page1.HasNextPage);
        Assert.False(page1.HasPreviousPage);
    }

    [Fact]
    public async Task GetDepartmentSummariesAsync_CalculatesAggregatesCorrectly()
    {
        // Act
        var summaries = await _deptService.GetDepartmentSummariesAsync();
        var engSummary = summaries.First(s => s.DepartmentCode == "Engineering");

        // Assert: 120k + 90k = 210k committed. Average = 105k. Min = 90k. Max = 120k.
        Assert.Equal(210_000m, engSummary.TotalCommittedSalaries);
        Assert.Equal(105_000m, engSummary.AverageSalary);
        Assert.Equal(90_000m, engSummary.MinSalary);
        Assert.Equal(120_000m, engSummary.MaxSalary);
        Assert.Equal(2, engSummary.TotalEmployees);
    }

    [Fact]
    public async Task CreateEmployeeAsync_DuplicateEmail_ThrowsValidationException()
    {
        // Arrange
        var dto = new CreateEmployeeDto(
            "Duplicate",
            "Person",
            "alice@test.com", // Existing email
            _engineering.Id,
            RoleType.JuniorEngineer,
            DateOnly.FromDateTime(DateTime.UtcNow),
            75_000m,
            "FullTime");

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => _empService.CreateEmployeeAsync(dto));
    }
}

