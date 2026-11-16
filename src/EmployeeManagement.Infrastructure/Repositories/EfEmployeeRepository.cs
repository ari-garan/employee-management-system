using EmployeeManagement.Domain.Entities;
using EmployeeManagement.Domain.Enums;
using EmployeeManagement.Domain.Interfaces;
using EmployeeManagement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagement.Infrastructure.Repositories;

/// <summary>
/// Day 9 Deliverable: EF Core implementation of IEmployeeRepository using DbContext,
/// AsNoTracking for query performance, and LINQ-to-Entities.
/// </summary>
public class EfEmployeeRepository : IEmployeeRepository
{
    private readonly EmployeeManagementDbContext _dbContext;

    public EfEmployeeRepository(EmployeeManagementDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Employee?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Employees
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Employee>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Employees
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Employee>> FindAsync(Func<Employee, bool> predicate, CancellationToken cancellationToken = default)
    {
        var all = await _dbContext.Employees.ToListAsync(cancellationToken);
        return all.Where(predicate).ToList();
    }

    public async Task<Employee> AddAsync(Employee entity, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);
        await _dbContext.Employees.AddAsync(entity, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task UpdateAsync(Employee entity, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);
        _dbContext.Employees.Update(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.Employees.FindAsync(new object[] { id }, cancellationToken);
        if (entity != null)
        {
            _dbContext.Employees.Remove(entity);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<Employee?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        var normalized = email.Trim().ToLowerInvariant();
        return await _dbContext.Employees
            .FirstOrDefaultAsync(e => e.Email == normalized, cancellationToken);
    }

    public async Task<IReadOnlyList<Employee>> GetByDepartmentIdAsync(Guid departmentId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Employees
            .AsNoTracking()
            .Where(e => e.DepartmentId == departmentId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Employee>> GetByStatusAsync(EmploymentStatus status, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Employees
            .AsNoTracking()
            .Where(e => e.Status == status)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Employee>> GetTopEarnersAsync(int count, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Employees
            .AsNoTracking()
            .OrderByDescending(e => e.Compensation.BaseSalary)
            .Take(count)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        var normalized = email.Trim().ToLowerInvariant();
        return await _dbContext.Employees
            .AnyAsync(e => e.Email == normalized, cancellationToken);
    }
}
