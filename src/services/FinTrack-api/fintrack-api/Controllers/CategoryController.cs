using fintrack_api_business_logic.Handlers.CategoryHandlers;
using fintrack_common.DTO.CategoryDTO;
using fintrack_common.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace fintrack_api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class CategoryController : Controller
    {
        private readonly IMediator _mediator;
        public CategoryController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("tree")]
        public async Task<IActionResult> GetCategoryTree()
        {
            try
            {
                uint userId = HttpContext.Items["userId"] as uint? ?? throw new Exception("userId not found");
                return Ok(await _mediator.Send(new GetCategoryTreeCommand() { UserId = userId}));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("")]
        public async Task<IActionResult> GetCategories ()
        {
            try
            {
                uint userId = HttpContext.Items["userId"] as uint? ?? throw new Exception("userId not found");
                return Ok(await _mediator.Send(new GetCategoriesCommand() { UserId = userId }));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("")]
        public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryRequest request)
        {
            try
            {
                uint userId = HttpContext.Items["userId"] as uint? ?? throw new Exception("userId not found");

                await _mediator.Send(new CreateCategoryCommand() { UserId = userId, Name = request.Name });

                return Ok();
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

        [HttpPut("{categoryId}")]
        public async Task<IActionResult> EditCategory([FromBody] EditCategoryRequest request, [FromRoute] uint categoryId)
        {
            try
            {
                uint userId = HttpContext.Items["userId"] as uint? ?? throw new Exception("userId not found");

                await _mediator.Send(new EditCategoryCommand(request) { UserId = userId, CategoryId = categoryId });
                return Ok();
            }
            catch (RecordNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("")]
        public async Task<IActionResult> EditParentOfCategories([FromBody] EditParentOfCategoriesRequest request)
        {
            try
            {
                uint userId = HttpContext.Items["userId"] as uint? ?? throw new Exception("userId not found");

                await _mediator.Send(new EditParentOfCategoriesCommand(request) { UserId = userId });
                return Ok();
            }
            catch (RecordNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{categoryId}")]
        public async Task<IActionResult> DeleteCategory([FromRoute] uint categoryId)
        {
            try
            {
                uint userId = HttpContext.Items["userId"] as uint? ?? throw new Exception("userId not found");

                await _mediator.Send(new DeleteCategoryCommand() { CategoryId = categoryId, UserId = userId });

                return Ok();
            }
            catch (RecordNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        
    }
}
