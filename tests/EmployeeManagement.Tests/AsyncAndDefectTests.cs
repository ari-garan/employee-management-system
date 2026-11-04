using EmployeeManagement.Domain.Entities;
using EmployeeManagement.Domain.Enums;
using EmployeeManagement.Domain.ValueObjects;
using EmployeeManagement.Infrastructure.Repositories;
using EmployeeManagement.Services.Calculators;
using EmployeeManagement.Services.Implementations;
using Xunit;

namespace EmployeeManagement.Tests;

public class AsyncAndDefectTests
{
    [Fact]
    public async Task GeneratePayrollReportAsync_CancellationRequested_ThrowsOperationCanceledException()
    {
        // Arrange
        var deptRepo = new InMemoryDepartmentRepository();
        var empRepo = new InMemoryEmployeeRepository();
        var calc = new BonusCalculator();
        var service = new EmployeeService(empRepo, deptRepo, calc);

        var dept = new Department("IT", DepartmentCode.Engineering, 500_000m);
        await deptRepo.AddAsync(dept);

        for (int i = 0; i < 5; i++)
        {
            var emp = new FullTimeEmployee($"Emp{i}", "Test", $"emp{i}@test.com", dept.Id, RoleType.SoftwareEngineer, new DateOnly(2023, 1, 1), new Compensation(80_000m));
            await empRepo.AddAsync(emp);
        }

        using var cts = new CancellationTokenSource();
        cts.Cancel(); // Pre-cancelled token

        // Act & Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            service.GeneratePayrollReportAsync(1.0m, null, cts.Token));
    }

    [Fact]
    public void Day5_InjectedDefect_ThrowsInvalidCastExceptionOnContractor()
    {
        // Arrange
        var calc = new BonusCalculator();
        var contractor = new ContractEmployee(
            "Contractor",
            "Person",
            "contractor@test.com",
            Guid.NewGuid(),
            RoleType.SoftwareEngineer,
            new DateOnly(2024, 1, 1),
            60m,
            new DateOnly(2024, 12, 31),
            "StaffingCo");

        // Act & Assert: Buggy implementation assumes non-FullTime is Manager
        Assert.Throws<InvalidCastException>(() => calc.CalculateWithInjectedDefect(contractor));

        // And the corrected Calculate method succeeds safely:
        var correctBonus = calc.Calculate(contractor);
        Assert.True(correctBonus > 0);
    }

    [Fact]
    public void Day5_InjectedDefect_PaysTerminatedEmployee_WhileCorrectedMethodReturnsZero()
    {
        // Arrange
        var calc = new BonusCalculator();
        var emp = new FullTimeEmployee(
            "Former",
            "Employee",
            "former@test.com",
            Guid.NewGuid(),
            RoleType.SeniorEngineer,
            new DateOnly(2020, 1, 1),
            new Compensation(100_000m),
            targetBonusPercentage: 0.10m);

        emp.ChangeStatus(EmploymentStatus.Terminated);

        // Act: Buggy method erroneously pays 10,000
        var buggyBonus = calc.CalculateWithInjectedDefect(emp);
        // Correct method pays 0
        var fixedBonus = calc.Calculate(emp);

        // Assert
        Assert.Equal(10_000m, buggyBonus);
        Assert.Equal(0m, fixedBonus);
    }
}

