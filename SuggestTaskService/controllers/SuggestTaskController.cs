using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using SuggestTaskService.Models;
using System.Text.RegularExpressions;


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

        // Bind JSON body to SuggestTaskRequest
        public IActionResult Post([FromBody] SuggestTaskRequest req)   
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

            // Match task based on utterance
            var task = MatchTask(req.utterance!); // req.utterance is non-null due to validation
            Console.WriteLine($"[INFO] Selected task='{task}' for userId={req.userId}");

            // Response with ISO-8601 timestamp
            return Ok(new
            {
                task,
                timestamp = DateTime.UtcNow.ToString("o")
            });
        }

        // Define patterns for task suggestion using regex
        private static readonly (Regex pattern, string task)[] _patterns = new[]
        {
            // ResetPasswordTask: all phrases with "reset/ting", "forgot", "can't/cannot remember", "lost", "recover/ing" + "password/s" or vice versa
            (
                new Regex(
                @"\b(?:reset(?:ting)?|forgot|can(?:'|no)t\s*remember|lost|recover(?:ing)?)\b.*\bpasswords?\b|\bpasswords?\b.*\b(?:reset(?:ting)?|forgot|can(?:'|no)t\s*remember|lost|recover(?:ing)?)\b",
                    RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled 
                ),
                "ResetPasswordTask"
            ),

            // CheckOrderStatusTask: all phrases with "order/s" + "status", "track", "check", "follow", "where is" or vice versa
            (
                new Regex(
                    // order status  OR  X ... order  OR  order ... X
                    @"\border\s*status\b|\b(track|check|follow|where\s+is)\b.*\border(s)?\b|\border(s)?\b.*\b(status|track|check|follow)\b",
                    RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled
                ),
                "CheckOrderStatusTask"
            )
        };

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

        // function to match the utterance to a task based on predefined patterns
        private static string MatchTask(string? utterance)
        {
            var text = (utterance ?? string.Empty);

            foreach (var (pattern, task) in _patterns)
            {
                if (pattern.IsMatch(text))
                    return task;
            }
            return "NoTaskFound";
        }
    }
}
