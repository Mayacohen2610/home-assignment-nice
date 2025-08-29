using Microsoft.AspNetCore.Mvc;

namespace SuggestTaskService.Controllers
{
    // Handles HTTP requests for /suggestTask
    [ApiController]
    [Route("suggestTask")]
    public class SuggestTaskController : ControllerBase
    {
        // POST /suggestTask
        [HttpPost]
        public IActionResult Post()
        {
            // Step 1: simple response to verify routing works
            return Ok(new { message = "Hello from suggestTask" });
        }
    }
}
