using Sandbox.InvocationHandlers;
using SandboxTest.Instances;
using Xunit;

namespace SandboxTest.InvocationHandler
{
    public sealed class DelegateFactoryTest
    {
        private readonly TestClass instance;

        public DelegateFactoryTest()
        {
            instance = new TestClass();
        }

        [Theory]
        [InlineData( nameof( TestClass.EventAction ) )]
        [InlineData( nameof( TestClass.EventWithHandler ) )]
        [InlineData( nameof( TestClass.EventWithStructArg ) )]
        public void TestEvents( string eventName )
        {
            var ev = instance.GetType().GetEvent( eventName );
            var del = DelegateFactory.Create( ev, Handler );
            ev.AddEventHandler( instance, del );
        }

        private static void Handler( string name, params object[] args )
        {
        }
    }
}