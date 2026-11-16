using System.Text.RegularExpressions;
using EmployeeManagement.Api.Models;
using EmployeeManagement.Domain.Enums;
using EmployeeManagement.Domain.Exceptions;
using EmployeeManagement.Services.DTOs;

namespace EmployeeManagement.Api.Validators;

/// <summary>
/// Day 8 Deliverable: Centralized DTO & Request Validation Rules.
/// Enforces business constraints before workflow execution.
/// </summary>
public static class ApiValidators
{
    private static readonly Regex EmailRegex = new(
        @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public static void Validate(this CreateEmployeeDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);

        if (string.IsNullOrWhiteSpace(dto.FirstName) || dto.FirstName.Trim().Length < 2 || dto.FirstName.Length > 50)
            throw new ValidationException("First name is required and must be between 2 and 50 characters.");

        if (string.IsNullOrWhiteSpace(dto.LastName) || dto.LastName.Trim().Length < 2 || dto.LastName.Length > 50)
            throw new ValidationException("Last name is required and must be between 2 and 50 characters.");

        if (string.IsNullOrWhiteSpace(dto.Email) || !EmailRegex.IsMatch(dto.Email))
            throw new ValidationException($"Invalid corporate email address: '{dto.Email}'.");

        if (dto.DepartmentId == Guid.Empty)
            throw new ValidationException("A valid Department ID must be specified.");

        if (dto.BaseSalary <= 0)
            throw new ValidationException("Base salary must be strictly greater than zero.");

        var validTypes = new[] { "FullTime", "Contractor", "Manager" };
        if (!validTypes.Contains(dto.EmploymentType, StringComparer.OrdinalIgnoreCase))
            throw new ValidationException($"Employment type '{dto.EmploymentType}' is invalid. Allowed values: FullTime, Contractor, Manager.");

        if (dto.EmploymentType.Equals("FullTime", StringComparison.OrdinalIgnoreCase))
        {
            if (dto.TargetBonusPercentage is < 0 or > 1.0m)
                throw new ValidationException("Target bonus percentage for full-time employees must be between 0.0 (0%) and 1.0 (100%).");
        }
        else if (dto.EmploymentType.Equals("Contractor", StringComparison.OrdinalIgnoreCase))
        {
            if (!dto.HourlyRate.HasValue || dto.HourlyRate.Value <= 0)
                throw new ValidationException("Contractors must have an hourly rate greater than zero.");

            if (string.IsNullOrWhiteSpace(dto.AgencyName))
                throw new ValidationException("Contractors must have an agency name specified.");

            if (!dto.ContractEndDate.HasValue || dto.ContractEndDate.Value <= dto.HireDate)
                throw new ValidationException("Contract end date must be after the hire date.");
        }
        else if (dto.EmploymentType.Equals("Manager", StringComparison.OrdinalIgnoreCase))
        {
            if (dto.ManagementAllowance is < 0)
                throw new ValidationException("Management allowance cannot be negative.");
        }
    }

    public static void Validate(this PromoteEmployeeRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.NewSalary <= 0)
            throw new ValidationException("Promoted salary must be strictly greater than zero.");
    }

    public static void Validate(this LeaveRequestModel request)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.Days <= 0 || request.Days > 365)
            throw new ValidationException("Requested leave days must be between 1 and 365.");
    }
}
