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
                while ( true )
                {
                    for ( var i = 0; i < 100; i++ )
                        Console.WriteLine( calc.Instance.Add( i, 2 ) );
                    Console.ReadKey();
                }
            }
        }

        public interface ICalculator
        {
            int Add( int a, int b );
        }

        public class Calculator : ICalculator
        {
            public int Add( int a, int b )
            {
                return a + b;
            }
        }
    }
}