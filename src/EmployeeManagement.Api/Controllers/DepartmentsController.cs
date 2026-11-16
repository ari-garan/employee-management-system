using EmployeeManagement.Services.DTOs;
using EmployeeManagement.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class DepartmentsController : ControllerBase
{
    private readonly IDepartmentService _departmentService;
    private readonly ILogger<DepartmentsController> _logger;

    public DepartmentsController(IDepartmentService departmentService, ILogger<DepartmentsController> logger)
    {
        _departmentService = departmentService;
        _logger = logger;
    }

    /// <summary>
    /// Gets department analytics, headcount, and budget utilization metrics for all departments.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<DepartmentSummaryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<DepartmentSummaryDto>>> GetDepartments(CancellationToken cancellationToken)
    {
        var summaries = await _departmentService.GetDepartmentSummariesAsync(cancellationToken);
        return Ok(summaries);
    }

    /// <summary>
    /// Gets analytics and budget utilization for a specific department.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(DepartmentSummaryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DepartmentSummaryDto>> GetDepartmentById(Guid id, CancellationToken cancellationToken)
    {
        var summary = await _departmentService.GetDepartmentSummaryByIdAsync(id, cancellationToken);
        if (summary == null)
        {
            return NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Department Not Found",
                Detail = $"Department with ID '{id}' was not found.",
                Instance = Request.Path
            });
        }

        return Ok(summary);
    }
}
