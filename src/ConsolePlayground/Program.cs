using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
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
               // calc.Instance.EHEvent += ( s, e ) => Console.WriteLine( e );
                calc.Instance.ActionEvent += InstanceOnActionEvent;
                //Console.ReadKey();
                while ( true )
                {
                    CallInstance( calc );

                    if ( Console.ReadKey().Key == ConsoleKey.Escape )
                        break;
                }

             //   calc.Instance.EHEvent -= ( s, e ) => Console.WriteLine( e );
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
                Console.WriteLine( "Last i " + calc.Instance.LastI );
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
                OnAction();
                OnEHEvent();
                return a + b;
            }

            public string LastI { get; private set; }

            public void OnEHEvent()
            {
                EHEvent?.Invoke( nameof( OnEHEvent ), new EventArgs() );
            }

            public void OnAction()
            {
                ActionEvent?.Invoke();
            }

            public event Action ActionEvent;
            public event EventHandler EHEvent;
        }
    }
}