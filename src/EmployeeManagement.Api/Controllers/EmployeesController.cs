using EmployeeManagement.Api.Models;
using EmployeeManagement.Api.Validators;
using EmployeeManagement.Domain.Enums;
using EmployeeManagement.Services.DTOs;
using EmployeeManagement.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class EmployeesController : ControllerBase
{
    private readonly IEmployeeService _employeeService;
    private readonly ILogger<EmployeesController> _logger;

    public EmployeesController(IEmployeeService employeeService, ILogger<EmployeesController> logger)
    {
        _employeeService = employeeService;
        _logger = logger;
    }

    /// <summary>
    /// Gets a paginated list of employees with optional department, status, salary, and search filters.
    /// Public endpoint.
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(PagedResult<EmployeeDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<EmployeeDto>>> GetEmployees(
        [FromQuery] Guid? departmentId,
        [FromQuery] EmploymentStatus? status,
        [FromQuery] string? search,
        [FromQuery] decimal? minSalary,
        [FromQuery] decimal? maxSalary,
        [FromQuery] string? sortBy = "name",
        [FromQuery] bool desc = false,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var filter = new EmployeeFilterDto(
            DepartmentId: departmentId,
            Status: status,
            SearchTerm: search,
            MinSalary: minSalary,
            MaxSalary: maxSalary,
            SortBy: sortBy,
            IsDescending: desc,
            PageNumber: page,
            PageSize: pageSize);

        var result = await _employeeService.GetEmployeesAsync(filter, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Gets a specific employee by unique identifier.
    /// Public endpoint.
    /// </summary>
    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(EmployeeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EmployeeDto>> GetEmployeeById(Guid id, CancellationToken cancellationToken)
    {
        var employee = await _employeeService.GetEmployeeByIdAsync(id, cancellationToken);
        if (employee == null)
        {
            return NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Employee Not Found",
                Detail = $"Employee with ID '{id}' was not found.",
                Instance = Request.Path
            });
        }

        return Ok(employee);
    }

    /// <summary>
    /// Retrieves the top compensation earners across the organization.
    /// Public endpoint.
    /// </summary>
    [HttpGet("top-earners")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IReadOnlyList<EmployeeDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<EmployeeDto>>> GetTopEarners(
        [FromQuery] int count = 5,
        CancellationToken cancellationToken = default)
    {
        var earners = await _employeeService.GetTopEarnersAsync(Math.Clamp(count, 1, 50), cancellationToken);
        return Ok(earners);
    }

    /// <summary>
    /// Onboards a new employee into the organization.
    /// Protected endpoint: Requires Admin or Manager role.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(EmployeeDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<EmployeeDto>> CreateEmployee(
        [FromBody] CreateEmployeeDto dto,
        CancellationToken cancellationToken)
    {
        // Day 8 Request Validation
        dto.Validate();

        _logger.LogInformation("Creating new employee {FirstName} {LastName} in department {DepartmentId}",
            dto.FirstName, dto.LastName, dto.DepartmentId);

        var created = await _employeeService.CreateEmployeeAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetEmployeeById), new { id = created.Id }, created);
    }

    /// <summary>
    /// Updates an employee's employment status (Active, OnLeave, Suspended, Terminated).
    /// Protected endpoint: Requires Admin or Manager role.
    /// </summary>
    [HttpPatch("{id:guid}/status")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdateStatus(
        Guid id,
        [FromBody] UpdateStatusRequest request,
        CancellationToken cancellationToken)
    {
        await _employeeService.UpdateEmployeeStatusAsync(id, request.Status, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Promotes an employee to a new role and adjusts their base compensation.
    /// Protected endpoint: Requires Admin role only (Strict privilege check).
    /// </summary>
    [HttpPatch("{id:guid}/promote")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> PromoteEmployee(
        Guid id,
        [FromBody] PromoteEmployeeRequest request,
        CancellationToken cancellationToken)
    {
        // Day 8 Request Validation
        request.Validate();

        await _employeeService.PromoteEmployeeAsync(id, request.NewRole, request.NewSalary, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Submits a Paid Time Off (PTO) leave request for a full-time employee.
    /// Protected endpoint: Requires authenticated user.
    /// </summary>
    [HttpPost("{id:guid}/leave")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RequestLeave(
        Guid id,
        [FromBody] LeaveRequestModel request,
        CancellationToken cancellationToken)
    {
        // Day 8 Request Validation
        request.Validate();

        await _employeeService.RequestLeaveAsync(id, request.Days, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Returns an employee back to Active status from leave.
    /// Protected endpoint: Requires Admin or Manager role.
    /// </summary>
    [HttpPost("{id:guid}/return-leave")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ReturnFromLeave(Guid id, CancellationToken cancellationToken)
    {
        await _employeeService.ReturnFromLeaveAsync(id, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Simulates and generates an asynchronous payroll and bonus compensation disbursement report.
    /// Protected endpoint: Requires Admin or Manager role.
    /// </summary>
    [HttpGet("payroll-report")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(IReadOnlyList<PayrollReportItem>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IReadOnlyList<PayrollReportItem>>> GetPayrollReport(
        [FromQuery] decimal companyMultiplier = 1.0m,
        CancellationToken cancellationToken = default)
    {
        var report = await _employeeService.GeneratePayrollReportAsync(companyMultiplier, null, cancellationToken);
        return Ok(report);
    }
}
