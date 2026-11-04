using EmployeeManagement.Domain.Entities;
using EmployeeManagement.Domain.Enums;
using EmployeeManagement.Domain.Interfaces;

namespace EmployeeManagement.Infrastructure.Repositories;

public class InMemoryDepartmentRepository : InMemoryRepository<Department>, IDepartmentRepository
{
    public Task<Department?> GetByCodeAsync(DepartmentCode code, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var dept = Storage.Values.FirstOrDefault(d => d.Code == code);
        return Task.FromResult(dept);
    }

    public Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (string.IsNullOrWhiteSpace(name))
            return Task.FromResult(false);

        var trimmed = name.Trim();
        var exists = Storage.Values.Any(d => d.Name.Equals(trimmed, StringComparison.OrdinalIgnoreCase));
        return Task.FromResult(exists);
    }
}

