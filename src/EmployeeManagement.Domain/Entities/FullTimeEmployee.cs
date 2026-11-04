using EmployeeManagement.Domain.Enums;
using EmployeeManagement.Domain.ValueObjects;

namespace EmployeeManagement.Domain.Entities;

public class FullTimeEmployee : Employee
{
    public decimal TargetBonusPercentage { get; private set; }
    public int PaidTimeOffDays { get; private set; }

    public FullTimeEmployee(
        string firstName,
        string lastName,
        string email,
        Guid departmentId,
        RoleType role,
        DateOnly hireDate,
        Compensation compensation,
        decimal targetBonusPercentage = 0.10m,
        int ptoDays = 20,
        Address? address = null)
        : base(firstName, lastName, email, departmentId, role, hireDate, compensation, address)
    {
        TargetBonusPercentage = targetBonusPercentage;
        PaidTimeOffDays = ptoDays;
    }

    public override string EmploymentType => "FullTime";

    /// <summary>
    /// Bonus = BaseSalary * TargetBonusPercentage * CompanyMultiplier
    /// </summary>
    public override decimal CalculateAnnualBonus(decimal companyMultiplier = 1.0m)
    {
        if (Status != EmploymentStatus.Active)
            return 0m;

        return Math.Round(Compensation.AnnualBaseSalary * TargetBonusPercentage * companyMultiplier, 2);
    }
}

