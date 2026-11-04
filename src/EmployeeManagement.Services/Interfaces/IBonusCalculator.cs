using EmployeeManagement.Domain.Entities;
using EmployeeManagement.Domain.Enums;

namespace EmployeeManagement.Services.Interfaces;

public interface IBonusCalculator
{
    decimal Calculate(Employee employee, decimal companyMultiplier = 1.0m);
    
    /// <summary>
    /// Demonstrates the Day 5 injected defect (e.g. division by zero or failing to check inactive status)
    /// vs the corrected behavior.
    /// </summary>
    decimal CalculateWithInjectedDefect(Employee employee, decimal companyMultiplier = 1.0m);
}

