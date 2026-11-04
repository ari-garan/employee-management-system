using EmployeeManagement.Domain.Enums;
using EmployeeManagement.Domain.ValueObjects;

namespace EmployeeManagement.Domain.Entities;

public class Manager : FullTimeEmployee
{
    private readonly List<Guid> _directReportIds = new();

    public decimal ManagementAllowance { get; private set; }
    public IReadOnlyCollection<Guid> DirectReportIds => _directReportIds.AsReadOnly();

    public Manager(
        string firstName,
        string lastName,
        string email,
        Guid departmentId,
        RoleType role,
        DateOnly hireDate,
        Compensation compensation,
        decimal targetBonusPercentage = 0.20m,
        decimal managementAllowance = 12000m,
        Address? address = null)
        : base(firstName, lastName, email, departmentId, role, hireDate, compensation, targetBonusPercentage, 25, address)
    {
        ManagementAllowance = managementAllowance;
    }

    public override string EmploymentType => "Manager";

    public void AddDirectReport(Guid employeeId)
    {
        if (employeeId != Guid.Empty && !_directReportIds.Contains(employeeId))
        {
            _directReportIds.Add(employeeId);
            MarkUpdated();
        }
    }

    public void RemoveDirectReport(Guid employeeId)
    {
        if (_directReportIds.Remove(employeeId))
        {
            MarkUpdated();
        }
    }

    /// <summary>
    /// Managers calculate bonus based on target bonus + management allowance + team size incentive.
    /// </summary>
    public override decimal CalculateAnnualBonus(decimal companyMultiplier = 1.0m)
    {
        if (Status != EmploymentStatus.Active)
            return 0m;

        var baseBonus = base.CalculateAnnualBonus(companyMultiplier);
        var teamBonus = _directReportIds.Count * 500m;
        return baseBonus + ManagementAllowance + teamBonus;
    }
}

