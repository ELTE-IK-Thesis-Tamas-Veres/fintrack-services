using fintrack_api_business_logic.Handlers.CategoryHandlers;
using fintrack_api_business_logic.Handlers.RecordHandlers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace fintrack_api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class RecordController : Controller
    {
        private readonly IMediator _mediator;

        public RecordController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("")]
        public async Task<IActionResult> GetRecords ()
        {
            try
            {
                uint userId = HttpContext.Items["userId"] as uint? ?? throw new Exception("userId not found");
                return Ok(await _mediator.Send(new GetRecordsCommand() { UserId = userId }));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
