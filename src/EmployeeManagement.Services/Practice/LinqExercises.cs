using EmployeeManagement.Domain.Entities;
using EmployeeManagement.Domain.Enums;

namespace EmployeeManagement.Services.Practice;

// Day 3 Deliverable: 8 distinct LINQ operations with complexity.
public static class LinqExercises
{
    // Query 1: Filter active employees by department. Time: O(n), Space: O(k).
    public static List<Employee> FilterActiveByDepartment(IEnumerable<Employee> employees, Guid departmentId)
    {
        ArgumentNullException.ThrowIfNull(employees);

        return employees
            .Where(e => e.DepartmentId == departmentId && e.Status == EmploymentStatus.Active)
            .ToList();
    }

    // Query 2: Project uppercase employee badges. Time: O(n), Space: O(n).
    public static List<string> ProjectFormattedEmployeeBadges(IEnumerable<Employee> employees)
    {
        ArgumentNullException.ThrowIfNull(employees);

        return employees
            .Select(e => $"[{e.LastName.ToUpperInvariant()}, {e.FirstName.ToUpperInvariant()}] - {e.Role}")
            .ToList();
    }

    // Query 3: Multi-level sort by salary desc, then name asc. Time: O(n log n).
    public static List<Employee> SortBySalaryDescendingThenName(IEnumerable<Employee> employees)
    {
        ArgumentNullException.ThrowIfNull(employees);

        return employees
            .OrderByDescending(e => e.Compensation.AnnualBaseSalary)
            .ThenBy(e => e.LastName)
            .ThenBy(e => e.FirstName)
            .ToList();
    }

    // Query 4: Group employees by role and count. Time: O(n), Space: O(u).
    public static Dictionary<RoleType, int> GroupAndCountByRole(IEnumerable<Employee> employees)
    {
        ArgumentNullException.ThrowIfNull(employees);

        return employees
            .GroupBy(e => e.Role)
            .ToDictionary(g => g.Key, g => g.Count());
    }

    // Query 5: Calculate payroll aggregates: Total, Avg, Min, Max. Time: O(n).
    public static (decimal Total, decimal Average, decimal Min, decimal Max) CalculateSalaryMetrics(IEnumerable<Employee> employees)
    {
        ArgumentNullException.ThrowIfNull(employees);
        var list = employees.ToList();

        if (list.Count == 0)
            return (0m, 0m, 0m, 0m);

        decimal total = list.Sum(e => e.Compensation.AnnualBaseSalary);
        decimal avg = list.Average(e => e.Compensation.AnnualBaseSalary);
        decimal min = list.Min(e => e.Compensation.AnnualBaseSalary);
        decimal max = list.Max(e => e.Compensation.AnnualBaseSalary);

        return (total, Math.Round(avg, 2), min, max);
    }

    // Query 6: Check if any employee meets salary threshold. Time: O(n) worst.
    public static bool HasHighEarner(IEnumerable<Employee> employees, decimal threshold)
    {
        ArgumentNullException.ThrowIfNull(employees);

        return employees.Any(e => e.Compensation.AnnualBaseSalary >= threshold);
    }

    // Query 7: Verify all employees match corporate email domain. Time: O(n).
    public static bool AllHaveCorporateEmail(IEnumerable<Employee> employees, string requiredDomain)
    {
        ArgumentNullException.ThrowIfNull(employees);
        ArgumentException.ThrowIfNullOrWhiteSpace(requiredDomain);

        return employees.All(e => e.Email.EndsWith(requiredDomain, StringComparison.OrdinalIgnoreCase));
    }

    // Query 8: Project unique sorted residential cities. Time: O(n log k).
    public static List<string> GetDistinctCities(IEnumerable<Employee> employees)
    {
        ArgumentNullException.ThrowIfNull(employees);

        return employees
            .Where(e => e.ResidentialAddress != null && !string.IsNullOrWhiteSpace(e.ResidentialAddress.City))
            .Select(e => e.ResidentialAddress!.City.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(c => c)
            .ToList();
    }
}
