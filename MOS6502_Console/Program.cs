using MOS6502_BL;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MOS6502_Console
{
    class Program
    {
        static void Main(string[] args)
        {
            string baseTestLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string testLocation = Path.Combine(baseTestLocation, @"Files\6502_functional_test.bin");

            Emulator emulator = Emulator.Instance;
            emulator.CPU.PC = 0x400;

            emulator.Run(testLocation);
            Console.ReadKey();
        }
    }
}
