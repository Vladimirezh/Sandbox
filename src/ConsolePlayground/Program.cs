using System;
using Sandbox.Server;

namespace ConsolePlayground
{
    internal class Program
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
                    for ( var i = 0; i < 100; i++ )
                    {
                        Console.WriteLine( $"Add {calc.Instance.Add( i, 2 )}" );
                        Console.WriteLine( "Last i " + calc.Instance.LastI );
                    }

                    Console.ReadKey();
                }
            }
        }

        private static void InstanceOnActionEvent()
        {
            Console.WriteLine( "InstanceOnActionEvent" );
        }

        public interface ICalculator
        {
            int Add( int a, int b );
            string LastI { get; }
            event Action ActionEvent;
            event EventHandler EHEvent;
        }

        public class Calculator : ICalculator
        {
            public int Add( int a, int b )
            {
                LastI = a.ToString();
                return a + b;
            }

            public string LastI { get; private set; }

            public event Action ActionEvent;
            public event EventHandler EHEvent;
        }
    }
}