using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MOS6502_BL.MMU;

namespace MOS6502_BL.MOS6502
{
    public class Stack
    {         
        public CPU CPU
        {
            get { return Emulator.Instance.CPU; }
        }

        public MCU MCU
        {
            get { return Emulator.Instance.MCU; }
        }

        /// <summary>
        /// Push 16bit data into the stack pointer (two bytes)
        /// </summary>
        /// <param name="data">16bit data to be pushed</param>
        public void PushWord(ushort data)
        {
            byte highOrder = (byte)(data >> 8);
            byte lowOrder = (byte)(data & 0xFF);

            MCU.WriteByte((ushort)(CPU.SP | 0x0100), highOrder);
            CPU.SP -= 1;

            MCU.WriteByte((ushort)(CPU.SP | 0x0100), lowOrder);
            CPU.SP -= 1;
        }

        /// <summary>
        /// Pop 16bit data from the stack
        /// </summary>
        /// <returns>unsigned short of 16bit data (two bytes)</returns>
        public ushort PopWord()
        {
            CPU.SP += 1;
            byte lowOrder = MCU.ReadByte((ushort)(CPU.SP | 0x0100));

            CPU.SP += 1;
            byte highOrder = MCU.ReadByte((ushort)(CPU.SP | 0x0100));

            return (ushort)((highOrder << 8) | lowOrder);
        }

        /// <summary>
        /// Push 8bit data into the stack pointer
        /// </summary>
        /// <param name="data">8bit data to be pushed</param>
        public void Push(byte data)
        {
            MCU.WriteByte((ushort)(CPU.SP | 0x0100), data);
            CPU.SP -= 1;
        }

        /// <summary>
        /// Retrieve a byte (8 bit) from the stack pointer
        /// </summary>
        /// <returns>byte popped from stack pointer</returns>
        public byte Pop()
        {
            CPU.SP += 1;
            return MCU.ReadByte((ushort)(CPU.SP | 0x0100));
        }
    }
}
