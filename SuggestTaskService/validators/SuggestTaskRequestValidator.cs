using FluentValidation;
using SuggestTaskService.Models;

namespace SuggestTaskService.Validators
{
    public class SuggestTaskRequestValidator : AbstractValidator<SuggestTaskRequest>
    {
        public SuggestTaskRequestValidator()
        {
            RuleFor(x => x.utterance)
                .NotEmpty().WithMessage("utterance is required");

            RuleFor(x => x.userId)
                .NotEmpty().WithMessage("userId is required");

            RuleFor(x => x.sessionId)
                .NotEmpty().WithMessage("sessionId is required");

            RuleFor(x => x.timestamp)
                .NotEmpty().WithMessage("timestamp is required")
                .Must(BeValidIso8601)
                .WithMessage("timestamp must be ISO-8601 (e.g., 2025-08-21T12:00:00Z)");
        }

        // function to validate ISO-8601 format
        private static bool BeValidIso8601(string? ts)
        {
            if (string.IsNullOrWhiteSpace(ts))
                return false;

            return DateTime.TryParse(
                ts,
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.RoundtripKind,
                out _);
        }
    }
}
