﻿using System;
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
                calc.UnexpectedExceptionHandler.Subscribe( Console.WriteLine );
                calc.OnProcessEnded.Subscribe( it => Console.WriteLine( "Proccess ended" ) );
                calc.Instance.ActionEvent += InstanceOnActionEvent;
                calc.Instance.Event += InstanceOnEvent;
                calc.Instance.ActionCalcArg += InstanceOnActionCalcArg;

                while ( true )
                {
                    CallInstance( calc );

                    if ( Console.ReadKey().Key == ConsoleKey.Q )
                        break;
                }

                calc.Instance.ActionEvent -= InstanceOnActionEvent;
                calc.Instance.Event -= InstanceOnEvent;
                calc.Instance.ActionCalcArg -= InstanceOnActionCalcArg;
                CallInstance( calc );

               // Console.ReadKey();
            }
            Console.ReadKey();
        }

        private static void InstanceOnActionCalcArg( object sender, CalcArg e )
        {
            Console.WriteLine( $"{sender} , {e.Result}" );
        }

        private static void InstanceOnEvent( object sender, EventArgs e )
        {
            Console.WriteLine( $"{sender} {e}" );
        }

        private static void CallInstance( Sandbox< ICalculator, Calculator > calc )
        {
            for ( var i = 0; i < 100; i++ )
            {
                Console.WriteLine( $"Add {calc.Instance.Add( i, 2 )}" );
                Console.WriteLine( "Last result " + calc.Instance.LastResult );
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
            event EventHandler< CalcArg > ActionCalcArg;
        }

        public class Calculator : ICalculator
        {
            public int Add( int a, int b )
            {
                var sum = a + b;
                OnAction();
                OnEvent();
                ActionCalcArg?.Invoke( null, new CalcArg { Result = sum } );
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
            public event EventHandler< CalcArg > ActionCalcArg;
        }

        [Serializable]
        public struct CalcArg
        {
            public int Result { set; get; }
        }
    }
}