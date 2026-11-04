namespace EmployeeManagement.Domain.ValueObjects;

/// <summary>
/// Immutable value object representing a physical mailing address.
/// Uses C# 9+ record syntax for value-based equality.
/// </summary>
public record Address(
    string Street,
    string City,
    string State,
    string PostalCode,
    string Country = "USA")
{
    public override string ToString() => $"{Street}, {City}, {State} {PostalCode}, {Country}";
}

/// <summary>
/// Readonly record struct demonstrating value types, immutability, and zero allocation semantics.
/// </summary>
public readonly record struct Compensation(decimal BaseSalary, string Currency = "USD")
{
    public decimal AnnualBaseSalary => BaseSalary;

    public static Compensation FromMonthly(decimal monthlySalary, string currency = "USD")
    {
        if (monthlySalary < 0)
            throw new ArgumentOutOfRangeException(nameof(monthlySalary), "Monthly salary cannot be negative.");
        return new Compensation(monthlySalary * 12, currency);
    }
}

