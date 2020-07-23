using MOS6502_BL.MMU;
using MOS6502_BL.MOS6502;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MOS6502_BL
{    
    public class Emulator
    {
        private static readonly Emulator _instance;
        public static Emulator Instance
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
       
        static Emulator()
        {
            _instance = new Emulator();

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
    }
}
