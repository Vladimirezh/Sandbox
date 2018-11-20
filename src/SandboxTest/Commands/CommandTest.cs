using Sandbox.Commands;
using Xunit;

namespace SandboxTest.Commands
{
    public class CommandTest
    {
        [Fact]
        public void AllCallsCtorMustIncrementNumber()
        {
            var prevNum = new Message().Number;
            for (var i = 1; i < 10; i++)
            {
                var newNum = new Message().Number;
                Assert.True(prevNum < newNum);
                prevNum = newNum;
            }
        }
    }
}