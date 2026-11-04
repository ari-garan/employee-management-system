using EmployeeManagement.Domain.Enums;
using EmployeeManagement.Domain.Interfaces;
using EmployeeManagement.Services.DTOs;
using EmployeeManagement.Services.Interfaces;

namespace EmployeeManagement.Services.Implementations;

public class DepartmentService : IDepartmentService
{
    private readonly IDepartmentRepository _departmentRepo;
    private readonly IEmployeeRepository _employeeRepo;

    public DepartmentService(IDepartmentRepository departmentRepo, IEmployeeRepository employeeRepo)
    {
        _departmentRepo = departmentRepo ?? throw new ArgumentNullException(nameof(departmentRepo));
        _employeeRepo = employeeRepo ?? throw new ArgumentNullException(nameof(employeeRepo));
    }

    public async Task<IReadOnlyList<DepartmentSummaryDto>> GetDepartmentSummariesAsync(CancellationToken cancellationToken = default)
    {
        var departments = await _departmentRepo.GetAllAsync(cancellationToken);
        var employees = await _employeeRepo.GetAllAsync(cancellationToken);

        // Group employees by DepartmentId using LINQ
        var employeesByDept = employees
            .GroupBy(e => e.DepartmentId)
            .ToDictionary(g => g.Key, g => g.ToList());

        var summaries = new List<DepartmentSummaryDto>();

        foreach (var dept in departments)
        {
            var deptEmployees = employeesByDept.TryGetValue(dept.Id, out var emps)
                ? emps
                : new List<Domain.Entities.Employee>();

            var totalCommitted = deptEmployees.Sum(e => e.Compensation.AnnualBaseSalary);
            var remaining = dept.AnnualBudget - totalCommitted;
            var utilPct = dept.AnnualBudget > 0 ? (totalCommitted / dept.AnnualBudget) * 100m : 0m;

            var activeCount = deptEmployees.Count(e => e.Status == EmploymentStatus.Active);
            var avgSalary = deptEmployees.Count > 0 ? deptEmployees.Average(e => e.Compensation.AnnualBaseSalary) : 0m;
            var minSalary = deptEmployees.Count > 0 ? deptEmployees.Min(e => e.Compensation.AnnualBaseSalary) : 0m;
            var maxSalary = deptEmployees.Count > 0 ? deptEmployees.Max(e => e.Compensation.AnnualBaseSalary) : 0m;

            summaries.Add(new DepartmentSummaryDto(
                dept.Id,
                dept.Name,
                dept.Code.ToString(),
                dept.AnnualBudget,
                totalCommitted,
                remaining,
                Math.Round(utilPct, 2),
                deptEmployees.Count,
                activeCount,
                Math.Round(avgSalary, 2),
                minSalary,
                maxSalary));
        }

        return summaries.OrderByDescending(s => s.TotalCommittedSalaries).ToList();
    }

    public async Task<DepartmentSummaryDto?> GetDepartmentSummaryByIdAsync(Guid departmentId, CancellationToken cancellationToken = default)
    {
        var summaries = await GetDepartmentSummariesAsync(cancellationToken);
        return summaries.FirstOrDefault(s => s.DepartmentId == departmentId);
    }
}

