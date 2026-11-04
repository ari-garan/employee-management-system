namespace EmployeeManagement.Domain.Exceptions;

public class DomainException : Exception
{
    public DomainException(string message) : base(message) { }
    public DomainException(string message, Exception innerException) : base(message, innerException) { }
}

public class EntityNotFoundException : DomainException
{
    public string EntityName { get; }
    public object Key { get; }

    public EntityNotFoundException(string entityName, object key)
        : base($"Entity '{entityName}' with key '{key}' was not found.")
    {
        EntityName = entityName;
        Key = key;
    }
}

public class ValidationException : DomainException
{
    public IReadOnlyList<string> Errors { get; }

    public ValidationException(string message, IEnumerable<string>? errors = null)
        : base(message)
    {
        Errors = errors?.ToList() ?? new List<string> { message };
    }
}

public class BudgetExceededException : DomainException
{
    public decimal RequestedAmount { get; }
    public decimal AvailableBudget { get; }

    public BudgetExceededException(decimal requested, decimal available)
        : base($"Requested amount {requested:C} exceeds available budget of {available:C}.")
    {
        RequestedAmount = requested;
        AvailableBudget = available;
    }
}

public class InsufficientLeaveBalanceException : DomainException
{
    public int RequestedDays { get; }
    public int RemainingDays { get; }

    public InsufficientLeaveBalanceException(int requestedDays, int remainingDays)
        : base($"Cannot grant {requestedDays} days of leave. Only {remainingDays} paid time off days remaining.")
    {
        RequestedDays = requestedDays;
        RemainingDays = remainingDays;
    }
}


