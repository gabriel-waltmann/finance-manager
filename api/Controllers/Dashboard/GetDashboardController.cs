using api.Requests.Dashboard;
using api.Responses.Dashboard;
using api.Services.Dashboard;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers.Dashboard;

[ApiController]
[Tags("Dashboard")]
[Route("/dashboard")]
public class GetDashboardController(
    ILogger<GetDashboardController> logger,
    DashboardService service
) : ControllerBase
{
    private readonly ILogger<GetDashboardController> _logger = logger;
    private readonly DashboardService _service = service;

    [HttpGet]
    public async Task<ActionResult<GetDashboardResponse>> ExecuteAsync(
        [FromQuery] GetDashboardRequest request
    )
    {
        try
        {
            request.Order = request.Order.Trim().ToLowerInvariant();

            if (request.Order != "asc" && request.Order != "desc")
            {
                return BadRequest(new { error = "Order must be asc or desc." });
            }

            if (
                request.StartDate.HasValue &&
                request.EndDate.HasValue &&
                request.StartDate.Value.Date > request.EndDate.Value.Date
            )
            {
                return BadRequest(new { error = "Start date must be before or equal to end date." });
            }

            var response = await _service.Get(request);

            return StatusCode(200, response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[GetDashboardController]");

            return StatusCode(500, new { error = "Internal server error" });
        }
    }
}
