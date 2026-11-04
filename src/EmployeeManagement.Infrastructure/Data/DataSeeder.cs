using EmployeeManagement.Domain.Entities;
using EmployeeManagement.Domain.Enums;
using EmployeeManagement.Domain.Interfaces;
using EmployeeManagement.Domain.ValueObjects;

namespace EmployeeManagement.Infrastructure.Data;

/// <summary>
/// Seeds realistic company data representing departments, staff, managers, and contractors.
/// </summary>
public static class DataSeeder
{
    public static async Task SeedAsync(
        IDepartmentRepository departmentRepo,
        IEmployeeRepository employeeRepo,
        CancellationToken cancellationToken = default)
    {
        var existingDepts = await departmentRepo.GetAllAsync(cancellationToken);
        if (existingDepts.Count > 0)
            return;

        // 1. Create Departments
        var engDept = new Department("Engineering & Technology", DepartmentCode.Engineering, 1_500_000m);
        var hrDept = new Department("People & Culture", DepartmentCode.HumanResources, 500_000m);
        var finDept = new Department("Finance & Accounting", DepartmentCode.Finance, 650_000m);
        var mktDept = new Department("Product & Marketing", DepartmentCode.Marketing, 550_000m);

        await departmentRepo.AddAsync(engDept, cancellationToken);
        await departmentRepo.AddAsync(hrDept, cancellationToken);
        await departmentRepo.AddAsync(finDept, cancellationToken);
        await departmentRepo.AddAsync(mktDept, cancellationToken);

        // 2. Create Managers
        var engManager = new Manager(
            "Sarah",
            "Connor",
            "sarah.connor@enterprise.io",
            engDept.Id,
            RoleType.EngineeringManager,
            new DateOnly(2021, 3, 15),
            new Compensation(145_000m),
            targetBonusPercentage: 0.20m,
            managementAllowance: 15_000m,
            new Address("100 Innovation Way", "Austin", "TX", "78701"));

        var hrManager = new Manager(
            "Michael",
            "Scott",
            "michael.scott@enterprise.io",
            hrDept.Id,
            RoleType.Director,
            new DateOnly(2019, 5, 1),
            new Compensation(115_000m),
            targetBonusPercentage: 0.15m,
            managementAllowance: 10_000m,
            new Address("1725 Slough Ave", "Scranton", "PA", "18503"));

        await employeeRepo.AddAsync(engManager, cancellationToken);
        await employeeRepo.AddAsync(hrManager, cancellationToken);

        // 3. Create Full-time Engineers & Staff
        var emp1 = new FullTimeEmployee(
            "Alice",
            "Smith",
            "alice.smith@enterprise.io",
            engDept.Id,
            RoleType.SeniorEngineer,
            new DateOnly(2022, 1, 10),
            new Compensation(120_000m),
            targetBonusPercentage: 0.12m,
            ptoDays: 22,
            new Address("45 Silicon Blvd", "Austin", "TX", "78702"));

        var emp2 = new FullTimeEmployee(
            "Bob",
            "Jones",
            "bob.jones@enterprise.io",
            engDept.Id,
            RoleType.SoftwareEngineer,
            new DateOnly(2023, 6, 1),
            new Compensation(92_000m),
            targetBonusPercentage: 0.10m,
            ptoDays: 20,
            new Address("78 River St", "Austin", "TX", "78703"));

        var emp3 = new FullTimeEmployee(
            "Elena",
            "Rostova",
            "elena.rostova@enterprise.io",
            engDept.Id,
            RoleType.StaffEngineer,
            new DateOnly(2020, 8, 15),
            new Compensation(160_000m),
            targetBonusPercentage: 0.18m,
            ptoDays: 25,
            new Address("220 Mountain View Rd", "Austin", "TX", "78704"));

        var emp4 = new FullTimeEmployee(
            "Pam",
            "Beesly",
            "pam.beesly@enterprise.io",
            hrDept.Id,
            RoleType.HrSpecialist,
            new DateOnly(2022, 4, 1),
            new Compensation(68_000m),
            targetBonusPercentage: 0.08m,
            ptoDays: 18,
            new Address("310 Elm St", "Scranton", "PA", "18504"));

        var emp5 = new FullTimeEmployee(
            "Oscar",
            "Martinez",
            "oscar.martinez@enterprise.io",
            finDept.Id,
            RoleType.FinancialAnalyst,
            new DateOnly(2021, 9, 20),
            new Compensation(88_000m),
            targetBonusPercentage: 0.10m,
            ptoDays: 20,
            new Address("55 Maple Ave", "Scranton", "PA", "18505"));

        var emp6 = new FullTimeEmployee(
            "Kelly",
            "Kapoor",
            "kelly.kapoor@enterprise.io",
            mktDept.Id,
            RoleType.MarketingSpecialist,
            new DateOnly(2023, 2, 14),
            new Compensation(72_000m),
            targetBonusPercentage: 0.08m,
            ptoDays: 18,
            new Address("12 Fashion Blvd", "New York", "NY", "10001"));

        // 4. Create Contractor
        var contractor1 = new ContractEmployee(
            "David",
            "Kim",
            "david.contractor@partner-tech.com",
            engDept.Id,
            RoleType.SoftwareEngineer,
            new DateOnly(2024, 1, 15),
            hourlyRate: 65.00m,
            contractEndDate: new DateOnly(2024, 12, 31),
            agencyName: "Apex Systems",
            new Address("404 Remote Ave", "Seattle", "WA", "98101"));

        await employeeRepo.AddAsync(emp1, cancellationToken);
        await employeeRepo.AddAsync(emp2, cancellationToken);
        await employeeRepo.AddAsync(emp3, cancellationToken);
        await employeeRepo.AddAsync(emp4, cancellationToken);
        await employeeRepo.AddAsync(emp5, cancellationToken);
        await employeeRepo.AddAsync(emp6, cancellationToken);
        await employeeRepo.AddAsync(contractor1, cancellationToken);

        // Link direct reports to engineering manager
        engManager.AddDirectReport(emp1.Id);
        engManager.AddDirectReport(emp2.Id);
        engManager.AddDirectReport(emp3.Id);
        engManager.AddDirectReport(contractor1.Id);
        await employeeRepo.UpdateAsync(engManager, cancellationToken);

        hrManager.AddDirectReport(emp4.Id);
        await employeeRepo.UpdateAsync(hrManager, cancellationToken);

        // Assign employees to departments
        engDept.AssignEmployee(engManager);
        engDept.AssignEmployee(emp1);
        engDept.AssignEmployee(emp2);
        engDept.AssignEmployee(emp3);
        engDept.AssignEmployee(contractor1);
        await departmentRepo.UpdateAsync(engDept, cancellationToken);

        hrDept.AssignEmployee(hrManager);
        hrDept.AssignEmployee(emp4);
        await departmentRepo.UpdateAsync(hrDept, cancellationToken);

        finDept.AssignEmployee(emp5);
        await departmentRepo.UpdateAsync(finDept, cancellationToken);

        mktDept.AssignEmployee(emp6);
        await departmentRepo.UpdateAsync(mktDept, cancellationToken);
    }
}

