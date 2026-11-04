using EmployeeManagement.Domain.Common;
using EmployeeManagement.Domain.Enums;
using EmployeeManagement.Domain.Exceptions;
using EmployeeManagement.Domain.ValueObjects;

namespace EmployeeManagement.Domain.Entities;

/// <summary>
/// Abstract base employee demonstrating OOP encapsulation, property accessors,
/// and polymorphic behavior for compensation and bonus calculation.
/// </summary>
public abstract class Employee : BaseEntity
{
    private string _firstName = string.Empty;
    private string _lastName = string.Empty;
    private string _email = string.Empty;

    public string FirstName
    {
        get => _firstName;
        protected set
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ValidationException("First name cannot be empty.");
            _firstName = value.Trim();
        }
    }

    public string LastName
    {
        get => _lastName;
        protected set
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ValidationException("Last name cannot be empty.");
            _lastName = value.Trim();
        }
    }

    public string FullName => $"{FirstName} {LastName}";

    public string Email
    {
        get => _email;
        protected set
        {
            if (string.IsNullOrWhiteSpace(value) || !value.Contains('@'))
                throw new ValidationException($"Invalid email format: '{value}'");
            _email = value.Trim().ToLowerInvariant();
        }
    }

    public Guid DepartmentId { get; protected set; }
    public RoleType Role { get; protected set; }
    public EmploymentStatus Status { get; protected set; }
    public DateOnly HireDate { get; protected set; }
    public Address? ResidentialAddress { get; protected set; }
    public Compensation Compensation { get; protected set; }

    protected Employee(
        string firstName,
        string lastName,
        string email,
        Guid departmentId,
        RoleType role,
        DateOnly hireDate,
        Compensation compensation,
        Address? address = null)
    {
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        DepartmentId = departmentId;
        Role = role;
        HireDate = hireDate;
        Compensation = compensation;
        ResidentialAddress = address;
        Status = EmploymentStatus.Active;
    }

    /// <summary>
    /// Polymorphic method: each employment type implements its own bonus computation rule.
    /// </summary>
    public abstract decimal CalculateAnnualBonus(decimal companyMultiplier = 1.0m);

    /// <summary>
    /// Abstract property indicating employment model.
    /// </summary>
    public abstract string EmploymentType { get; }

    public virtual void Promote(RoleType newRole, decimal newBaseSalary)
    {
        if (newBaseSalary <= Compensation.AnnualBaseSalary)
            throw new ValidationException("Promotion must be accompanied by a salary increase.");

        Role = newRole;
        Compensation = new Compensation(newBaseSalary, Compensation.Currency);
        MarkUpdated();
    }

    public virtual void ChangeStatus(EmploymentStatus newStatus)
    {
        Status = newStatus;
        MarkUpdated();
    }

    public virtual void UpdateAddress(Address newAddress)
    {
        ResidentialAddress = newAddress ?? throw new ArgumentNullException(nameof(newAddress));
        MarkUpdated();
    }

    public virtual void TransferDepartment(Guid newDepartmentId)
    {
        if (newDepartmentId == Guid.Empty)
            throw new ValidationException("Department ID cannot be empty.");
        DepartmentId = newDepartmentId;
        MarkUpdated();
    }
}

