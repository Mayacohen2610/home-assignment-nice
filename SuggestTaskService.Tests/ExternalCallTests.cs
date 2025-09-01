using Xunit;
using SuggestTaskService.Controllers;

namespace SuggestTaskService.Tests
{
    public class ExternalCallTests
    {
        [Fact]
        public void SimulatedExternalCall_ShouldAlwaysFail_WhenProbabilityIs100Percent()
        {
            // 100% failure → always false
            var result = InvokeSimulatedExternalCall(1.0);
            Assert.False(result);
        }

        [Fact]
        public void SimulatedExternalCall_ShouldAlwaysSucceed_WhenProbabilityIs0Percent()
        {
            // 0% failure → always true
            var result = InvokeSimulatedExternalCall(0.0);
            Assert.True(result);
        }

        // Helper method to access the private static method via reflection
        private bool InvokeSimulatedExternalCall(double probability)
        {
            var method = typeof(SuggestTaskController)
                .GetMethod("SimulatedExternalCall",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

            Assert.NotNull(method); // make sure the method was found

            return (bool)method!.Invoke(null, new object[] { probability })!;
        }
    }
}
