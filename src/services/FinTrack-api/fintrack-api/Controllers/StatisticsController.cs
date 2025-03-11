using fintrack_api_business_logic.Handlers.StatisticsHandlers;
using fintrack_common.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace fintrack_api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class StatisticsController : Controller
    {
        private readonly IMediator _mediator;

        public StatisticsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("lastYearMonthly")]
        public async Task<IActionResult> GetLastYearMonthlyStatistics()
        {
            try
            {
                uint userId = HttpContext.Items["userId"] as uint? ?? throw new Exception("userId not found");
                return Ok(await _mediator.Send(new GetLastYearMonthlyStatisticsCommand() { UserId = userId }));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (RecordNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("lastYearCategoryMonthly")]
        public async Task<IActionResult> GetLastYearCategoryMonthlyStatistics([FromQuery] uint categoryId)
        {
            try
            {
                uint userId = HttpContext.Items["userId"] as uint? ?? throw new Exception("userId not found");
                return Ok(await _mediator.Send(new GetLastYearCategoryMonthlyStatisticsCommand() { UserId = userId, CategoryId = categoryId }));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (RecordNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
