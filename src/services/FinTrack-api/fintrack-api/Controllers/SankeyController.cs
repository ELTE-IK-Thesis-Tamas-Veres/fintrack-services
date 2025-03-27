using fintrack_api_business_logic.Handlers.RecordHandlers;
using fintrack_api_business_logic.Handlers.SankeyHandlers;
using fintrack_common.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace fintrack_api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class SankeyController : Controller
    {
        private readonly IMediator _mediator;

        public SankeyController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("")]
        public async Task<IActionResult> GetSankeyData([FromQuery] int? year, [FromQuery] int? month)
        {
            try
            {
                uint userId = HttpContext.Items["userId"] as uint? ?? throw new Exception("userId not found");
                return Ok(await _mediator.Send(new GetSankeyDataCommand() { UserId = userId, Month = month, Year = year }));
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
