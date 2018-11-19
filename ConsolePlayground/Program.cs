using System;
using System.Reactive.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsolePlayground
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Running");
            using (var calc = new Sandbox.Server.SandboxBuilder().WithClient(Sandbox.Server.Platform.x86)
                .Build<ICalculator, Calculator>())
            {
                Console.WriteLine("Connected");
                calc.UnexpectedExceptionHandler.Subscribe(it => { Console.WriteLine(it); });
                for (int i = 0; i < 100; i++)
                    Console.WriteLine(calc.Instance.Add(i, 2));
                Console.ReadKey();
            }
        }

        public interface ICalculator
        {
            int Add(int a, int b);
        }

        public class Calculator : ICalculator
        {
            public int Add(int a, int b)
            {
                return a + b;
            }
        }
    }
}
