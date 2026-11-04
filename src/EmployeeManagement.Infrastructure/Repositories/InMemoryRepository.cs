using System.Collections.Concurrent;
using EmployeeManagement.Domain.Common;
using EmployeeManagement.Domain.Exceptions;
using EmployeeManagement.Domain.Interfaces;

namespace EmployeeManagement.Infrastructure.Repositories;

/// <summary>
/// Generic thread-safe in-memory repository implementing async operations and cancellation tokens.
/// </summary>
public class InMemoryRepository<T> : IRepository<T> where T : BaseEntity
{
    protected readonly ConcurrentDictionary<Guid, T> Storage = new();

    public virtual Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Storage.TryGetValue(id, out var entity);
        return Task.FromResult(entity);
    }

    public virtual Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        IReadOnlyList<T> list = Storage.Values.ToList();
        return Task.FromResult(list);
    }

    public virtual Task<IReadOnlyList<T>> FindAsync(Func<T, bool> predicate, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(predicate);
        IReadOnlyList<T> list = Storage.Values.Where(predicate).ToList();
        return Task.FromResult(list);
    }

    public virtual Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(entity);

        if (!Storage.TryAdd(entity.Id, entity))
        {
            throw new DomainException($"Entity of type {typeof(T).Name} with ID '{entity.Id}' already exists.");
        }

        return Task.FromResult(entity);
    }

    public virtual Task UpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(entity);

        if (!Storage.ContainsKey(entity.Id))
        {
            throw new EntityNotFoundException(typeof(T).Name, entity.Id);
        }

        Storage[entity.Id] = entity;
        entity.MarkUpdated();
        return Task.CompletedTask;
    }

    public virtual Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!Storage.TryRemove(id, out _))
        {
            throw new EntityNotFoundException(typeof(T).Name, id);
        }

        return Task.CompletedTask;
    }
}

