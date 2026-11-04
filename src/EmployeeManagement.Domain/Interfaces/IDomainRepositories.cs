using EmployeeManagement.Domain.Entities;
using EmployeeManagement.Domain.Enums;

namespace EmployeeManagement.Domain.Interfaces;

public interface IEmployeeRepository : IRepository<Employee>
{
    Task<Employee?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Employee>> GetByDepartmentIdAsync(Guid departmentId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Employee>> GetByStatusAsync(EmploymentStatus status, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Employee>> GetTopEarnersAsync(int count, CancellationToken cancellationToken = default);
    Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default);
}

public interface IDepartmentRepository : IRepository<Department>
{
    Task<Department?> GetByCodeAsync(DepartmentCode code, CancellationToken cancellationToken = default);
    Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default);
}

