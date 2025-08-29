using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using SuggestTaskService.Models;

namespace SuggestTaskService.Controllers
{
    // Handles HTTP requests for /suggestTask
    [ApiController]
    [Route("suggestTask")]
    public class SuggestTaskController : ControllerBase
    {
        
        [HttpPost] // Handle POST requests
        [ProducesResponseType(typeof(object), 200)] // Successful response type
        [ProducesResponseType(typeof(object), 400)] // Bad request response type
        public IActionResult Post([FromBody] SuggestTaskRequest req)   // Bind JSON body to SuggestTaskRequest
        {
            // logging of the incoming JSON 
            Console.WriteLine($"[INFO] Incoming: userId={req?.userId}, sessionId={req?.sessionId}, utterance='{req?.utterance}', ts='{req?.timestamp}'");
            
            //  Validate input 
            var (ok, error) = ValidateRequest(req); 
            if (!ok)
            {
                Console.WriteLine($"[WARN] Validation failed: {error}");
                return BadRequest(new { error }); // Return 400 Bad Request with error message
            }

            // log message to confirm validation passed
            Console.WriteLine("[INFO] Validation passed");
            return Ok(new
            {
                message = "Validation OK",
                receivedAt = DateTime.UtcNow.ToString("o")
            });
        }
        // function to validate the incoming JSON request
        private static (bool ok, string? error) ValidateRequest(SuggestTaskRequest? req)
        {
            // Check for null request
            if (req is null) return (false, "invalid JSON body");

            // Validate required string fields (non-empty)
            if (string.IsNullOrWhiteSpace(req.utterance)) return (false, "utterance is required");
            if (string.IsNullOrWhiteSpace(req.userId))   return (false, "userId is required");
            if (string.IsNullOrWhiteSpace(req.sessionId))return (false, "sessionId is required");

            // Validate ISO-8601 timestamp (e.g., 2025-08-21T12:00:00Z)
            if (string.IsNullOrWhiteSpace(req.timestamp))
                return (false, "timestamp is required");

            if (!DateTime.TryParse(req.timestamp,
                                   CultureInfo.InvariantCulture,
                                   DateTimeStyles.RoundtripKind,
                                   out _))
            {
                return (false, "timestamp must be ISO-8601 (e.g., 2025-08-21T12:00:00Z)");
            }

            return (true, null);
        }
    }
}
