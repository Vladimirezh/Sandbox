[![Build status](https://ci.appveyor.com/api/projects/status/q35q6un8lv9rfoxm?svg=true)](https://ci.appveyor.com/project/Vladimirezh/sandbox)
# Sandbox

Sandbox provides interface for out-of-process code execution. Increase the reliability of your application by leveraging multi-process architecture. You can run the code in a process of the correct bitness.So if you have legacy 32 bit DLLs you want to call from a 64 bit process then you want Sandbox. When you run your task Sandbox will automatically put your code into a separate process. Normal everyday exceptions will cross the process boundary and can be handled by your calling code.
Sandbox based on named pipes, binary serialization and observers.

#How to use

For example you have Calculator class and ICalculator interface

```
        public class Calculator : ICalculator
        {
            public int Add(int a, int b)
            {
                return a + b;
            }
        }
        
        public interface ICalculator
        {
           int Add(int a, int b);
        }
```
Now, run it in Sandbox and call methods
```
var calcSandbox = new SandboxBuilder().WithClient(Platform.x86).Build<ICalculator, Calculator>();
calcSandbox.Instance.Add(10,20);
```
Easy!
