using EmployeeManagement.Domain.Enums;
using EmployeeManagement.Domain.ValueObjects;

namespace EmployeeManagement.Domain.Entities;

public class ContractEmployee : Employee
{
    public decimal HourlyRate { get; private set; }
    public DateOnly ContractEndDate { get; private set; }
    public string AgencyName { get; private set; }

    public ContractEmployee(
        string firstName,
        string lastName,
        string email,
        Guid departmentId,
        RoleType role,
        DateOnly hireDate,
        decimal hourlyRate,
        DateOnly contractEndDate,
        string agencyName,
        Address? address = null)
        : base(firstName, lastName, email, departmentId, role, hireDate, new Compensation(hourlyRate * 2080), address)
    {
        HourlyRate = hourlyRate;
        ContractEndDate = contractEndDate;
        AgencyName = agencyName;
    }

    public override string EmploymentType => "Contractor";

    /// <summary>
    /// Contractors receive a fixed discretionary performance completion bonus, unaffected by company multipliers.
    /// </summary>
    public override decimal CalculateAnnualBonus(decimal companyMultiplier = 1.0m)
    {
        if (Status != EmploymentStatus.Active)
            return 0m;

        // Contractors receive 3% flat completion bonus if contract is still active
        return Math.Round(Compensation.AnnualBaseSalary * 0.03m, 2);
    }
}

