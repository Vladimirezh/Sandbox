using System.Threading.Tasks;

namespace SandboxTest.Common
{
    public static class Extentions
    {
        public static Task CompletedTask { get; } = Task.FromResult( false );
    }
}