using System;
using System.IO;

namespace TestConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            File.WriteAllText($"F:\\{DateTime.Now.Ticks}.txt", DateTime.Now.Ticks.ToString());
        }
    }
}
