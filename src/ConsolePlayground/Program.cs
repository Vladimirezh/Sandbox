using System;
using Sandbox.Server;

namespace ConsolePlayground
{
    public class Program
    {
        private static void Main()
        {
            Console.WriteLine( "Running" );

            using ( var calc = new SandboxBuilder().WithClient( Platform.x86 ).Build< ICalculator, Calculator >() )
            {
                Console.WriteLine( "Connected" );
                calc.UnexpectedExceptionHandler.Subscribe( Console.WriteLine );
                calc.Instance.ActionEvent += InstanceOnActionEvent;

                while ( true )
                {
                    CallInstance( calc );

                    if ( Console.ReadKey().Key == ConsoleKey.Q )
                        break;
                }

                calc.Instance.ActionEvent -= InstanceOnActionEvent;

                CallInstance( calc );

                Console.ReadKey();
            }
        }

        private static void CallInstance( Sandbox< ICalculator, Calculator > calc )
        {
            for ( var i = 0; i < 100; i++ )
            {
                Console.WriteLine( $"Add {calc.Instance.Add( i, 2 )}" );
                Console.WriteLine( "Last i " + calc.Instance.LastResult );
            }
        }

        private static void InstanceOnActionEvent()
        {
            Console.WriteLine( "InstanceOnActionEvent" );
        }

        public interface ICalculator
        {
            int Add( int a, int b );
            string LastResult { get; }
            event Action ActionEvent;
            event EventHandler Event;
        }

        public class Calculator : ICalculator
        {
            public int Add( int a, int b )
            {
                var sum = a + b;
                OnAction();
                OnEvent();
                LastResult = sum.ToString();
                return sum;
            }

            public string LastResult { get; private set; }

            public void OnEvent()
            {
                Event?.Invoke( nameof( OnEvent ), new EventArgs() );
            }

            public void OnAction()
            {
                ActionEvent?.Invoke();
            }

            public event Action ActionEvent;
            public event EventHandler Event;
        }
    }
}