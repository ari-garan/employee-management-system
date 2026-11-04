using EmployeeManagement.Domain.Entities;
using EmployeeManagement.Domain.Enums;
using EmployeeManagement.Domain.Exceptions;
using EmployeeManagement.Domain.Interfaces;
using EmployeeManagement.Domain.ValueObjects;
using EmployeeManagement.Services.DTOs;
using EmployeeManagement.Services.Interfaces;

namespace EmployeeManagement.Services.Implementations;

public class EmployeeService : IEmployeeService
{
    private readonly IEmployeeRepository _employeeRepo;
    private readonly IDepartmentRepository _departmentRepo;
    private readonly IBonusCalculator _bonusCalculator;

    public EmployeeService(
        IEmployeeRepository employeeRepo,
        IDepartmentRepository departmentRepo,
        IBonusCalculator bonusCalculator)
    {
        _employeeRepo = employeeRepo ?? throw new ArgumentNullException(nameof(employeeRepo));
        _departmentRepo = departmentRepo ?? throw new ArgumentNullException(nameof(departmentRepo));
        _bonusCalculator = bonusCalculator ?? throw new ArgumentNullException(nameof(bonusCalculator));
    }

    public async Task<EmployeeDto> CreateEmployeeAsync(CreateEmployeeDto dto, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dto);

        // Validation: Verify department exists
        var department = await _departmentRepo.GetByIdAsync(dto.DepartmentId, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(Department), dto.DepartmentId);

        // Validation: Email uniqueness
        if (await _employeeRepo.ExistsByEmailAsync(dto.Email, cancellationToken))
            throw new ValidationException($"An employee with email '{dto.Email}' already exists.");

        // Create polymorphic entity based on EmploymentType
        Employee employee = dto.EmploymentType.ToLowerInvariant() switch
        {
            "fulltime" => new FullTimeEmployee(
                dto.FirstName,
                dto.LastName,
                dto.Email,
                dto.DepartmentId,
                dto.Role,
                dto.HireDate,
                new Compensation(dto.BaseSalary),
                dto.TargetBonusPercentage ?? 0.10m,
                20,
                dto.Address),

            "manager" => new Manager(
                dto.FirstName,
                dto.LastName,
                dto.Email,
                dto.DepartmentId,
                dto.Role,
                dto.HireDate,
                new Compensation(dto.BaseSalary),
                dto.TargetBonusPercentage ?? 0.20m,
                dto.ManagementAllowance ?? 12_000m,
                dto.Address),

            "contractor" => new ContractEmployee(
                dto.FirstName,
                dto.LastName,
                dto.Email,
                dto.DepartmentId,
                dto.Role,
                dto.HireDate,
                dto.HourlyRate ?? (dto.BaseSalary / 2080m),
                dto.ContractEndDate ?? DateOnly.FromDateTime(DateTime.UtcNow.AddYears(1)),
                dto.AgencyName ?? "Independent Contractor",
                dto.Address),

            _ => throw new ValidationException($"Unsupported employment type: '{dto.EmploymentType}'. Use 'FullTime', 'Manager', or 'Contractor'.")
        };

        // Assign to department (checks department budget)
        department.AssignEmployee(employee);
        await _departmentRepo.UpdateAsync(department, cancellationToken);

        await _employeeRepo.AddAsync(employee, cancellationToken);
        return MapToDto(employee, department.Name);
    }

    public async Task<EmployeeDto?> GetEmployeeByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var employee = await _employeeRepo.GetByIdAsync(id, cancellationToken);
        if (employee == null) return null;

        var department = await _departmentRepo.GetByIdAsync(employee.DepartmentId, cancellationToken);
        return MapToDto(employee, department?.Name ?? "Unknown");
    }

    public async Task<PagedResult<EmployeeDto>> GetEmployeesAsync(EmployeeFilterDto filter, CancellationToken cancellationToken = default)
    {
        var allEmployees = await _employeeRepo.GetAllAsync(cancellationToken);
        var departments = (await _departmentRepo.GetAllAsync(cancellationToken))
            .ToDictionary(d => d.Id, d => d.Name);

        // LINQ query pipeline
        var query = allEmployees.AsEnumerable();

        if (filter.DepartmentId.HasValue)
            query = query.Where(e => e.DepartmentId == filter.DepartmentId.Value);

        if (filter.Status.HasValue)
            query = query.Where(e => e.Status == filter.Status.Value);

        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var term = filter.SearchTerm.Trim().ToLowerInvariant();
            query = query.Where(e => e.FullName.ToLowerInvariant().Contains(term) ||
                                     e.Email.ToLowerInvariant().Contains(term));
        }

        if (filter.MinSalary.HasValue)
            query = query.Where(e => e.Compensation.AnnualBaseSalary >= filter.MinSalary.Value);

        if (filter.MaxSalary.HasValue)
            query = query.Where(e => e.Compensation.AnnualBaseSalary <= filter.MaxSalary.Value);

        // Sorting
        query = (filter.SortBy?.ToLowerInvariant(), filter.IsDescending) switch
        {
            ("salary", true) => query.OrderByDescending(e => e.Compensation.AnnualBaseSalary),
            ("salary", false) => query.OrderBy(e => e.Compensation.AnnualBaseSalary),
            ("hiredate", true) => query.OrderByDescending(e => e.HireDate),
            ("hiredate", false) => query.OrderBy(e => e.HireDate),
            ("name", true) => query.OrderByDescending(e => e.LastName).ThenByDescending(e => e.FirstName),
            _ => query.OrderBy(e => e.LastName).ThenBy(e => e.FirstName)
        };

        var filteredList = query.ToList();
        var totalCount = filteredList.Count;

        var pagedItems = filteredList
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(e => MapToDto(e, departments.TryGetValue(e.DepartmentId, out var name) ? name : "Unknown"))
            .ToList();

        return new PagedResult<EmployeeDto>(pagedItems, totalCount, filter.PageNumber, filter.PageSize);
    }

    public async Task<IReadOnlyList<EmployeeDto>> GetTopEarnersAsync(int count, CancellationToken cancellationToken = default)
    {
        var topEmployees = await _employeeRepo.GetTopEarnersAsync(count, cancellationToken);
        var departments = (await _departmentRepo.GetAllAsync(cancellationToken))
            .ToDictionary(d => d.Id, d => d.Name);

        return topEmployees
            .Select(e => MapToDto(e, departments.TryGetValue(e.DepartmentId, out var name) ? name : "Unknown"))
            .ToList();
    }

    public async Task UpdateEmployeeStatusAsync(Guid id, EmploymentStatus newStatus, CancellationToken cancellationToken = default)
    {
        var employee = await _employeeRepo.GetByIdAsync(id, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(Employee), id);

        employee.ChangeStatus(newStatus);
        await _employeeRepo.UpdateAsync(employee, cancellationToken);
    }

    public async Task PromoteEmployeeAsync(Guid id, RoleType newRole, decimal newSalary, CancellationToken cancellationToken = default)
    {
        var employee = await _employeeRepo.GetByIdAsync(id, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(Employee), id);

        employee.Promote(newRole, newSalary);
        await _employeeRepo.UpdateAsync(employee, cancellationToken);
    }

    public async Task<IReadOnlyList<PayrollReportItem>> GeneratePayrollReportAsync(
        decimal companyMultiplier,
        IProgress<string>? progress = null,
        CancellationToken cancellationToken = default)
    {
        progress?.Report("Initializing payroll report generation...");
        var employees = await _employeeRepo.GetAllAsync(cancellationToken);
        var departments = (await _departmentRepo.GetAllAsync(cancellationToken))
            .ToDictionary(d => d.Id, d => d.Name);

        var report = new List<PayrollReportItem>();
        int count = 0;

        foreach (var employee in employees)
        {
            // Check for cooperative cancellation
            cancellationToken.ThrowIfCancellationRequested();

            // Simulate slight async delay (e.g. external tax or compliance check)
            await Task.Delay(80, cancellationToken);

            var bonus = _bonusCalculator.Calculate(employee, companyMultiplier);
            var baseSalary = employee.Compensation.AnnualBaseSalary;
            var total = baseSalary + bonus;

            var deptName = departments.TryGetValue(employee.DepartmentId, out var name) ? name : "General";

            report.Add(new PayrollReportItem(
                employee.Id,
                employee.FullName,
                deptName,
                employee.EmploymentType,
                baseSalary,
                bonus,
                total));

            count++;
            progress?.Report($"Processed {count}/{employees.Count} employees ({employee.FullName})...");
        }

        progress?.Report("Payroll report completed successfully.");
        return report.OrderByDescending(r => r.TotalCompensation).ToList();
    }

    public async Task RequestLeaveAsync(Guid employeeId, int days, CancellationToken cancellationToken = default)
    {
        var employee = await _employeeRepo.GetByIdAsync(employeeId, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(Employee), employeeId);

        if (employee is FullTimeEmployee ft)
        {
            ft.RequestLeave(days);
            await _employeeRepo.UpdateAsync(ft, cancellationToken);
        }
        else
        {
            throw new ValidationException($"Employee '{employee.FullName}' is a {employee.EmploymentType}. Paid leave is only available for full-time employees.");
        }
    }

    public async Task ReturnFromLeaveAsync(Guid employeeId, CancellationToken cancellationToken = default)
    {
        var employee = await _employeeRepo.GetByIdAsync(employeeId, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(Employee), employeeId);

        if (employee is FullTimeEmployee ft)
        {
            ft.ReturnFromLeave();
            await _employeeRepo.UpdateAsync(ft, cancellationToken);
        }
        else
        {
            employee.ChangeStatus(EmploymentStatus.Active);
            await _employeeRepo.UpdateAsync(employee, cancellationToken);
        }
    }

    private static EmployeeDto MapToDto(Employee e, string departmentName)
    {
        return new EmployeeDto(
            e.Id,
            e.FirstName,
            e.LastName,
            e.FullName,
            e.Email,
            e.DepartmentId,
            departmentName,
            e.Role.ToString(),
            e.Status.ToString(),
            e.EmploymentType,
            e.Compensation.AnnualBaseSalary,
            e.HireDate,
            e.ResidentialAddress?.ToString());
    }
}

