using EmployeeManagement.Domain.Entities;
using EmployeeManagement.Domain.Enums;
using EmployeeManagement.Domain.Interfaces;

namespace EmployeeManagement.Infrastructure.Repositories;

public class InMemoryEmployeeRepository : InMemoryRepository<Employee>, IEmployeeRepository
{
    public Task<Employee?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (string.IsNullOrWhiteSpace(email))
            return Task.FromResult<Employee?>(null);

        var normalizedEmail = email.Trim().ToLowerInvariant();
        var employee = Storage.Values.FirstOrDefault(e => e.Email.Equals(normalizedEmail, StringComparison.OrdinalIgnoreCase));
        return Task.FromResult(employee);
    }

    public Task<IReadOnlyList<Employee>> GetByDepartmentIdAsync(Guid departmentId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        IReadOnlyList<Employee> list = Storage.Values
            .Where(e => e.DepartmentId == departmentId)
            .OrderBy(e => e.LastName)
            .ThenBy(e => e.FirstName)
            .ToList();

        return Task.FromResult(list);
    }

    public Task<IReadOnlyList<Employee>> GetByStatusAsync(EmploymentStatus status, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        IReadOnlyList<Employee> list = Storage.Values
            .Where(e => e.Status == status)
            .ToList();

        return Task.FromResult(list);
    }

    public Task<IReadOnlyList<Employee>> GetTopEarnersAsync(int count, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (count <= 0)
            return Task.FromResult<IReadOnlyList<Employee>>(Array.Empty<Employee>());

        IReadOnlyList<Employee> list = Storage.Values
            .OrderByDescending(e => e.Compensation.AnnualBaseSalary)
            .Take(count)
            .ToList();

        return Task.FromResult(list);
    }

    public Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (string.IsNullOrWhiteSpace(email))
            return Task.FromResult(false);

        var normalizedEmail = email.Trim().ToLowerInvariant();
        var exists = Storage.Values.Any(e => e.Email.Equals(normalizedEmail, StringComparison.OrdinalIgnoreCase));
        return Task.FromResult(exists);
    }
}

