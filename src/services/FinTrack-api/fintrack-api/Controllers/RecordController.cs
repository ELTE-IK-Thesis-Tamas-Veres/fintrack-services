using fintrack_api_business_logic.Handlers.CategoryHandlers;
using fintrack_api_business_logic.Handlers.RecordHandlers;
using fintrack_common.DTO.RecordDTO;
using fintrack_common.Exceptions;
using fintrack_database.Entities;
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

        [HttpPost("")]
        public async Task<IActionResult> CreateRecord([FromBody] CreateRecordRequest request)
        {
            try
            {
                uint userId = HttpContext.Items["userId"] as uint? ?? throw new Exception("userId not found");
                await _mediator.Send(new CreateRecordCommand(request, userId));
                return Ok();
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

        [HttpPut("{recordId}")]
        public async Task<IActionResult> EditRecord([FromRoute] uint recordId, [FromBody] EditRecordRequest request)
        {
            try
            {
                uint userId = HttpContext.Items["userId"] as uint? ?? throw new Exception("userId not found");
                await _mediator.Send(new EditRecordCommand(request) { UserId = userId, RecordId = recordId });
                return Ok();
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

        [HttpDelete("{recordId}")]
        public async Task<IActionResult> DeleteRecord([FromRoute] uint recordId)
        {
            try
            {
                uint userId = HttpContext.Items["userId"] as uint? ?? throw new Exception("userId not found");
                await _mediator.Send(new DeleteRecordCommand() { UserId = userId, RecordId = recordId });
                return Ok();
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
