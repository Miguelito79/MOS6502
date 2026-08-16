using Emulator;
using System.Reflection;

namespace FunctionalTest
{
    internal class Program
    {
        static void Main(string[] args)
        {         
            string baseTestLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string testLocation = Path.Combine(baseTestLocation, @"Files\6502_functional_test.bin");

            MOS6502 emulator = MOS6502.Instance;
            emulator.CPU.PC = 0x400;

            emulator.Run(testLocation);
            Console.ReadKey();        
        }
    }
}
