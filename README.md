# .NET Engineering Bridge — Week 1 Capstone: Employee Management System

> **Week 1 Gate 1 Implementation**: A structured, modular C# domain and application layer built with clean architecture, ready to be converted into an ASP.NET Core Web API in Week 2.

---

## 1. Solution Architecture

The solution is divided into cohesive, decoupled projects following Onion/Clean Architecture principles:

```
EmployeeManagement/
├── EmployeeManagement.sln
├── nuget.config
├── src/
│   ├── EmployeeManagement.Domain/          # Core Domain Layer
│   │   ├── Common/BaseEntity.cs            # Identity, auditing, encapsulation
│   │   ├── Enums/DomainEnums.cs            # DepartmentCode, EmploymentStatus, RoleType
│   │   ├── ValueObjects/ValueObjects.cs    # Address (record) & Compensation (readonly record struct)
│   │   ├── Entities/
│   │   │   ├── Department.cs               # Department aggregate root with budget rules
│   │   │   ├── Employee.cs                 # Abstract base employee with polymorphic methods
│   │   │   ├── FullTimeEmployee.cs         # Target bonus % & PTO calculations
│   │   │   ├── ContractEmployee.cs         # Hourly rate, agency, fixed bonus
│   │   │   └── Manager.cs                  # Direct reports, allowances, team incentives
│   │   ├── Exceptions/DomainExceptions.cs  # DomainException, EntityNotFoundException, ValidationException, BudgetExceededException
│   │   └── Interfaces/
│   │       ├── IRepository.cs              # Generic async repository contract
│   │       └── IDomainRepositories.cs      # IEmployeeRepository & IDepartmentRepository
│   │
│   ├── EmployeeManagement.Infrastructure/  # In-Memory Persistence Layer
│   │   ├── Repositories/
│   │   │   ├── InMemoryRepository.cs       # Generic thread-safe ConcurrentDictionary store
│   │   │   ├── InMemoryEmployeeRepository.cs # Specialized LINQ queries (top earners, by status)
│   │   │   └── InMemoryDepartmentRepository.cs
│   │   └── Data/DataSeeder.cs              # Realistic seed dataset (4 departments, 9 employees)
│   │
│   ├── EmployeeManagement.Services/        # Business Logic & Orchestration
│   │   ├── DTOs/ServiceDtos.cs             # CreateEmployeeDto, EmployeeDto, DepartmentSummaryDto, PagedResult
│   │   ├── Interfaces/IServiceInterfaces.cs# IEmployeeService, IDepartmentService, IBonusCalculator
│   │   ├── Calculators/BonusCalculator.cs  # Polymorphic calculation + Day 5 Injected Defect demo
│   │   ├── Implementations/
│   │   │   ├── EmployeeService.cs          # LINQ pipeline (Where, OrderBy, Skip, Take), async reporting
│   │   │   └── DepartmentService.cs        # LINQ aggregation (GroupBy, Sum, Average, Min, Max)
│   │   └── Practice/LeetCodeWeek1.cs       # Days 1–5 LeetCode problem solutions & Big-O analysis
│   │
│   └── EmployeeManagement.ConsoleApp/       # Presentation & Interactive CLI
│       └── Program.cs                      # Interactive CLI menu + automated `--demo` mode
│
└── tests/
    └── EmployeeManagement.Tests/           # Automated xUnit Test Suite (38 tests)
        ├── EmployeeDomainTests.cs          # Domain business rules, OOP, records equality
        ├── LinqAndServiceTests.cs          # LINQ filtering, sorting, pagination, aggregations
        ├── AsyncAndDefectTests.cs          # Async cancellation tokens & Day 5 defect reproduction
        └── LeetCodePracticeTests.cs        # Tests for all 5 LeetCode problems & edge cases
```

---

## 2. Week 1 Syllabus Objectives Demonstrated

| Day | Topic | Implementation in Solution |
|---|---|---|
| **Day 1** | C# Fundamentals, Value vs Reference types, Nullables | `Compensation` (`readonly record struct`), `Address` (`record`), null checks with `ArgumentNullException.ThrowIfNull()`, top-level flow in `Program.cs`. |
| **Day 2** | OOP, Abstraction, Polymorphism, SOLID | Abstract `Employee` base class with polymorphic `CalculateAnnualBonus()`. Concrete implementations: `FullTimeEmployee`, `ContractEmployee`, `Manager`. Encapsulation of fields. |
| **Day 3** | Modern C# & LINQ | `GroupBy`, `Sum`, `Average`, `Min`, `Max` in `DepartmentService`. `Where`, `OrderBy`, `Skip`, `Take` with deferred execution in `EmployeeService`. |
| **Day 4** | Async/Await, Tooling, `CancellationToken` | Multi-project `.sln`. Asynchronous I/O simulation in `GeneratePayrollReportAsync()` with cooperative cancellation using `CancellationToken.ThrowIfCancellationRequested()`. |
| **Day 5** | Checkpoint, Refactoring & Defect Fix | Injected defect in `BonusCalculator.CalculateWithInjectedDefect()` (casting failure on contractors and paying terminated staff), verified with automated tests in `AsyncAndDefectTests.cs`. |

---

## 3. Day 5 Assessment: Injected Defect & Fix Analysis

### The Injected Defect (`CalculateWithInjectedDefect`)
1. **Defect 1 — Missed Status Validation**: The calculation did not check `employee.Status`, erroneously issuing annual bonuses to `Terminated` employees.
2. **Defect 2 — Unsafe Downcast**: The code assumed that any non-`FullTimeEmployee` must be a `Manager`:
   ```csharp
   // BUGGY CODE:
   var mgr = (Manager)employee; // Throws InvalidCastException when employee is a ContractEmployee!
   ```

### The Corrected Implementation (`Calculate`)
1. **Status Guard**: Early check `if (employee.Status != EmploymentStatus.Active) return 0m;`.
2. **Polymorphic Delegation**: Delegates to `employee.CalculateAnnualBonus(companyMultiplier)`, letting each subclass calculate its own compensation without brittle casting.

---

## 4. Week 1 LeetCode Solutions (`LeetCodeWeek1.cs`)

| Day | Problem | LeetCode # | Time Complexity | Space Complexity |
|---|---|---|---|---|
| Day 1 | Concatenation of Array | LC 1929 | $\mathcal{O}(n)$ | $\mathcal{O}(n)$ |
| Day 2 | Number of Steps to Reduce a Number to Zero | LC 1342 | $\mathcal{O}(\log n)$ | $\mathcal{O}(1)$ |
| Day 3 | Contains Duplicate | LC 217 | $\mathcal{O}(n)$ | $\mathcal{O}(n)$ |
| Day 4 | Best Time to Buy and Sell Stock | LC 121 | $\mathcal{O}(n)$ | $\mathcal{O}(1)$ |
| Day 5 | Valid Anagram | LC 242 | $\mathcal{O}(n)$ | $\mathcal{O}(1)$ |

---

## 5. How to Run and Test

### Prerequisites
* .NET 8.0 SDK (installed at `C:\Users\Arigaran\.dotnet\dotnet.exe`).

### Run Automated Walkthrough
```powershell
dotnet run --project src/EmployeeManagement.ConsoleApp -- --demo
```

### Run Interactive Console Menu
```powershell
dotnet run --project src/EmployeeManagement.ConsoleApp
```

### Run All 38 Unit Tests
```powershell
dotnet test EmployeeManagement.sln
```
All 38 tests cover domain business rules, LINQ queries, async cancellation, defect reproduction, and LeetCode edge cases.

