using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace fintrack_api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CategoryController : Controller
    {
        public CategoryController()
        {
        }

        [HttpPost("add")]
        public IActionResult AddCategory ()
        {
            var userId = User.FindFirst("sub")?.Value;
            return Ok(userId);
        }
    }
}
