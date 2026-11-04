using EmployeeManagement.Domain.Common;
using EmployeeManagement.Domain.Enums;
using EmployeeManagement.Domain.Exceptions;

namespace EmployeeManagement.Domain.Entities;

public class Department : BaseEntity
{
    private readonly List<Employee> _employees = new();

    public string Name { get; private set; }
    public DepartmentCode Code { get; private set; }
    public decimal AnnualBudget { get; private set; }
    public IReadOnlyCollection<Employee> Employees => _employees.AsReadOnly();

    public Department(string name, DepartmentCode code, decimal annualBudget)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ValidationException("Department name cannot be empty.");
        if (annualBudget <= 0)
            throw new ValidationException("Annual budget must be greater than zero.");

        Name = name.Trim();
        Code = code;
        AnnualBudget = annualBudget;
    }

    public void UpdateBudget(decimal newBudget)
    {
        if (newBudget <= 0)
            throw new ValidationException("Budget must be positive.");

        var committedSalaries = _employees.Sum(e => e.Compensation.AnnualBaseSalary);
        if (newBudget < committedSalaries)
            throw new BudgetExceededException(committedSalaries, newBudget);

        AnnualBudget = newBudget;
        MarkUpdated();
    }

    public void AssignEmployee(Employee employee)
    {
        ArgumentNullException.ThrowIfNull(employee);

        if (_employees.Any(e => e.Id == employee.Id))
            return;

        var projectedPayroll = _employees.Sum(e => e.Compensation.AnnualBaseSalary) + employee.Compensation.AnnualBaseSalary;
        if (projectedPayroll > AnnualBudget)
            throw new BudgetExceededException(projectedPayroll, AnnualBudget);

        _employees.Add(employee);
        MarkUpdated();
    }

    public void RemoveEmployee(Guid employeeId)
    {
        var emp = _employees.FirstOrDefault(e => e.Id == employeeId);
        if (emp != null)
        {
            _employees.Remove(emp);
            MarkUpdated();
        }
    }
}

