namespace SuggestTaskService.Models
{
    // DTO for incoming JSON
    public class SuggestTaskRequest
    {
        public string? utterance { get; set; }
        public string? userId { get; set; }
        public string? sessionId { get; set; }
        public string? timestamp { get; set; } 
    }
}
