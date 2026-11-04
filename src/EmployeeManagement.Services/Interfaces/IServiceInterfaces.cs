using EmployeeManagement.Domain.Enums;
using EmployeeManagement.Services.DTOs;

namespace EmployeeManagement.Services.Interfaces;

public interface IEmployeeService
{
    Task<EmployeeDto> CreateEmployeeAsync(CreateEmployeeDto dto, CancellationToken cancellationToken = default);
    Task<EmployeeDto?> GetEmployeeByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PagedResult<EmployeeDto>> GetEmployeesAsync(EmployeeFilterDto filter, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<EmployeeDto>> GetTopEarnersAsync(int count, CancellationToken cancellationToken = default);
    Task UpdateEmployeeStatusAsync(Guid id, EmploymentStatus newStatus, CancellationToken cancellationToken = default);
    Task PromoteEmployeeAsync(Guid id, RoleType newRole, decimal newSalary, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PayrollReportItem>> GeneratePayrollReportAsync(
        decimal companyMultiplier,
        IProgress<string>? progress = null,
        CancellationToken cancellationToken = default);
    Task RequestLeaveAsync(Guid employeeId, int days, CancellationToken cancellationToken = default);
    Task ReturnFromLeaveAsync(Guid employeeId, CancellationToken cancellationToken = default);
}

public interface IDepartmentService
{
    Task<IReadOnlyList<DepartmentSummaryDto>> GetDepartmentSummariesAsync(CancellationToken cancellationToken = default);
    Task<DepartmentSummaryDto?> GetDepartmentSummaryByIdAsync(Guid departmentId, CancellationToken cancellationToken = default);
}

