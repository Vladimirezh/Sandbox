using Sandbox.Commands;
using Xunit;

namespace SandboxTest.Commands
{
    public class CommandTest
    {
        [Fact]
        public void AllCallsCtorMustIncrementNumber()
        {
            var num = new Message().Number;
            for (var i = 1; i < 100; i++)
                Assert.Equal(num + i, new Message().Number);
        }
    }
}