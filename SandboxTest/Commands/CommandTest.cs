using Sandbox.Commands;
using Xunit;

namespace SandboxTest.Commands
{
    public class CommandTest
    {
        [Fact]
        public void AllCallsCtorMustIncrementNumber()
        {
            for (var i = 0; i < 100; i++)
                Assert.Equal(i + 1, new Message().Number);
        }
    }
}