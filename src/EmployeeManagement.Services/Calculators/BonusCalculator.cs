using EmployeeManagement.Domain.Entities;
using EmployeeManagement.Domain.Enums;
using EmployeeManagement.Services.Interfaces;

namespace EmployeeManagement.Services.Calculators;

/// <summary>
/// Calculates annual bonuses using modern C# pattern matching and polymorphism.
/// Also includes a defect reproduction mechanism fulfilling the Day 5 assessment requirement.
/// </summary>
public class BonusCalculator : IBonusCalculator
{
    public decimal Calculate(Employee employee, decimal companyMultiplier = 1.0m)
    {
        ArgumentNullException.ThrowIfNull(employee);

        // Terminated or on-leave employees are not eligible for bonus
        if (employee.Status != EmploymentStatus.Active)
            return 0m;

        // Delegates to polymorphic domain method
        return employee.CalculateAnnualBonus(companyMultiplier);
    }

    /// <summary>
    /// DAY 5 INJECTED DEFECT:
    /// In this buggy implementation:
    /// 1. It does NOT check employee.Status, erroneously paying bonuses to Terminated employees.
    /// 2. For ContractEmployees, it accesses an uninitialized field causing a NullReferenceException,
    ///    or performs unhandled integer truncation.
    /// </summary>
    public decimal CalculateWithInjectedDefect(Employee employee, decimal companyMultiplier = 1.0m)
    {
        ArgumentNullException.ThrowIfNull(employee);

        // DEFECT 1: Missed Status check (Terminated employee gets paid!)
        // DEFECT 2: Pattern match bug using faulty cast
        if (employee is FullTimeEmployee ft)
        {
            return ft.Compensation.AnnualBaseSalary * ft.TargetBonusPercentage * companyMultiplier;
        }

        // Buggy assumption: treats non-fulltime as manager without checking type
        var mgr = (Manager)employee; // Throws InvalidCastException for Contractors!
        return mgr.CalculateAnnualBonus(companyMultiplier);
    }
}

