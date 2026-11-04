using EmployeeManagement.Domain.Enums;
using EmployeeManagement.Domain.Exceptions;
using EmployeeManagement.Domain.ValueObjects;

namespace EmployeeManagement.Domain.Entities;

public class FullTimeEmployee : Employee
{
    public decimal TargetBonusPercentage { get; private set; }
    public int PaidTimeOffDays { get; private set; }
    public int RemainingPtoDays { get; private set; }

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
        RemainingPtoDays = ptoDays;
    }

    public override string EmploymentType => "FullTime";

    // Validates balance, deducts days, and sets status to OnLeave.
    public void RequestLeave(int days)
    {
        if (days <= 0)
            throw new ValidationException("Requested leave days must be greater than zero.");

        if (Status == EmploymentStatus.Terminated)
            throw new ValidationException("Terminated employees cannot request leave.");

        if (days > RemainingPtoDays)
            throw new InsufficientLeaveBalanceException(days, RemainingPtoDays);

        RemainingPtoDays -= days;
        Status = EmploymentStatus.OnLeave;
        MarkUpdated();
    }

    // Restores status to Active from OnLeave.
    public void ReturnFromLeave()
    {
        if (Status == EmploymentStatus.OnLeave)
        {
            Status = EmploymentStatus.Active;
            MarkUpdated();
        }
    }

    // Bonus = BaseSalary * TargetBonusPercentage * CompanyMultiplier
    public override decimal CalculateAnnualBonus(decimal companyMultiplier = 1.0m)
    {
        if (Status != EmploymentStatus.Active)
            return 0m;

        return Math.Round(Compensation.AnnualBaseSalary * TargetBonusPercentage * companyMultiplier, 2);
    }
}

