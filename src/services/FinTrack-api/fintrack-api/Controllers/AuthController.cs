using fintrack_api_business_logic.Handlers;
using fintrack_common.DTO.AuthDTO;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace fintrack_api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : Controller
    {
        private readonly IMediator _mediator;
        public AuthController (IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("login")]
        public IActionResult Login(CancellationToken cancellationToken)
        {
            string idToken = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                 ?? HttpContext.User.FindFirst("sub")?.Value;
            return Ok(new { Be = "vagy autholva geci" });
        }
    }
}
