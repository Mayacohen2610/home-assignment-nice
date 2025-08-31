using Xunit;

namespace SuggestTaskService.Tests
{
    public class MatchTaskTests
    {
        // ResetPasswordTask
        [Theory]
        [InlineData("reset password", "ResetPasswordTask")]
        [InlineData("please RESET my PASSWORD", "ResetPasswordTask")]                 // case-insensitive
        [InlineData("user is resetting password now", "ResetPasswordTask")]           // resetting
        [InlineData("I forgot my password yesterday", "ResetPasswordTask")]           // forgot
        [InlineData("can't remember password", "ResetPasswordTask")]                  // can't remember
        [InlineData("cannot   remember   password", "ResetPasswordTask")]             // extra spaces
        [InlineData("recover password ASAP!", "ResetPasswordTask")]                   // recover + punctuation
        [InlineData("password reset please", "ResetPasswordTask")]                    // reversed order
        [InlineData("passwords resetting for multiple users", "ResetPasswordTask")]   // plural "passwords"

        // CheckOrderStatusTask
        [InlineData("track order 12345", "CheckOrderStatusTask")]
        [InlineData("can you check my order?", "CheckOrderStatusTask")]
        [InlineData("what is my order status", "CheckOrderStatusTask")]
        [InlineData("where is my order now", "CheckOrderStatusTask")]
        [InlineData("order status please", "CheckOrderStatusTask")]
        [InlineData("please CHECK the ORDER", "CheckOrderStatusTask")]                // case-insensitive

        // Both tasks mentioned - should match ResetPasswordTask due to priority 
        [InlineData("forgot password and also track order", "ResetPasswordTask")]
        [InlineData("track order but I also forgot password", "ResetPasswordTask")]   // Reset has priority

        // NoTaskFound
        [InlineData("hello world", "NoTaskFound")]
        [InlineData("need help with my account", "NoTaskFound")]
        [InlineData("change pass", "NoTaskFound")]                                    // "pass" != "password"
        [InlineData("remember password", "NoTaskFound")]                               // missing can't/cannot
        [InlineData("reset the device", "NoTaskFound")]                                // no "password"
        [InlineData("order a pizza", "NoTaskFound")]                                   // "order" not in the right sense
        [InlineData("tracking number only", "NoTaskFound")]                            // no "order"

        public void MatchTask_ReturnsExpectedTask(string utterance, string expected)
        {
            // act
            var actual = SuggestTaskService.Controllers.SuggestTaskController.MatchTask(utterance);

            // assert
            Assert.Equal(expected, actual);
        }
    }
}
