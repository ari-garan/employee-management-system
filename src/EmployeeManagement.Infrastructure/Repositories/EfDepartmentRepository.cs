using EmployeeManagement.Domain.Entities;
using EmployeeManagement.Domain.Enums;
using EmployeeManagement.Domain.Interfaces;
using EmployeeManagement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagement.Infrastructure.Repositories;

/// <summary>
/// Day 9 Deliverable: EF Core implementation of IDepartmentRepository with eager loading
/// of Employees collection using .Include(d => d.Employees).
/// </summary>
public class EfDepartmentRepository : IDepartmentRepository
{
    private readonly EmployeeManagementDbContext _dbContext;

    public EfDepartmentRepository(EmployeeManagementDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Department?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Departments
            .Include(d => d.Employees)
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Department>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Departments
            .Include(d => d.Employees)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Department>> FindAsync(Func<Department, bool> predicate, CancellationToken cancellationToken = default)
    {
        var all = await _dbContext.Departments
            .Include(d => d.Employees)
            .ToListAsync(cancellationToken);
        return all.Where(predicate).ToList();
    }

    public async Task<Department> AddAsync(Department entity, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);
        await _dbContext.Departments.AddAsync(entity, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task UpdateAsync(Department entity, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);
        _dbContext.Departments.Update(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.Departments.FindAsync(new object[] { id }, cancellationToken);
        if (entity != null)
        {
            _dbContext.Departments.Remove(entity);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<Department?> GetByCodeAsync(DepartmentCode code, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Departments
            .Include(d => d.Employees)
            .FirstOrDefaultAsync(d => d.Code == code, cancellationToken);
    }

    public async Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        return await _dbContext.Departments
            .AnyAsync(d => d.Name.ToLower() == name.Trim().ToLower(), cancellationToken);
    }
}
