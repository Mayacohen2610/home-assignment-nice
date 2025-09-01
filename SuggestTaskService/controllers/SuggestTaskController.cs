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

            // log message to confirm validation passed
            Console.WriteLine("[INFO] Validation passed");

            // Match task based on utterance
            // req.utterance is non-null due to validation- but to satisfy the compiler:
            if (req.utterance == null) 
            {
                // this should never happen due to validation
                Console.WriteLine("[ERROR] utterance is null after validation- this should never happen");
                return BadRequest(new { error = "utterance is null after validation- this should never happen" });
            }
            var task = MatchTask(req.utterance!); 
            Console.WriteLine($"[INFO] Selected task='{task}' for userId={req.userId}");

            // Simulate external call with retries
            var success = TryExternalWithRetry(maxAttempts: 3); // stop on first success
            if (!success)
            {
                // After 3 consecutive failures, stop and return an error response 503 (Service Unavailable)
                Console.WriteLine($"[ERROR] External dependency failed after 3 consecutive attempts for userId={req.userId}");
                return StatusCode(503, new { error = "External dependency failed after 3 consecutive attempts" });
            }

            // Response with ISO-8601 timestamp
            Console.WriteLine($"[INFO] External dependency succeeded. Returning OK responsefor userId={req.userId}");
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

        // function to match the utterance to a task based on predefined patterns
        public static string MatchTask(string? utterance)
        {
            var text = (utterance ?? string.Empty);

            foreach (var (pattern, task) in _patterns)
            {
                if (pattern.IsMatch(text))
                    return task;
            }
            return "NoTaskFound";
        }

        // Simulates an external call that may randomly fail (bonus) .
        // Returns true on success, false on failure- defalt 50% failure rate
        private static bool SimulatedExternalCall(double failureProbability = 0.50)
        {
            var rnd = Random.Shared;
            bool ok = rnd.NextDouble() >= failureProbability;

            // simulate some latency- for realism
            System.Threading.Thread.Sleep(20);

            return ok;
        }

        // 3 attempts to call SimulatedExternalCall, returns true if any attempt succeeds and false if all fails
        private static bool TryExternalWithRetry(int maxAttempts = 3)
        {
            for (int attempt = 1; attempt <= maxAttempts; attempt++)
            {
                if (SimulatedExternalCall()){
                    // log success
                    Console.WriteLine($"[INFO] External call succeeded on attempt {attempt}");
                    return true; // success on this attempt
                }
                // simulate some latency before retrying- for realism
                System.Threading.Thread.Sleep(50);
            }
            return false; // failed maxAttempts times in a row
        }


    }
}
