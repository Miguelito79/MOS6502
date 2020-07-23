using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MOS6502_BL.MMU;

namespace MOS6502_BL.MOS6502
{
    public class ALU
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
        /// Perform addition of acumulator and the value in input
        /// </summary>
        /// <param name="accumulator">Accumulator</param>
        /// <param name="value">Value to be added to the accumulator</param>
        /// <param name="carry">Carry flag status</param>
        /// <returns></returns>
        private byte PerformDecimalAddition(byte accumulator, byte value, byte carry)
        {
            ushort lnibble = (ushort)((accumulator & 0xF) + (value & 0xF) + carry);
            byte halfCarry = (byte)((lnibble > 0x09) ? 0x10 : 0);

            ushort hnibble = (ushort)((accumulator & 0xF0) + (value & 0xF0) + halfCarry);
            byte binaryResult = (byte)((lnibble & 0x0F) + (hnibble & 0xF0));
            
            CPU.Flags.ComputeNFlag(binaryResult);
            CPU.Flags.ComputeZFlag(binaryResult);
            CPU.Flags.ComputeVFlag(accumulator, value, binaryResult);
            CPU.Flags.ComputeCFlagFromADC(hnibble);

            //Executing Decimal Adjust if necessary
            if (halfCarry != 0)
                lnibble += 0x06;

            if (hnibble >= 0x9F)
                hnibble += 0x60;
            /*************************************/

            return (byte)((lnibble & 0xF) + (hnibble & 0xF0));
        }        

        /// <summary>
        /// Perform subtraction of input value from accumulator 
        /// </summary>
        /// <param name="accumulator">accumulator</param>
        /// <param name="value">value to be subtracted fromaccumulato</param>
        /// <param name="carry">carry flag</param>
        /// <returns></returns>
        private byte PerormDecimalSubtraction(byte accumulator, byte value, byte carry)
        {
            byte response = 0x00;

            ushort lnibble = (ushort)(0xF + (accumulator & 0xF) - (value & 0xF) + carry);
            byte halfCarry = (byte)((lnibble > 0xF) ? 0x10 : 0);

            ushort hnibble = (ushort)(0xF0 + (accumulator & 0xF0) - (value & 0xF0) + halfCarry);
            byte binaryResult = (byte)((lnibble & 0x0F) + (hnibble & 0xF0));

            CPU.Flags.ComputeNFlag(binaryResult);
            CPU.Flags.ComputeZFlag(binaryResult);
            CPU.Flags.ComputeVFlag(accumulator, (byte)~value, binaryResult);
            CPU.Flags.ComputeCFlagFromSBC(hnibble);

            //Executing Decimal Adjust if necessary
            if (halfCarry == 0)
                lnibble -= 0x6;

            if (hnibble < 0xFF)
                hnibble -= 0x60;
            /*************************************/

            response = (byte)((lnibble & 0xF) + (hnibble & 0xF0));
            return response;
        }

        /// <summary>
        /// Perform addition of accumulato, carry flag and input operand
        /// </summary>
        /// <param name="operand">Operand to be addedd to the accumulator</param>
        public void PerformADC(byte operand)
        {
            if (CPU.Flags.DFlag == 0)
            {
                ushort result = (ushort)(CPU.A + operand + CPU.Flags.CFlag);

                CPU.Flags.ComputeNFlag(result);
                CPU.Flags.ComputeZFlag(result);
                CPU.Flags.ComputeVFlag(CPU.A, operand, result);

                CPU.A = (byte)result;                
                CPU.Flags.ComputeCFlagFromADC(result);
            }
            else
            {
                //We are computing in BCD so perform decimal adjust is needed
                CPU.A = PerformDecimalAddition(CPU.A, (byte)(operand), (byte)CPU.Flags.CFlag);
            }
                

            
        }

        /// <summary>
        /// Perform subtract from accumulator including the content of carry flag
        /// </summary>
        /// <param name="operand">Operand to be subtracted from accumulator</param>
        public void PerformSBC(byte operand)
        {
            if (CPU.Flags.DFlag == 0)
            {
                ushort result = (ushort)(0xFF + CPU.A - operand + CPU.Flags.CFlag);

                CPU.Flags.ComputeNFlag(result);
                CPU.Flags.ComputeZFlag(result);
                CPU.Flags.ComputeVFlag(CPU.A, (byte)~operand, result);

                CPU.A = (byte)result;
                CPU.Flags.ComputeCFlagFromSBC(result);
            }
            else
            {
                //We are computing in BCD so perform decimal adjust is needed
                CPU.A = PerormDecimalSubtraction(CPU.A, operand, (byte)CPU.Flags.CFlag);
            }           
        }

        /// <summary>
        /// Decrement input register by 1
        /// </summary>
        /// <param name="register">cpu register</param>
        public byte PerformDescrementRegister(byte register)
        {
            register -= 1;

            CPU.Flags.ComputeNFlag(register);
            CPU.Flags.ComputeZFlag(register);

            return register;
        }

        /// <summary>
        /// Increment input register by 1
        /// </summary>
        /// <param name="register"></param>
        /// <returns></returns>
        public byte PerformIncrementRegister(byte register)
        {
            register += 1;

            CPU.Flags.ComputeNFlag(register);
            CPU.Flags.ComputeZFlag(register);

            return register;
        }        

        /// <summary>
        /// Perform AND bit a bit of the two input operands
        /// </summary>
        /// <param name="operand1">First operand</param>
        /// <param name="operand2">Second Operand</param>
        public void PerformAND(byte operand1, byte operand2)
        {
            ushort result = (ushort)(operand1 & operand2);

            CPU.Flags.ComputeNFlag(result);
            CPU.Flags.ComputeZFlag(result);

            CPU.A = (byte)result;
        }

        /// <summary>
        /// Perform Exclusive OR bit bit of the two input operands
        /// </summary>
        /// <param name="operand1"></param>
        /// <param name="operand2"></param>
        public void PerformEOR(byte operand1, byte operand2)
        {
            ushort result = (ushort)(operand1 ^ operand2);

            CPU.Flags.ComputeNFlag(result);
            CPU.Flags.ComputeZFlag(result);

            CPU.A = (byte)result;
        }

        /// <summary>
        /// Perform OR bit bit of the two input operands
        /// </summary>
        /// <param name="operand1"></param>
        /// <param name="operand2"></param>
        public void PerformOR(byte operand1, byte operand2)
        {
            ushort result = (ushort)(operand1 | operand2);

            CPU.Flags.ComputeNFlag(result);
            CPU.Flags.ComputeZFlag(result);

            CPU.A = (byte)result;
        }

        /// <summary>
        /// Do compare performing subtraction from register and input value
        /// and set accordingly, flag C, Z, N
        /// </summary>
        /// <param name="register"></param>
        /// <param name="value">value to be subtracted from register</param>
        public void Compare(byte register, byte value)
        {
            byte result = (byte)(register - value);

            CPU.Flags.CFlag = register >= value ? 1 : 0;
            CPU.Flags.ComputeNFlag(result);
            CPU.Flags.ComputeZFlag(result);
        }

        /// <summary>
        /// Perform logical AND between register and data setting ONLY the appropriate flags
        /// </summary>
        /// <param name="register">input register</param>
        /// <param name="data">input data</param>
        public void PerformBIT(byte register, byte data)
        {            
            CPU.Flags.NFlag = (data & 0x80) != 0 ? 1 : 0;
            CPU.Flags.VFlag = (data & 0x40) != 0 ? 1 : 0;
            CPU.Flags.ZFlag = (data & register) == 0 ? 1 : 0;
        }          
    }
}
