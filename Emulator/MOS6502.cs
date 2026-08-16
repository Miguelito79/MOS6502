using Emulator.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace Emulator
{
    public class MOS6502
    {
        private static readonly MOS6502 _instance;
        public static MOS6502 Instance
        {
            get { return _instance; }
        }

        public CPU CPU
        {
            get;
            private set;
        }

        public MCU MCU
        {
            get;
            private set;
        }

        static MOS6502()
        {
            _instance = new MOS6502();

            _instance.MCU = new MCU();
            _instance.CPU = new CPU();
        }

        public void Run(string fullPath)
        {
            if (MCU.LoadFileInMemoryAt(fullPath, 0))
            {
                Thread thread = new Thread(RunAsync_WorkingThread);
                thread.Start();
            }
        }

        private void RunAsync_WorkingThread()
        {
            int tCycles = 0;

            while (true)
            {
                tCycles += CPU.FetchAndExecute();

                /*Passed Klauss Dorman Functional Test*/
                if (CPU.PC == 0x3469)
                {
                    Console.WriteLine("Success");
                    break;
                }
                /*************************************/
            }
        }

        public void Run_A_Cycle()
        {
            int tCycles = 0;
            tCycles = CPU.FetchAndExecute();
        }
    }
}
