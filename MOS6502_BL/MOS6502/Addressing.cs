using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

using MOS6502_BL.MMU;

namespace MOS6502_BL.MOS6502
{    
    public class Addressing
    {
        private MCU _mcu = Emulator.Instance.MCU;
              
        /// <summary>
        /// (abs) - Absolute addressing: Fetch value from memory, 
        /// whose address id based on 2nd and 3th byte of current instruction
        /// </summary>
        /// <param name="address">address to be read</param>
        /// <returns>The value fethed from memory address</returns>
        public byte ReadAbsoluteValue(ushort address)
        {
            return _mcu.ReadByte(address);
        }

        /// <summary>
        /// (abs) - Store data in memory by absolute address
        /// </summary>
        /// <param name="address">address to be writed</param>
        /// <param name="value">data to be stored</param>
        public void WriteAbsoluteValue(ushort address, byte value)
        {
            _mcu.WriteByte(address, value);
        }

        /// <summary>
        /// (zpg) - Fetch value from Zero Page memory area
        /// </summary>
        /// /// <param name="address">address to be read</param>
        /// <returns>Byte fetched from zero page</returns>
        public byte ReadZeroPage(ushort address)
        {            
            return _mcu.ReadByte((byte)(address & 0xFF));
        }

        /// <summary>
        /// (zpg) - Store valie in Zero Page memory area
        /// </summary>
        /// <param name="address">address where data must be stored</param>
        /// <param name="value">data to be stored</param>
        public void WriteZeroPage(ushort address, byte value)
        {
            //In Zero Page, address must be from 0001 to 00FF
            _mcu.WriteByte((ushort)(address & 0xFF), value);
        }

        /// <summary>
        /// abs,x OR abs,y - Fetch value from memory whose effective address is the sum of next
        /// word of current instruction, and the value of register X or Y
        /// </summary>
        /// <param name="register"></param>
        /// <returns></returns>
        public byte ReadAbsoluteIndexedAddressing(ushort address, byte register)
        {            
            return _mcu.ReadByte((ushort)(address + register));
        }

        /// <summary>
        /// abs,x OR abs,y - Write in memory using absolute indirect addressing        
        /// </summary>
        /// <param name="address">base address</param>
        /// <param name="register">regiser offset</param>
        /// <param name="value">value to be stored in (address + register) memory location</param>
        public void WriteAbsoluteIndexedAddressing(ushort address, byte register, byte value)
        {
            _mcu.WriteByte((ushort)(address + register), value);
        }

        /// <summary>
        /// zgp,x OR zpg,y - Read from Zero Page memory area using indexed addressing
        /// </summary>
        /// <param name="address">base address</param>
        /// <param name="register">register offset</param>
        /// <returns>value to be read from (address + register) memory location</returns>
        public byte ReadZPageByIndexedAddressing(ushort address, byte register)
        {
            //In Zero Page adressing only lower bytes of address will be taken in consideration                        
            return _mcu.ReadByte((ushort)((address + register) & 0xFF));
        }


        /// <summary>
        /// zpg,x OR zpg,y - Write in Zero Page memory area using indexed addressing
        /// </summary>
        /// <param name="address">Base address</param>
        /// <param name="register">Register offset</param>
        /// <param name="value">value to be stored in (address + register) memory location/param>
        public void WriteZpageByIndexedAddressing(ushort address, byte register, byte value)
        {
            //In Zero Page adressing only lower bytes of address will be taken in consideration
            _mcu.WriteByte((ushort)((address + register) & 0xFF), value);
        }

        /// <summary>
        /// (zpg,x) - Read from Zero Page memory area using indexed indirect address 
        /// <param name="address">Base address</param>
        /// <param name="register">Register offset</param>
        /// </summary>        
        /// <returns>value to be read from Zero Page (address + register) memory location</returns>
        public byte ReadZPageByIndexedIndirectAddressing(ushort address, byte register)
        {           
            byte lowOrderAddress = _mcu.ReadByte((ushort)((address + register) & 0xFF));
            byte highOrderAddress = _mcu.ReadByte((ushort)((address + register + 1) &0xFF));

            ushort effectiveAddress = (ushort)(lowOrderAddress | (highOrderAddress << 8));            
            return _mcu.ReadByte(effectiveAddress);
        }

        /// <summary>
        /// (zpg,x) - Write in Zero Page memory area using indirect indexed address         
        /// </summary>
        /// <param name="address">Base address</param>
        /// <param name="register">Register offset</param>
        /// <param name="value">Value to be stored in Zero Page (address + register) memory location</param>
        public void WriteZPageByIndexedIndirectAddressing(ushort address, byte register, byte value)
        {
            byte lowOrderAddress = _mcu.ReadByte((ushort)((address + register) & 0xFF));
            byte highOrderAddress = _mcu.ReadByte((ushort)((address + register + 1) & 0xFF));

            ushort effectiveAddress = (ushort)(lowOrderAddress | (highOrderAddress << 8));
            _mcu.WriteByte(effectiveAddress, value);
        }

        /// <summary>
        /// (zpg),y OR (zpg),x - Read from Zero Page memoy area using indirect indexed addressing
        /// </summary>
        /// <param name="address">base address</param>
        /// <param name="register">register</param>
        /// <returns>Vaue read from memory area address</returns>
        public byte ReadZPageByIndirectIndexedAddressing(ushort address, byte register)
        {
            byte lowOrderBaseAddress = _mcu.ReadByte(address);
            byte highOrderBaseAddress = _mcu.ReadByte((ushort)(address + 1));
            
            ushort baseAddress = (ushort)(lowOrderBaseAddress | highOrderBaseAddress << 8);
            ushort effectiveAddress = (ushort)(baseAddress + register);                     

            return _mcu.ReadByte(effectiveAddress);           
        }

        /// <summary>
        /// (zpg),y OR (zpg),x - Write value in Zero Page using indirect indexed addressing
        /// </summary>
        /// <param name="address">base address</param>
        /// <param name="register">register</param>
        /// <param name="value">value to be stored in ZPage</param>
        public void WriteZPageByIndirectIndexedAddressing(ushort address, byte register, byte value)
        {
            byte lowOrderBaseAddress = _mcu.ReadByte(address);
            byte highOrderBaseAddress = _mcu.ReadByte((ushort)(address + 1));

            ushort baseAddress = (ushort)(lowOrderBaseAddress | highOrderBaseAddress << 8);
            ushort effectiveAddress = (ushort)(baseAddress + register);

            _mcu.WriteByte(effectiveAddress, value);
        }

        /// <summary>
        /// The function detect if there is a carry from 7th to 8th bit when adding
        /// the content of any register to an address in memory.
        /// </summary>
        /// <param name="address">Memory address</param>
        /// <param name="register">Register value</param>
        /// <returns></returns>
        public bool DetectCrossPageBoundary(ushort address1, ushort address2)
        {
            return (address1 & 0xFF00) != (address2 & 0xFF00) ? true : false;
        }        
    }
}
