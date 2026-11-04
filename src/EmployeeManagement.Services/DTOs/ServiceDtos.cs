using EmployeeManagement.Domain.Enums;
using EmployeeManagement.Domain.ValueObjects;

namespace EmployeeManagement.Services.DTOs;

public record CreateEmployeeDto(
    string FirstName,
    string LastName,
    string Email,
    Guid DepartmentId,
    RoleType Role,
    DateOnly HireDate,
    decimal BaseSalary,
    string EmploymentType, // "FullTime", "Contractor", "Manager"
    decimal? TargetBonusPercentage = null,
    decimal? HourlyRate = null,
    DateOnly? ContractEndDate = null,
    string? AgencyName = null,
    decimal? ManagementAllowance = null,
    Address? Address = null);

public record EmployeeDto(
    Guid Id,
    string FirstName,
    string LastName,
    string FullName,
    string Email,
    Guid DepartmentId,
    string DepartmentName,
    string Role,
    string Status,
    string EmploymentType,
    decimal AnnualSalary,
    DateOnly HireDate,
    string? ResidentialAddress);

public record DepartmentSummaryDto(
    Guid DepartmentId,
    string DepartmentName,
    string DepartmentCode,
    decimal AnnualBudget,
    decimal TotalCommittedSalaries,
    decimal RemainingBudget,
    decimal BudgetUtilizationPercentage,
    int TotalEmployees,
    int ActiveEmployees,
    decimal AverageSalary,
    decimal MinSalary,
    decimal MaxSalary);

public record EmployeeFilterDto(
    Guid? DepartmentId = null,
    EmploymentStatus? Status = null,
    string? SearchTerm = null,
    decimal? MinSalary = null,
    decimal? MaxSalary = null,
    string? SortBy = "name", // name, salary, hireDate
    bool IsDescending = false,
    int PageNumber = 1,
    int PageSize = 10);

public record PagedResult<T>(
    IReadOnlyList<T> Items,
    int TotalCount,
    int PageNumber,
    int PageSize)
{
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasNextPage => PageNumber < TotalPages;
    public bool HasPreviousPage => PageNumber > 1;
}

public record PayrollReportItem(
    Guid EmployeeId,
    string FullName,
    string DepartmentName,
    string EmploymentType,
    decimal BaseSalary,
    decimal CalculatedBonus,
    decimal TotalCompensation);

