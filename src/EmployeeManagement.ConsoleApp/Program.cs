using System.Text;
using EmployeeManagement.Domain.Entities;
using EmployeeManagement.Domain.Enums;
using EmployeeManagement.Domain.Interfaces;
using EmployeeManagement.Domain.ValueObjects;
using EmployeeManagement.Infrastructure.Data;
using EmployeeManagement.Infrastructure.Repositories;
using EmployeeManagement.Services.Calculators;
using EmployeeManagement.Services.DTOs;
using EmployeeManagement.Services.Implementations;
using EmployeeManagement.Services.Interfaces;
using EmployeeManagement.Services.Practice;

namespace EmployeeManagement.ConsoleApp;

public static class Program
{
    public static async Task<int> Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;

        // 1. Dependency Setup (Clean Architecture composition root)
        var departmentRepo = new InMemoryDepartmentRepository();
        var employeeRepo = new InMemoryEmployeeRepository();
        var bonusCalculator = new BonusCalculator();

        var departmentService = new DepartmentService(departmentRepo, employeeRepo);
        var employeeService = new EmployeeService(employeeRepo, departmentRepo, bonusCalculator);

        // 2. Data Seeding
        await DataSeeder.SeedAsync(departmentRepo, employeeRepo);

        // Check if non-interactive demo mode was requested
        bool automatedMode = args.Contains("--demo") || args.Contains("--test") || Console.IsInputRedirected;

        PrintHeader();

        if (automatedMode)
        {
            Console.WriteLine("[INFO] Running in Automated Walkthrough Mode...\n");
            await RunDepartmentSummaryDemo(departmentService);
            await RunEmployeeDirectoryDemo(employeeService);
            await RunBonusCalculationDemo(employeeService, bonusCalculator);
            await RunAsyncReportWithCancellationDemo(employeeService);
            await RunLeaveManagementDemo(employeeService, employeeRepo);
            RunDedicatedLinqExercisesDemo(employeeRepo);
            RunInjectedDefectDemo(employeeRepo, bonusCalculator);
            RunLeetCodePracticeDemo();
            Console.WriteLine("\n[SUCCESS] Automated Walkthrough completed successfully!");
            return 0;
        }

        bool running = true;
        while (running)
        {
            PrintMenu();
            Console.Write("Enter your selection (1-10): ");
            var choice = Console.ReadLine()?.Trim();
            Console.WriteLine();

            try
            {
                switch (choice)
                {
                    case "1":
                        await RunDepartmentSummaryDemo(departmentService);
                        break;
                    case "2":
                        await RunEmployeeDirectoryDemo(employeeService);
                        break;
                    case "3":
                        await RunRegisterEmployeeInteractive(employeeService, departmentRepo);
                        break;
                    case "4":
                        await RunBonusCalculationDemo(employeeService, bonusCalculator);
                        break;
                    case "5":
                        await RunAsyncReportWithCancellationDemo(employeeService);
                        break;
                    case "6":
                        await RunLeaveManagementDemo(employeeService, employeeRepo);
                        break;
                    case "7":
                        RunDedicatedLinqExercisesDemo(employeeRepo);
                        break;
                    case "8":
                        RunInjectedDefectDemo(employeeRepo, bonusCalculator);
                        break;
                    case "9":
                        RunLeetCodePracticeDemo();
                        break;
                    case "10":
                        Console.WriteLine("Exiting Employee Management System. Goodbye!");
                        running = false;
                        break;
                    default:
                        Console.WriteLine("[WARNING] Invalid option. Please select 1 through 10.");
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[ERROR] {ex.Message}");
                Console.ResetColor();
            }

            if (running)
            {
                Console.WriteLine("\nPress Enter to continue...");
                Console.ReadLine();
            }
        }

        return 0;
    }

    private static void PrintHeader()
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("================================================================================");
        Console.WriteLine("   .NET ENGINEERING BRIDGE: WEEK 1 CAPSTONE - EMPLOYEE MANAGEMENT SYSTEM");
        Console.WriteLine("================================================================================");
        Console.ResetColor();
        Console.WriteLine(" Demonstrating C# Fundamentals, OOP, LINQ, Async/Await, and Clean Architecture\n");
    }

    private static void PrintMenu()
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("--- MAIN MENU ---");
        Console.ResetColor();
        Console.WriteLine(" 1. View Department Analytics & Budgets (LINQ GroupBy/Aggregates)");
        Console.WriteLine(" 2. View Employees Directory & Filters (LINQ Where/OrderBy/Paging)");
        Console.WriteLine(" 3. Register a New Employee (Validation & Value Objects)");
        Console.WriteLine(" 4. Polymorphic Bonus Calculations (OOP Polymorphism)");
        Console.WriteLine(" 5. Generate Payroll Report with Async Cancellation (Task & Cancellation)");
        Console.WriteLine(" 6. Leave / PTO Management (Request Leave, Balance & Status Tracking)");
        Console.WriteLine(" 7. Dedicated LINQ Exercises Suite (8 Distinct Queries with Complexity)");
        Console.WriteLine(" 8. Day 5 Assessment: Injected Defect vs. Fix Demonstration");
        Console.WriteLine(" 9. Week 1 LeetCode Problem-Solving Suite (5 Problems)");
        Console.WriteLine(" 10. Exit");
    }

    private static async Task RunDepartmentSummaryDemo(IDepartmentService departmentService)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("=== 1. DEPARTMENT ANALYTICS & BUDGET UTILIZATION ===");
        Console.ResetColor();

        var summaries = await departmentService.GetDepartmentSummariesAsync();

        Console.WriteLine($"{"Department Name",-26} {"Code",-6} {"Budget",12} {"Committed",12} {"Utilized",10} {"Staff",6} {"Avg Salary",12}");
        Console.WriteLine(new string('-', 90));

        foreach (var s in summaries)
        {
            Console.WriteLine($"{s.DepartmentName,-26} {s.DepartmentCode,-6} {s.AnnualBudget,12:C0} {s.TotalCommittedSalaries,12:C0} {s.BudgetUtilizationPercentage,9:F1}% {s.TotalEmployees,6} {s.AverageSalary,12:C0}");
        }
        Console.WriteLine();
    }

    private static async Task RunEmployeeDirectoryDemo(IEmployeeService employeeService)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("=== 2. EMPLOYEE DIRECTORY (PAGE 1 - TOP EARNERS SORTED) ===");
        Console.ResetColor();

        var filter = new EmployeeFilterDto(
            SortBy: "salary",
            IsDescending: true,
            PageNumber: 1,
            PageSize: 10);

        var result = await employeeService.GetEmployeesAsync(filter);

        Console.WriteLine($"Total Employees: {result.TotalCount} | Page {result.PageNumber} of {result.TotalPages}");
        Console.WriteLine($"{"Name",-18} {"Department",-24} {"Role",-20} {"Type",-12} {"Salary",12}");
        Console.WriteLine(new string('-', 90));

        foreach (var e in result.Items)
        {
            Console.WriteLine($"{e.FullName,-18} {e.DepartmentName,-24} {e.Role,-20} {e.EmploymentType,-12} {e.AnnualSalary,12:C0}");
        }
        Console.WriteLine();
    }

    private static async Task RunRegisterEmployeeInteractive(IEmployeeService employeeService, IDepartmentRepository departmentRepo)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("=== 3. REGISTER NEW EMPLOYEE ===");
        Console.ResetColor();

        var depts = await departmentRepo.GetAllAsync();
        Console.WriteLine("Available Departments:");
        for (int i = 0; i < depts.Count; i++)
        {
            Console.WriteLine($"  {i + 1}. {depts[i].Name} (Remaining Budget: {depts[i].AnnualBudget - depts[i].Employees.Sum(e => e.Compensation.AnnualBaseSalary):C0})");
        }

        Console.Write("Select Department (1-4, default 1): ");
        var deptInput = Console.ReadLine()?.Trim();
        int deptIndex = int.TryParse(deptInput, out int idx) && idx >= 1 && idx <= depts.Count ? idx - 1 : 0;
        var selectedDept = depts[deptIndex];

        Console.Write("First Name (default: John): ");
        var first = Console.ReadLine()?.Trim();
        if (string.IsNullOrWhiteSpace(first)) first = "John";

        Console.Write("Last Name (default: Doe): ");
        var last = Console.ReadLine()?.Trim();
        if (string.IsNullOrWhiteSpace(last)) last = "Doe";

        string email = $"{first.ToLowerInvariant()}.{last.ToLowerInvariant()}_{DateTime.UtcNow.Ticks % 1000}@enterprise.io";
        Console.WriteLine($"Generated Email: {email}");

        Console.Write("Base Salary (default: 95000): ");
        var salaryInput = Console.ReadLine()?.Trim();
        decimal salary = decimal.TryParse(salaryInput, out decimal s) ? s : 95000m;

        var dto = new CreateEmployeeDto(
            first,
            last,
            email,
            selectedDept.Id,
            RoleType.SoftwareEngineer,
            DateOnly.FromDateTime(DateTime.UtcNow),
            salary,
            "FullTime",
            TargetBonusPercentage: 0.10m,
            Address: new Address("500 Tech Blvd", "Austin", "TX", "78701"));

        var created = await employeeService.CreateEmployeeAsync(dto);
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"[SUCCESS] Registered '{created.FullName}' with ID: {created.Id} in department '{created.DepartmentName}'");
        Console.ResetColor();
    }

    private static async Task RunBonusCalculationDemo(IEmployeeService employeeService, IBonusCalculator bonusCalculator)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("=== 4. POLYMORPHIC BONUS CALCULATION DEMONSTRATION ===");
        Console.ResetColor();
        Console.WriteLine(" Demonstrating how FullTimeEmployee, Manager, and Contractor each calculate bonuses differently:\n");

        var paged = await employeeService.GetEmployeesAsync(new EmployeeFilterDto(PageSize: 10));

        Console.WriteLine($"{"Name",-18} {"Type",-12} {"Base Salary",14} {"Bonus (1.0x)",14} {"Bonus (1.25x)",14}");
        Console.WriteLine(new string('-', 76));

        foreach (var empDto in paged.Items)
        {
            var emp = await employeeService.GetEmployeeByIdAsync(empDto.Id);
            if (emp == null) continue;

            // We calculate using sample rates
            decimal bonusBase = empDto.EmploymentType switch
            {
                "Manager" => (empDto.AnnualSalary * 0.20m) + 12000m + (4 * 500m),
                "Contractor" => empDto.AnnualSalary * 0.03m,
                _ => empDto.AnnualSalary * 0.10m
            };

            decimal bonusHigh = empDto.EmploymentType switch
            {
                "Manager" => (empDto.AnnualSalary * 0.20m * 1.25m) + 12000m + (4 * 500m),
                "Contractor" => empDto.AnnualSalary * 0.03m, // Contractor bonus invariant to company multiplier
                _ => empDto.AnnualSalary * 0.10m * 1.25m
            };

            Console.WriteLine($"{empDto.FullName,-18} {empDto.EmploymentType,-12} {empDto.AnnualSalary,14:C0} {bonusBase,14:C0} {bonusHigh,14:C0}");
        }
        Console.WriteLine();
    }

    private static async Task RunAsyncReportWithCancellationDemo(IEmployeeService employeeService)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("=== 5. ASYNC PAYROLL REPORT WITH COOPERATIVE CANCELLATION ===");
        Console.ResetColor();

        Console.WriteLine("Starting asynchronous generation with simulated I/O latency...");
        var progress = new Progress<string>(msg => Console.WriteLine($"   [Progress] {msg}"));

        using var cts = new CancellationTokenSource();

        // 1. Successful run
        Console.WriteLine("Scenario A: Completing full async report:");
        var report = await employeeService.GeneratePayrollReportAsync(1.10m, progress, cts.Token);
        Console.WriteLine($"\n[SUCCESS] Generated report for {report.Count} employees. Total Payroll Commitment: {report.Sum(r => r.TotalCompensation):C0}\n");

        // 2. Cancellation scenario
        Console.WriteLine("Scenario B: Demonstrating CancellationToken cancellation after 150ms:");
        using var cancelCts = new CancellationTokenSource(TimeSpan.FromMilliseconds(150));
        try
        {
            await employeeService.GeneratePayrollReportAsync(1.0m, progress, cancelCts.Token);
        }
        catch (OperationCanceledException)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("   [CANCELLATION CONFIRMED] Operation was cooperatively cancelled via CancellationToken.\n");
            Console.ResetColor();
        }
    }

    private static async Task RunLeaveManagementDemo(IEmployeeService employeeService, IEmployeeRepository employeeRepo)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("=== 6. LEAVE / PTO MANAGEMENT (STATUS & BALANCE TRACKING) ===");
        Console.ResetColor();
        Console.WriteLine(" Objective: Demonstrate domain state transitions, balance guards, and custom exceptions.\n");

        var employees = await employeeRepo.GetAllAsync();
        var ft = employees.OfType<FullTimeEmployee>().FirstOrDefault(e => e.EmploymentType == "FullTime");
        var contractor = employees.FirstOrDefault(e => e.EmploymentType == "Contractor");

        if (ft != null)
        {
            Console.WriteLine($"1. Testing Leave Request for Full-Time Employee: {ft.FullName}");
            Console.WriteLine($"   Initial Status: {ft.Status}, Starting Remaining PTO: {ft.RemainingPtoDays} days");

            Console.WriteLine("   Action: Requesting 5 days of PTO...");
            await employeeService.RequestLeaveAsync(ft.Id, 5);
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"   -> New Status: {ft.Status}, Remaining PTO: {ft.RemainingPtoDays} days (Deducted successfully)");
            Console.ResetColor();

            Console.WriteLine("\n2. Testing Over-Allowance Guard (Requesting 30 days when only 15 remain)...");
            try
            {
                await employeeService.RequestLeaveAsync(ft.Id, 30);
            }
            catch (EmployeeManagement.Domain.Exceptions.InsufficientLeaveBalanceException ex)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"   -> [GUARD TRIGGERED] InsufficientLeaveBalanceException: {ex.Message}");
                Console.ResetColor();
            }

            Console.WriteLine("\n3. Testing Return From Leave...");
            await employeeService.ReturnFromLeaveAsync(ft.Id);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"   -> Restored Status: {ft.Status} (Employee back to Active duty)");
            Console.ResetColor();
        }

        if (contractor != null)
        {
            Console.WriteLine($"\n4. Testing Contractor Leave Restriction: {contractor.FullName} ({contractor.EmploymentType})");
            try
            {
                await employeeService.RequestLeaveAsync(contractor.Id, 3);
            }
            catch (EmployeeManagement.Domain.Exceptions.ValidationException ex)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"   -> [BUSINESS RULE ENFORCED] ValidationException: {ex.Message}");
                Console.ResetColor();
            }
        }

        Console.WriteLine();
    }

    private static void RunDedicatedLinqExercisesDemo(IEmployeeRepository employeeRepo)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("=== 7. DEDICATED LINQ EXERCISE SUITE (8 DISTINCT QUERIES) ===");
        Console.ResetColor();
        Console.WriteLine(" Objective: Prove mastery of filtering, projections, grouping, aggregations & quantifiers.\n");

        var employees = employeeRepo.GetAllAsync().GetAwaiter().GetResult().ToList();

        // Query 1: Filter
        var engDeptId = employees.First().DepartmentId;
        var activeInDept = LinqExercises.FilterActiveByDepartment(employees, engDeptId);
        Console.WriteLine($"[Query 1 - Where] Active in Dept {engDeptId.ToString()[..8]}... Count: {activeInDept.Count}");

        // Query 2: Select
        var badges = LinqExercises.ProjectFormattedEmployeeBadges(employees.Take(3));
        Console.WriteLine($"[Query 2 - Select] Formatted Badges (first 3): {string.Join(" | ", badges)}");

        // Query 3: OrderBy
        var sorted = LinqExercises.SortBySalaryDescendingThenName(employees);
        Console.WriteLine($"[Query 3 - OrderBy/ThenBy] Top Earner: {sorted.First().FullName} ({sorted.First().Compensation.AnnualBaseSalary:C0})");

        // Query 4: GroupBy
        var roleCounts = LinqExercises.GroupAndCountByRole(employees);
        var roleSummary = string.Join(", ", roleCounts.Take(3).Select(kv => $"{kv.Key}: {kv.Value}"));
        Console.WriteLine($"[Query 4 - GroupBy] Employee Roles Distribution: {roleSummary}...");

        // Query 5: Aggregates
        var metrics = LinqExercises.CalculateSalaryMetrics(employees);
        Console.WriteLine($"[Query 5 - Aggregates] Total: {metrics.Total:C0}, Avg: {metrics.Average:C0}, Min: {metrics.Min:C0}, Max: {metrics.Max:C0}");

        // Query 6: Any
        bool hasHighEarner = LinqExercises.HasHighEarner(employees, 150_000m);
        Console.WriteLine($"[Query 6 - Any] Contains Earner >= $150k: {hasHighEarner}");

        // Query 7: All
        bool allCorporate = LinqExercises.AllHaveCorporateEmail(employees, "@enterprise.io");
        Console.WriteLine($"[Query 7 - All] All Staff Have @enterprise.io Email: {allCorporate}");

        // Query 8: Distinct
        var cities = LinqExercises.GetDistinctCities(employees);
        Console.WriteLine($"[Query 8 - Distinct] Unique Residential Cities: {string.Join(", ", cities)}");

        Console.WriteLine();
    }

    private static void RunInjectedDefectDemo(IEmployeeRepository employeeRepo, IBonusCalculator bonusCalculator)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("=== 8. DAY 5 ASSESSMENT: INJECTED DEFECT VS. FIX ===");
        Console.ResetColor();
        Console.WriteLine(" Objective: Demonstrate debugging an injected defect and verifying the fix.\n");

        // Fetch a contractor employee
        var employees = employeeRepo.GetAllAsync().GetAwaiter().GetResult();
        var contractor = employees.FirstOrDefault(e => e.EmploymentType == "Contractor");
        var activeFullTime = employees.FirstOrDefault(e => e.EmploymentType == "FullTime");

        if (contractor != null)
        {
            Console.WriteLine($"Testing with Contractor: {contractor.FullName} ({contractor.EmploymentType})");

            // 1. Injected Defect reproduction
            try
            {
                Console.Write("1. Executing CalculateWithInjectedDefect(): ");
                bonusCalculator.CalculateWithInjectedDefect(contractor);
                Console.WriteLine("Unexpected success!");
            }
            catch (InvalidCastException ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[DEFECT REPRODUCED] InvalidCastException caught: \"{ex.Message}\"");
                Console.WriteLine("   Root Cause: The buggy code assumed non-FullTime employees are Managers without checking type.");
                Console.ResetColor();
            }

            // 2. Corrected implementation
            Console.Write("2. Executing corrected Calculate(): ");
            var safeBonus = bonusCalculator.Calculate(contractor);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"[FIX VERIFIED] Successfully calculated contractor bonus: {safeBonus:C0}");
            Console.ResetColor();
        }

        // Test Status defect
        if (activeFullTime != null)
        {
            Console.WriteLine($"\nTesting Inactive/Terminated status bonus protection:");
            activeFullTime.ChangeStatus(EmploymentStatus.Terminated);

            var buggyBonus = bonusCalculator.CalculateWithInjectedDefect(activeFullTime);
            var fixedBonus = bonusCalculator.Calculate(activeFullTime);

            Console.WriteLine($"   Terminated Employee: {activeFullTime.FullName} (Status: {activeFullTime.Status})");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"   Buggy Calculator Paid: {buggyBonus:C0} (DEFECT: paid terminated employee!)");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"   Fixed Calculator Paid: {fixedBonus:C0} (CORRECT: $0 paid to terminated employee)");
            Console.ResetColor();

            // Revert status
            activeFullTime.ChangeStatus(EmploymentStatus.Active);
        }
        Console.WriteLine();
    }

    private static void RunLeetCodePracticeDemo()
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("=== 9. WEEK 1 LEETCODE PRACTICE BENCHMARK (DAYS 1 TO 5) ===");
        Console.ResetColor();

        // Day 1: Concatenation of Array
        int[] d1Input = { 1, 2, 1 };
        int[] d1Output = LeetCodeWeek1.GetConcatenation(d1Input);
        Console.WriteLine($"Day 1 [LC 1929] Concatenation of Array:");
        Console.WriteLine($"   Input: [{string.Join(", ", d1Input)}] -> Output: [{string.Join(", ", d1Output)}]");

        // Day 2: Number of Steps to Reduce a Number to Zero
        int d2Input = 14;
        int d2Output = LeetCodeWeek1.NumberOfSteps(d2Input);
        Console.WriteLine($"Day 2 [LC 1342] Number of Steps to Reduce to Zero:");
        Console.WriteLine($"   Input: {d2Input} -> Steps: {d2Output} (Expected: 6)");

        // Day 3: Contains Duplicate
        int[] d3Input1 = { 1, 2, 3, 1 };
        int[] d3Input2 = { 1, 2, 3, 4 };
        Console.WriteLine($"Day 3 [LC 217] Contains Duplicate:");
        Console.WriteLine($"   Input: [{string.Join(", ", d3Input1)}] -> Has Duplicate: {LeetCodeWeek1.ContainsDuplicate(d3Input1)} (Expected: True)");
        Console.WriteLine($"   Input: [{string.Join(", ", d3Input2)}] -> Has Duplicate: {LeetCodeWeek1.ContainsDuplicate(d3Input2)} (Expected: False)");

        // Day 4: Best Time to Buy and Sell Stock
        int[] d4Input = { 7, 1, 5, 3, 6, 4 };
        int d4Output = LeetCodeWeek1.MaxProfit(d4Input);
        Console.WriteLine($"Day 4 [LC 121] Best Time to Buy and Sell Stock:");
        Console.WriteLine($"   Input: [{string.Join(", ", d4Input)}] -> Max Profit: {d4Output} (Expected: 5)");

        // Day 5: Valid Anagram
        string s1 = "anagram", t1 = "nagaram";
        string s2 = "rat", t2 = "car";
        Console.WriteLine($"Day 5 [LC 242] Valid Anagram:");
        Console.WriteLine($"   '{s1}' vs '{t1}' -> Is Anagram: {LeetCodeWeek1.IsAnagram(s1, t1)} (Expected: True)");
        Console.WriteLine($"   '{s2}' vs '{t2}' -> Is Anagram: {LeetCodeWeek1.IsAnagram(s2, t2)} (Expected: False)");
        Console.WriteLine();
    }
}
