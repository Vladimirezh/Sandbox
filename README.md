[![Build status](https://ci.appveyor.com/api/projects/status/q35q6un8lv9rfoxm?svg=true)](https://ci.appveyor.com/project/Vladimirezh/sandbox)[![NuGet version](https://badge.fury.io/nu/Sandbox.svg)](https://badge.fury.io/nu/Sandbox)
# Sandbox

Sandbox provides interface for out-of-process code execution. Increase the reliability of your application by leveraging multi-process architecture. You can run the code in a process of the correct bitness.So if you have legacy 32 bit DLLs you want to call from a 64 bit process then you want Sandbox. When you run your task Sandbox will automatically put your code into a separate process. Normal everyday exceptions will cross the process boundary and can be handled by your calling code.
Sandbox based on named pipes, binary serialization and observers.

# How to use

For example, you have Calculator class and ICalculator interface

```
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
```
Now, run it in Sandbox and call methods
```
var calcSandbox = new SandboxBuilder( Platform.x86 ).Build< ICalculator, Calculator >();
calcSandbox.Instance.ActionEvent += () => Console.WriteLine( "ActionEvent" );
calcSandbox.Instance.Event += ( sender, args ) => Console.WriteLine( $"{sender} {args}" );
calcSandbox.Instance.Add(10,20);
```
Easy!
