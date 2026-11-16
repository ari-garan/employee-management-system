using System.ComponentModel.DataAnnotations;
using EmployeeManagement.Domain.Enums;

namespace EmployeeManagement.Api.Models;

public record UpdateStatusRequest(
    [Required] EmploymentStatus Status);

public record PromoteEmployeeRequest(
    [Required] RoleType NewRole,
    [Range(1, 10_000_000, ErrorMessage = "New salary must be positive")] decimal NewSalary);

public record LeaveRequestModel(
    [Range(1, 365, ErrorMessage = "Requested leave days must be between 1 and 365")] int Days);
