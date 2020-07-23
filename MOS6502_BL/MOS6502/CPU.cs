using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Sockets;
using System.Reflection.Emit;
using System.Security.AccessControl;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;

using MOS6502_BL.MMU;

namespace MOS6502_BL.MOS6502
{
    public class CPU
    {        
        ushort previousPC;

        private MCU _mcu;
        private ALU _alu;

        private Stack _stack;
        private Addressing _addressing;

        //Program counter        
        public ushort PC
        {
            get; set;
        }

        private Flags _flags;
        public Flags Flags
        {
            get { return _flags; }
            private set { }
        }

        //Stack Pointer: can only range from 0100 to 01FF (In Zero page memory area)
        public byte SP
        {
            get; set;
        }

        //Accumulator
        public byte A
        {
            get;
            set;
        }

        //Y register
        public byte Y
        {
            get;
            private set;
        }

        //X register
        public byte X
        {
            get;
            private set;
        }

        private byte _opcode; //Current opcode                
        private Instructions _instructions;
        public Instructions Instructions
        {
            get { return _instructions; }
            private set { } 
        }

        public CPU()
        {
            X = 0;
            Y = 0;
            A = 0;

            _mcu = Emulator.Instance.MCU;
            _alu = new ALU();
            _flags = new Flags(0x20);

            _addressing = new Addressing();
            _stack = new Stack();
            
            _instructions = Instructions.Get();
            _instructions.SetAction(this);
        }

        #region Memory read functions
        private byte NextByte()
        {
            return _mcu.ReadByte((ushort)(this.PC + 1));
        }

        private ushort NextWord()
        {
            return (ushort)(ushort)(_mcu.ReadByte((ushort)(this.PC + 1)) | (_mcu.ReadByte((ushort)(this.PC + 2)) << 8));
        }
        #endregion

        #region Miscellaneous / Control instructions

        /// <summary>
        /// Opcode 0x00 - Causes a non-maskable interrupt and increments the program counter by one
        /// </summary>
        /// <param name="instruction"></param>
        /// <returns></returns>
        public int BRK_Implicit(Instruction instruction)
        {            
            PC += 2;

            //Signal a break command is issued by setting Break Flag
            _flags.BreakCommand = 1;            

            //Push previous incremented PC to the stack
            _stack.PushWord(PC);
            //Push status register into the stack
            _stack.Push(_flags.Register);

            //Disable Interrupts
            _flags.IRQDisabled = 1;

            //PC now point to the vector routine
            PC = _mcu.ReadWord(0xFFFE);               
            return instruction.tCycles;
        }

        /// <summary>
        /// Opcode 0xEA - No operatione executed
        /// </summary>
        /// <param name="instruction"></param>
        /// <returns></returns>
        public int NOP_Implicit(Instruction instruction)
        {
            PC += instruction.length;
            return instruction.tCycles;
        }

        /// <summary>
        /// Opcode 0xD8 - Claar decimal mode flag
        /// </summary>
        /// <param name="instruction">Instance of instruction details</param>
        /// <returns>Number of cycles required</returns>
        public int CLD_Implicit(Instruction instruction)
        {
            _flags.DFlag = 0;

            PC += instruction.length;
            return instruction.tCycles;
        }

        /// <summary>
        /// Opcode 0xF8 - Set decimal mode flag
        /// </summary>
        /// <param name="instruction">Instance of instruction details</param>
        /// <returns>Number of cycles required</returns>
        public int SED_Implicit(Instruction instruction)
        {
            _flags.DFlag = 1;

            PC += instruction.length;
            return instruction.tCycles;
        }

        /// <summary>
        /// Opcode 0x78 - Set interrupt disable bit
        /// </summary>
        /// <param name="instruction">Instance of instruction details</param>
        /// <returns></returns>
        public int SEI_Implicit(Instruction instruction)
        {
            _flags.IRQDisabled = 1;

            PC += instruction.length;
            return instruction.tCycles;
        }

        /// <summary>
        /// Opcode 0x58 - Clear interrupt disable bit
        /// </summary>
        /// <param name="instruction">Instance of instruction details</param>
        /// <returns>Number of cycles required</returns>
        public int CLI_Implicit(Instruction instruction)
        {
            _flags.IRQDisabled = 0;

            PC += instruction.length;
            return instruction.tCycles;
        }
        #endregion

        #region Load Instructions
        /// <summary>
        /// Opcode 0xA9 - Load accmulator with immediate value into A
        /// </summary>
        /// <param name="instruction">Instance of class contaning instruction details</param>
        /// <returns>Number of instruction clock cycles</returns>
        public int LDA_Immediate(Instruction instruction)
        {
            A = NextByte();

            _flags.ComputeZFlag(A);
            _flags.ComputeNFlag(A);

            PC += instruction.length;
            return instruction.tCycles;
        }

        /// <summary>
        /// Opcode 0xAD - Load accumulato from absolute value in memory
        /// </summary>
        /// <param name="instruction">instance of class containing instruction details</param>
        /// <returns>Number of instruction clock cycles</returns>
        public int LDA_Absolute(Instruction instruction)
        {            
            A = _addressing.ReadAbsoluteValue(NextWord());

            _flags.ComputeZFlag(A);
            _flags.ComputeNFlag(A);

            PC += instruction.length;
            return instruction.tCycles;
        }

        /// <summary>
        /// Opcode 0xA5 - Load accumulator from ZPage 
        /// </summary>
        /// <param name="instruction">instance of class containing instruction details</param>
        /// <returns>Number of instruction clock cycles</returns>
        public int LDA_ZPage(Instruction instruction)
        {            
            A = _addressing.ReadZeroPage(NextByte());

            _flags.ComputeZFlag(A);
            _flags.ComputeNFlag(A);

            PC += instruction.length;
            return instruction.tCycles;
        }

        /// <summary>
        /// Opcode 0xBD - Load accumulator from absolute address adding X
        /// </summary>
        /// <param name="instruction">instance of class containing instruction details</param>
        /// <returns></returns>
        internal int LDA_AbsoluteX(Instruction instruction)
        {
            int tCycles = 0;
            ushort address = NextWord();

            A = _addressing.ReadAbsoluteIndexedAddressing(address, X); 

            _flags.ComputeZFlag(A);
            _flags.ComputeNFlag(A);

            if (_addressing.DetectCrossPageBoundary(address, (ushort)(address + X)))
                tCycles = instruction.tCycles + 1; 

            PC += instruction.length;
            return tCycles;
        }

        /// <summary>
        /// Opcode B9 - Load accumulator from absolute address adding Y
        /// </summary>
        /// <param name="instruction">instance of class containing instruction details</param>
        /// <returns></returns>
        internal int LDA_AbsoluteY(Instruction instruction)
        {
            int tCycles = 0;
            ushort address = NextWord();

            A = _addressing.ReadAbsoluteIndexedAddressing(address, Y);

            _flags.ComputeZFlag(A);
            _flags.ComputeNFlag(A);

            if (_addressing.DetectCrossPageBoundary(address, (ushort)(address + Y)))
                tCycles = instruction.tCycles + 1;

            PC += instruction.length;
            return tCycles;
        }

        /// <summary>
        /// Opcode 0xB5 - Load accumulator from Zero Page with indexed adressing
        /// </summary>
        /// <param name="instruction">instance of class containing instruction details</param>
        /// <returns></returns>
        public int LDA_ZPageX(Instruction instruction)
        {           
            A = _addressing.ReadZPageByIndexedAddressing(NextByte(), X);

            _flags.ComputeZFlag(A);
            _flags.ComputeNFlag(A);

            PC += instruction.length;
            return instruction.tCycles;
        }

        /// <summary>
        /// Opcode 0xB1 - Load accumulator from Zero Page with indirect indexed adessing
        /// </summary>
        /// <param name="instruction">instance of class containing instruction details</param>
        /// <returns></returns>
        public int LDA_ZPageIndirectIndexedY(Instruction instruction)
        {           
            A = _addressing.ReadZPageByIndirectIndexedAddressing(NextByte(), Y);

            _flags.ComputeZFlag(A);
            _flags.ComputeNFlag(A);

            PC += instruction.length;
            return instruction.tCycles;
        }

        /// <summary>
        /// Opcode 0xA1 - Load accumulator from Zero Page with indexed indirect adessing
        /// </summary>
        /// <param name="instruction">instance of class containing instruction details</param>
        /// <returns></returns>
        public int LDA_ZPageIndexedIndirectX(Instruction instruction)
        {            
            A = _addressing.ReadZPageByIndexedIndirectAddressing(NextByte(), X);

            _flags.ComputeZFlag(A);
            _flags.ComputeNFlag(A);

            PC += instruction.length;
            return instruction.tCycles;
        }

        /// <summary>
        /// Opcode 0xA2 - Load X register with immediate value
        /// </summary>
        /// <param name="instruction">instance of class containing instruction details</param>
        /// <returns></returns>
        public int LDX_Immediate(Instruction instruction)
        {
            X = NextByte();

            _flags.ComputeZFlag(X);
            _flags.ComputeNFlag(X);

            PC += instruction.length;
            return instruction.tCycles;
        }

        /// <summary>
        /// Opcode 0xAE - Load X register with absolute value taken from memory 
        /// </summary>
        /// <param name="instruction">instance of class containing instruction details</param>
        /// <returns></returns>
        public int LDX_Absolute(Instruction instruction)
        {
            X = _addressing.ReadAbsoluteValue(NextWord());

            _flags.ComputeZFlag(X);
            _flags.ComputeNFlag(X);

            PC += instruction.length;
            return instruction.tCycles;
        }

        /// <summaryd
        /// Opcode 0xA6 - Load X Register with value taken from Zero Page 
        /// </summary>
        /// <param name="instruction">instance of class containing instruction details</param>
        /// <returns></returns>
        public int LDX_ZPage(Instruction instruction)
        {            
            X = _addressing.ReadZeroPage(NextByte());

            _flags.ComputeZFlag(X);
            _flags.ComputeNFlag(X);

            PC += instruction.length;
            return instruction.tCycles;
        }

        /// <summary>
        /// Opcode 0xBE - Load X register from memory taking absolute address + Y offset
        /// </summary>
        /// <param name="instruction">instance of class containing instruction details</param>
        /// <returns></returns>
        public int LDX_AbsoluteY(Instruction instruction)
        {
            int tCycles = 0;
            ushort address = NextWord();         
            
            X = _addressing.ReadAbsoluteIndexedAddressing(address, Y);

            _flags.ComputeZFlag(X);
            _flags.ComputeNFlag(X);

            if (_addressing.DetectCrossPageBoundary(address, (ushort)(address + Y)))
                tCycles = instruction.tCycles + 1;
     
            PC += instruction.length;
            return tCycles;
        }

        /// <summary>
        /// Opcode 0xB6 - Load X register from Zero Page adding Y register value to the address
        /// </summary>
        /// <param name="instruction">instance of class containing instruction details</param>
        /// <returns></returns>
        public int LDX_ZpageY(Instruction instruction)
        {           
            X = _addressing.ReadZPageByIndexedAddressing(NextByte(), Y);            

            _flags.ComputeZFlag(X);
            _flags.ComputeNFlag(X);

            PC += instruction.length;
            return instruction.tCycles;
        }

        /// <summary>
        /// Opcode 0xA0 - Load Y register with immediate value
        /// </summary>
        /// <param name="instruction">instance of class containing instruction details</param>
        /// <returns></returns>
        internal int LDY_Immediate(Instruction instruction)
        {
            Y = NextByte();

            _flags.ComputeZFlag(Y);
            _flags.ComputeNFlag(Y);

            PC += instruction.length;
            return instruction.tCycles;
        }

        /// <summary>
        /// Opcode 0xAC - Load Y register with absolute value taken from memory 
        /// </summary>
        /// <param name="instruction">instance of class containing instruction details</param>
        /// <returns></returns>
        public int LDY_Absolute(Instruction instruction)
        {
            Y = _addressing.ReadAbsoluteValue(NextWord());

            _flags.ComputeZFlag(Y);
            _flags.ComputeNFlag(Y);

            PC += instruction.length;
            return instruction.tCycles;
        }

        /// <summary>
        /// Opcode 0xA4 - Load Y Register with value taken from Zero Page 
        /// </summary>
        /// <param name="instruction">instance of class containing instruction details</param>
        /// <returns></returns>
        public int LDY_ZPage(Instruction instruction)
        {            
            Y = _addressing.ReadZeroPage(NextByte());

            _flags.ComputeZFlag(Y);
            _flags.ComputeNFlag(Y);

            PC += instruction.length;
            return instruction.tCycles;
        }

        /// <summary>
        /// Opcode 0xBC - Load Y register from memory taking absolute address + X offset
        /// </summary>
        /// <param name="instruction">instance of class containing instruction details</param>
        /// <returns></returns>
        public int LDY_AbsoluteX(Instruction instruction)
        {
            int tCycles = 0;
            ushort address = NextWord();

            Y = _addressing.ReadAbsoluteIndexedAddressing(address, X);

            _flags.ComputeZFlag(Y);
            _flags.ComputeNFlag(Y);

            if (_addressing.DetectCrossPageBoundary(address, (ushort)(address + X)))
                tCycles = instruction.tCycles + 1;

            PC += instruction.length;
            return tCycles;
        }        

        /// <summary>
        /// Opcode 0xB4 - Load Y register from Zero Page adding X register value to the address
        /// </summary>
        /// <param name="instruction">instance of class containing instruction details</param>
        /// <returns></returns>
        public int LDY_ZpageX(Instruction instruction)
        {           
            Y = _addressing.ReadZPageByIndexedAddressing(NextByte(), X);

            _flags.ComputeZFlag(Y);
            _flags.ComputeNFlag(Y);

            PC += instruction.length;
            return instruction.tCycles;
        }
        #endregion

        #region Transfer Instructions

        /// <summary>
        /// Opcode 0xAA - Transfer accumulator to index X
        /// </summary>
        /// <param name="instruction">Instance of class contaning instruction details</param>
        /// <returns>Number of instruction clock cycles</returns>
        public int TAX_Implicit(Instruction instruction)
        {
            X = A;

            _flags.ComputeZFlag(X);
            _flags.ComputeNFlag(X);            

            PC += instruction.length;
            return instruction.tCycles;
        }

        /// <summary>
        /// Opcode 0x8A - Transfer X to accumulator
        /// </summary>
        /// <param name="instruction">Instance of class contaning instruction details</param>
        /// <returns>Number of instruction clock cycles</returns>
        public int TXA_Implicit(Instruction instruction)
        {
            A = X;

            _flags.ComputeZFlag(A);
            _flags.ComputeNFlag(A);

            PC += instruction.length;
            return instruction.tCycles;
        }

        /// <summary>
        /// Opcode 0xA8 - Transfer accumulator to Y
        /// </summary>
        /// <param name="instruction">Instance of class contaning instruction details</param>
        /// <returns>Number of instruction clock cycles</returns>
        public int TAY_Implicit(Instruction instruction)
        {
            Y = A;

            _flags.ComputeZFlag(Y);
            _flags.ComputeNFlag(Y);

            PC += instruction.length;
            return instruction.tCycles;
        }

        /// <summary>
        /// Opcode 0x98 - Transfer Y to accumulator
        /// </summary>
        /// <param name="instruction">Instance of class contaning instruction details</param>
        /// <returns>Number of instruction clock cycles</returns>
        public int TYA_Implicit(Instruction instruction)
        {
            A = Y;

            _flags.ComputeZFlag(A);
            _flags.ComputeNFlag(A);

            PC += instruction.length;
            return instruction.tCycles;
        }

        /// <summary>
        /// Opcode 0xBA - Transfer stack pointer to X
        /// </summary>
        /// <param name="instruction">Instance of class contaning instruction details</param>
        /// <returns>Number of instruction clock cycles</returns>
        public int TSX_Implicit(Instruction instruction)
        {
            X = (byte)SP;

            _flags.ComputeZFlag(X);
            _flags.ComputeNFlag(X);

            PC += instruction.length;
            return instruction.tCycles;
        }

        /// <summary>
        /// Opcode 0x9A - Transfer X to stack pointer
        /// </summary>
        /// <param name="instruction">instruction">Instance of class contaning instruction details</param>
        /// <returns>Number of instruction clock cycles</returns>
        public int TXS_Implicit(Instruction instruction)
        {
            SP = X;
            
            PC += instruction.length;
            return instruction.tCycles;
        }
        #endregion

        #region Stack Pointer Instructions
        /// <summary>
        /// Opcode 0x48 - Push accumulator into stack pointer
        /// </summary>
        /// <param name="instruction">Instance of class contaning instruction details</param>
        /// <returns>Number of instruction clock cycles</returns>
        public int PHA_Implicit(Instruction instruction)
        {
            //Stack pointer range from 0x0100 TO 0x01FF            
            _stack.Push(A);
            
            PC += instruction.length;
            return instruction.tCycles;
        }

        /// <summary>
        /// Opcode 0x68 - Pop value from stack pointer into accumulator
        /// </summary>
        /// <param name="instruction">Instance of class contaning instruction details</param>
        /// <returns>Number of instruction clock cycles</returns>
        public int PLA_Implicit(Instruction instruction)
        {
            A = _stack.Pop();

            Flags.ComputeNFlag(A);
            Flags.ComputeZFlag(A);

            PC += instruction.length;
            return instruction.tCycles;
        }

        /// <summary>
        /// Opcode 0x08 - Push status flag register into stack pointer
        /// </summary>
        /// <param name="instruction">Instance of class contaning instruction details</param>
        /// <returns>Number of instruction clock cycles</returns>
        public int PHP_Implicit(Instruction instruction)
        {
            //Stack pointer range from 0x0100 TO 0x01FF
            _stack.Push(_flags.Register);            

            PC += instruction.length;
            return instruction.tCycles;
        }

        /// <summary>
        /// Opcode 0x28 - Pull status flag register from stack pointer and restore status of every flag 
        /// </summary>
        /// <param name="instruction">Instance of class contaning instruction details</param>
        /// <returns>Number of instruction clock cycles</returns>
        public int PLP_Implicit(Instruction instruction)
        {
            _flags.Register = _stack.Pop();            

            //On PLP Instruction Break Flag must be set to 1
            _flags.BreakCommand = 1;

            PC += instruction.length;
            return instruction.tCycles;
        }

        #endregion

        #region Store Instruction
        /// <summary>
        /// Opcode 0x8D - Store accumulatot in memory 
        /// <param name="instruction">instance of class containing instruction details</param>
        /// <returns>Number of instruction clock cycles</returns>
        /// </summary>
        public int STA_Absolute(Instruction instruction)
        {         
            _addressing.WriteAbsoluteValue(NextWord(), A);

            PC += instruction.length;
            return instruction.tCycles;
        }

        /// <summary>
        /// Opcode 0x85 - Store accumulator in ZPage memory 
        /// </summary>
        ///<param name="instruction">instance of class containing instruction details</param>
        /// <returns>Number of instruction clock cycles</returns>
        public int STA_ZPage(Instruction instruction)
        {
            _addressing.WriteZeroPage(NextByte(), A);

            PC += instruction.length;
            return instruction.tCycles;
        }

        /// <summary>
        /// Opcode 0x9D - Store accumulator in memory with X offset
        /// </summary>
        /// <param name="instruction">instruction">instance of class containing instruction details</param>
        /// <returns>Number of instruction clock cycles</returns>
        public int STA_AbsoluteX(Instruction instruction)
        {
            _addressing.WriteAbsoluteIndexedAddressing(NextWord(), X, A);

            PC += instruction.length;
            return instruction.tCycles;
        }

        /// <summary>
        /// Opcode 0x99 - Store accumulator in memory with Y offset
        /// </summary>
        /// <param name="instruction">instruction">instance of class containing instruction details</param>
        /// <returns>Number of instruction clock cycles</returns>
        public int STA_AbsoluteY(Instruction instruction)
        {
            _addressing.WriteAbsoluteIndexedAddressing(NextWord(), Y, A);

            PC += instruction.length;
            return instruction.tCycles;
        }

        /// <summary>
        /// Opcode 0x95 - Store accumulator in ZPage at the address computed by the sum
        /// of next consecutive byte and register X
        /// </summary>
        /// <param name="instruction">instruction">instance of class containing instruction details</param>
        /// <returns>Number of instruction clock cycles</returns>
        public int STA_ZPageIndexedX(Instruction instruction)
        {
            _addressing.WriteZpageByIndexedAddressing(NextByte(), X, A);

            PC += instruction.length;
            return instruction.tCycles;
        }

        /// <summary>
        /// Opcode 0x91 - Store accumulator in ZPage using indirect indexed addressing
        /// </summary>
        /// <param name="instruction">instruction">instance of class containing instruction details</param>
        /// <returns>Number of instruction clock cycles</returns>
        public int STA_ZPageIndirectIndexedY(Instruction instruction)
        {
            _addressing.WriteZPageByIndirectIndexedAddressing(NextByte(), Y, A);

            PC += instruction.length;
            return instruction.tCycles;
        }

        /// <summary>
        /// Opcode 0x81 - Store accumulator in ZPage memory area computing 
        /// </summary>
        /// <param name="instruction">instruction">instance of class containing instruction details</param>
        /// <returns>Number of instruction clock cycles</returns>
        public int STA_ZPageIndexedIndirectX(Instruction instruction)
        {
            _addressing.WriteZPageByIndexedIndirectAddressing(NextByte(), X, A);

            PC += instruction.length;
            return instruction.tCycles;
        }

        /// <summary>
        /// Opcode 0x8E - Store X register in memory by absolute value from instruction opcode
        /// </summary>
        /// <param name="instruction">instruction">instance of class containing instruction details</param>
        /// <returns>Number of instruction clock cycles</returns>
        public int STX_Absolute(Instruction instruction)
        {
            _addressing.WriteAbsoluteValue(NextWord(), X);            

            PC += instruction.length;
            return instruction.tCycles;
        }

        /// <summary>
        /// Opcode 0x86 - Store X register content in Zero Page memory area
        /// </summary>
        /// <param name="instruction">instruction">instance of class containing instruction details</param>
        /// <returns>Number of intruction clock cycle</returns>
        public int STX_ZPage(Instruction instruction)
        {
            _addressing.WriteZeroPage(NextByte(), X);

            PC += instruction.length;
            return instruction.tCycles;
        }

        /// <summary>
        /// Opcode 96 - Store X register content in ZPage through indexed addressing
        /// </summary>
        /// <param name="instruction">instruction">instance of class containing instruction details</param>
        /// <returns>Number of intruction clock cycle</returns>
        public int STX_ZPageIndexedY(Instruction instruction)
        {
            _addressing.WriteZpageByIndexedAddressing(NextByte(), Y, X);

            PC += instruction.length;
            return instruction.tCycles;
        }

        /// <summary>
        /// Opcode 0x8C - Store Y register in memory by absolute value from instruction opcode
        /// </summary>
        /// <param name="instruction">instruction">instance of class containing instruction details</param>
        /// <returns>Number of intruction clock cycle</returns>
        public int STY_Absolute(Instruction instruction)
        {
            _addressing.WriteAbsoluteValue(NextWord(), Y);

            PC += instruction.length;
            return instruction.tCycles;
        }

        /// <summary>
        /// Opcode 0x84 - Store Y register content in Zero Page memory area
        /// </summary>
        /// <param name="instruction">instruction">instance of class containing instruction details</param>
        /// <returns>Number of intruction clock cycle</returns>
        public int STY_ZPage(Instruction instruction)
        {
            _addressing.WriteZeroPage(NextByte(), Y);

            PC += instruction.length;
            return instruction.tCycles;
        }

        /// <summary>
        /// Opcode 0x94 - Store Y register content in ZPage through indexed addressing
        /// </summary>
        /// <param name="instruction">instruction">instance of class containing instruction details</param>
        /// <returns>Number of intruction clock cycle</returns>
        public int STY_ZPageIndexedX(Instruction instruction)
        {
            _addressing.WriteZpageByIndexedAddressing(NextByte(), X, Y);

            PC += instruction.length;
            return instruction.tCycles;
        }

        #endregion

        #region Shift and Rotation Instructions

        /// <summary>
        /// Opcode 0x2C - Perform AND between accumulator and input data using absolute addressing
        /// </summary>
        /// <param name="instruction">instruction">instance of class containing instruction details</param>
        /// <returns>Number of instruction clock cycles</returns>
        public int BIT_Absolute(Instruction instruction)
        {
            byte data = _addressing.ReadAbsoluteValue(NextWord());

            _alu.PerformBIT(A, data);

            PC += instruction.length;
            return instruction.tCycles;
        }

        /// <summary>
        /// Opcode 0x24 -Perform AND between accumulator and input data usin zero page addressing
        /// </summary>
        /// <param name="instruction">instruction">instruction">instance of class containing instruction details</param>
        /// <returns>Number of instruction clock cycles</returns>
        public int BIT_ZPage(Instruction instruction)
        {
            byte data = _addressing.ReadZeroPage(NextByte());

            _alu.PerformBIT(A, data);

            PC += instruction.length;
            return instruction.tCycles;
        }

        /// <summary>
        /// Opcode 0x0E - Perform ASL (Shift Left) using absolute addressing 
        /// 
        /// </summary>
        /// <param name="instruction">instruction">instruction">instance of class containing instruction details</param>
        /// <returns>Number of instruction clock cycles</returns>
        public int ASL_Absolute(Instruction instruction)
        {
            ushort address = NextWord();
            
            byte data = _addressing.ReadAbsoluteValue(NextWord());
            byte msb = (byte)((data & 0x80) >> 7);

            byte result = (byte)((data << 1) & 0xFF);
            _addressing.WriteAbsoluteValue(address, result);            

            _flags.CFlag = msb;
            _flags.ComputeNFlag(result);
            _flags.ComputeZFlag(result);

            PC += instruction.length;
            return instruction.tCycles;
        }

        /// <summary>
        /// Opcode 0x06 - Perform ASL (Shift Left) using zero page addressing 
        /// </summary>
        /// <param name="instruction">instruction">instruction">instance of class containing instruction details</param>
        /// <returns>Number of instruction clock cycles</returns>
        public int ASL_ZPage(Instruction instruction)
        {
            ushort address = NextByte();

            byte data = _addressing.ReadZeroPage(address);
            byte msb = (byte)((data & 0x80) >> 7);

            byte result = (byte)((data << 1) & 0xFF);
            _addressing.WriteZeroPage(address, result);

            _flags.CFlag = msb;
            _flags.ComputeNFlag(result);
            _flags.ComputeZFlag(result);

            PC += instruction.length;
            return instruction.tCycles;
        }

        /// <summary>
        /// Opcode 0x0A - Perform ASL (Shift Left) on accumulator 
        /// </summary>
        /// <param name="instruction">instruction">instruction">instance of class containing instruction details</param>
        /// <returns>Number of instruction clock cycles</returns>
        public int ASL_Accumulator(Instruction instruction)
        {
            byte msb = (byte)((A & 0x80) >> 7);
            A = (byte)((A << 1) & 0xFF);

            _flags.CFlag = msb;
            _flags.ComputeNFlag(A);
            _flags.ComputeZFlag(A);

            PC += instruction.length;
            return instruction.tCycles;
        }

        /// <summary>
        /// Opcode 0x1E - Perform ASL (Shift Left) usin absolute indirect addressing
        /// </summary>
        /// <param name="instruction">instruction">instruction">instance of class containing instruction details</param>
        /// <returns>Number of instruction clock cycles</returns><
        public int ASL_AbsoluteX(Instruction instruction)
        {
            ushort address = NextWord();

            byte data = _addressing.ReadAbsoluteIndexedAddressing(address, X);
            byte msb = (byte)((data & 0x80) >> 7);

            byte result = (byte)((data << 1) & 0xFF);
            _addressing.WriteAbsoluteIndexedAddressing(address, X, result);

            _flags.CFlag = msb;
            _flags.ComputeNFlag(result);
            _flags.ComputeZFlag(result);

            PC += instruction.length;
            return instruction.tCycles;
        }

        /// <summary>
        /// Opcode 0x16 - Perform ASL (Shift Left) usin zero page indexed addressing
        /// </summary>
        /// <param name="instruction">instruction">instance of class containing instruction details</param>
        /// <returns>Number of instruction clock cycles</returns>
        public int ASL_ZPageX(Instruction instruction)
        {
            ushort address = NextByte();

            byte data = _addressing.ReadZPageByIndexedAddressing(address, X);
            byte msb = (byte)((data & 0x80) >> 7);

            byte result = (byte)((data << 1) & 0xFF);
            _addressing.WriteZpageByIndexedAddressing(address, X, result);

            _flags.CFlag = msb;
            _flags.ComputeNFlag(result);
            _flags.ComputeZFlag(result);

            PC += instruction.length;
            return instruction.tCycles;
        }

        /// <summary>
        /// Opcode 0x4E - Perform logical shift right using absolute addressing 
        /// </summary>
        /// <param name="instruction">instruction">instance of class containing instruction details</param>
        /// <returns>Number of instruction clock cycles</returns>
        public int LSR_Absolute(Instruction instruction)
        {
            ushort address = NextWord();

            byte data = _addressing.ReadAbsoluteValue(address);
            byte lsb = (byte)(data & 0x01);

            byte result = (byte)(data >> 1);
            _addressing.WriteAbsoluteValue(address, result);

            _flags.CFlag = lsb;
            _flags.ComputeNFlag(result);
            _flags.ComputeZFlag(result);

            PC += instruction.length;
            return instruction.tCycles;
        }

        /// <summary>
        /// Opcode 0x46 - Perform logica shift right using zero page addressing
        /// </summary>
        /// <param name="instruction">instruction">instance of class containing instruction details</param>
        /// <returns>Number of instruction clock cycles</returns>
        public int LSR_ZPage(Instruction instruction)
        {
            ushort address = NextByte();

            byte data = _addressing.ReadZeroPage(address);
            byte lsb = (byte)(data & 0x01);

            byte result = (byte)(data >> 1);
            _addressing.WriteAbsoluteValue(address, result);

            _flags.CFlag = lsb;
            _flags.ComputeNFlag(result);
            _flags.ComputeZFlag(result);

            PC += instruction.length;
            return instruction.tCycles;
        }

        /// <summary>
        /// Perform logical shift right of accumulator register
        /// </summary>
        /// <param name="instruction">instance of class containing instruction details</param>
        /// <returns>Number of instruction clock cycles</returns>
        public int LSR_Accumulator(Instruction instruction)
        {
            byte lsb = (byte)(A & 0x01);
            A = (byte)(A >> 1);

            _flags.CFlag = lsb;
            _flags.ComputeNFlag(A);
            _flags.ComputeZFlag(A);

            PC += instruction.length;
            return instruction.tCycles;
        }

        /// <summary>
        /// Opcode 0x5E - Perform logical shift right using absolute indirect addressing
        /// </summary>
        /// <param name="instruction">instance of class containing instruction details</param>
        /// <returns>Number of instruction clock cycles</returns>
        public int LSR_AbsoluteX(Instruction instruction)
        {
            ushort address = NextWord();

            byte data = _addressing.ReadAbsoluteIndexedAddressing(address, X);
            byte lsb = (byte)(data & 0x01);

            byte result = (byte)(data >> 1);
            _addressing.WriteAbsoluteIndexedAddressing(address, X, result);

            _flags.CFlag = lsb;
            _flags.ComputeNFlag(result);
            _flags.ComputeZFlag(result);

            PC += instruction.length;
            return instruction.tCycles;
        }

        /// <summary>
        /// Opcode 0x56 - Perform logical shift right using zero page indexed addressing
        /// </summary>
        /// <param name="instruction">instance of class containing instruction details</param>
        /// <returns>Number of instruction clock cycles</returns>
        public int LSR_ZPageX(Instruction instruction)
        {
            ushort address = NextByte();

            byte data = _addressing.ReadZPageByIndexedAddressing(address, X);
            byte lsb = (byte)(data & 0x01);

            byte result = (byte)(data >> 1);
            _addressing.WriteZpageByIndexedAddressing(address, X, result);

            _flags.CFlag = lsb;
            _flags.ComputeNFlag(result);
            _flags.ComputeZFlag(result);

            PC += instruction.length;
            return instruction.tCycles;
        }

        /// <summary>
        /// Opcode 0x2E - Perorm rotation left through carry using absolute addressing
        /// </summary>
        /// <param name="instruction">instance of class containing instruction details</param>
        /// <returns>Number of instruction clock cycles</returns>
        public int ROL_Absolute(Instruction instruction)
        {
            ushort adddress = NextWord();

            byte data = _addressing.ReadAbsoluteValue(adddress);
            byte msb = (byte)((data & 0x80) >> 7);           

            byte result = (byte)(((data << 1) & 0xFE) | _flags.CFlag);
            _addressing.WriteAbsoluteValue(adddress, result);

            _flags.CFlag = msb;
            _flags.ComputeNFlag(result);
            _flags.ComputeZFlag(result);

            PC += instruction.length;
            return instruction.tCycles;
        }

        /// <summary>
        /// Opcode 0x26 - Perorm rotation left through carry using zero page addressing
        /// </summary>
        /// <param name="instruction">instance of class containing instruction details</param>
        /// <returns>Number of instruction clock cycles</returns<
        public int ROL_ZPage(Instruction instruction)
        {
            ushort address = NextByte();

            byte data = _addressing.ReadZeroPage(address);
            byte msb = (byte)((data & 0x80) >> 7);

            byte result = (byte)(((data << 1) & 0xFE) | _flags.CFlag);
            _addressing.WriteZeroPage(address, result);

            _flags.CFlag = msb;
            _flags.ComputeNFlag(result);
            _flags.ComputeZFlag(result);

            PC += instruction.length;
            return instruction.tCycles;
        }

        /// <summary>
        /// Opcode 0x2a - Perorm rotation left through carry of accumulator
        /// </summary>
        /// <param name="instruction">instance of class containing instruction details</param>
        /// <returns>Number of instruction clock cycles</returns>
        public int ROL_Accumulator(Instruction instruction)
        {
            byte msb = (byte)((A & 0x80) >> 7);
            A = (byte)(((A << 1) & 0xFE) | _flags.CFlag);

            _flags.CFlag = msb;
            _flags.ComputeNFlag(A);
            _flags.ComputeZFlag(A);

            PC += instruction.length;
            return instruction.tCycles;
        }

        /// <summary>
        /// Opcode 0x3E - Perorm rotation left through carry using absolute indirect addressing
        /// </summary>
        /// <param name="instruction">instance of class containing instruction details</param>
        /// <returns>Number of instruction clock cycles</returns>
        public int ROL_AbsoluteX(Instruction instruction)
        {
            ushort address = NextWord();
            
            byte data = _addressing.ReadAbsoluteIndexedAddressing(address, X);
            byte msb = (byte)((data & 0x80) >> 7);

            byte result = (byte)(((data << 1) & 0xFE) | _flags.CFlag);
            _addressing.WriteAbsoluteIndexedAddressing(address, X, result);

            _flags.CFlag = msb;
            _flags.ComputeNFlag(result);
            _flags.ComputeZFlag(result);

            PC += instruction.length;
            return instruction.tCycles;
        }

        /// <summary>
        /// Opcode 0x36 - Perorm rotation left through carry using zeropage indexed addressing
        /// </summary>
        /// <param name="instruction">instance of class containing instruction details</param>
        /// <returns>Number of instruction clock cycles</returns>
        public int ROL_ZPageX(Instruction instruction)
        {
            ushort address = NextByte();

            byte data = _addressing.ReadZPageByIndexedAddressing(address, X);
            byte msb = (byte)((data & 0x80) >> 7);

            byte result = (byte)(((data << 1) & 0xFE) | _flags.CFlag);
            _addressing.WriteZpageByIndexedAddressing(address, X, result);

            _flags.CFlag = msb;
            _flags.ComputeNFlag(result);
            _flags.ComputeZFlag(result);

            PC += instruction.length;
            return instruction.tCycles;
        }

        /// <summary>
        /// Opcode 0x6E - Perfom rotate right through carry using absolute addressing
        /// </summary>
        /// <param name="instruction">instance of class containing instruction details</param>
        /// <returns>Number of instruction clock cycles</returns>
        public int ROR_Absolute(Instruction instruction)
        {
            ushort address = NextWord();

            byte data = _addressing.ReadAbsoluteValue(address);
            byte lsb = (byte)(data & 0x01);

            byte result = (byte)(((data >> 1) & 0x7F) | (_flags.CFlag << 7));
            _addressing.WriteAbsoluteValue(address, result);

            _flags.CFlag = lsb;
            _flags.ComputeNFlag(result);
            _flags.ComputeZFlag(result);

            PC += instruction.length;
            return instruction.tCycles;
        }

        /// <summary>
        /// Opcode 0x66 - Perfom rotate right through carry using zero page addressing
        /// </summary>
        /// <param name="instruction">instance of class containing instruction details</param>
        /// <returns>Number of instruction clock cycles</returns>
        public int ROR_ZPage(Instruction instruction)
        {
            ushort address = NextByte();

            byte data = _addressing.ReadZeroPage(address);
            byte lsb = (byte)(data & 0x01);

            byte result = (byte)(((data >> 1) & 0x7F) | (_flags.CFlag << 7));
            _addressing.WriteZeroPage(address, result);

            _flags.CFlag = lsb;
            _flags.ComputeNFlag(result);
            _flags.ComputeZFlag(result);

            PC += instruction.length;
            return instruction.tCycles;
        }

        /// <summary>
        /// Opcode 0x6A - Perfom rotate right through carry of accumulator
        /// </summary>
        /// <param name="instruction">instance of class containing instruction details</param>
        /// <returns>Number of instruction clock cycles</returns>
        public int ROR_Accumulator(Instruction instruction)
        {
            byte lsb = (byte)(A & 0x01);
            A = (byte)(((A >> 1) & 0x7F) | (_flags.CFlag << 7));

            _flags.CFlag = lsb;
            _flags.ComputeNFlag(A);
            _flags.ComputeZFlag(A);

            PC += instruction.length;
            return instruction.tCycles;
        }

        /// <summary>
        /// Opcode 0x7E - Perfom rotate right through carry using absolute indirect addressing
        /// </summary>
        /// <param name="instruction">instance of class containing instruction details</param>
        /// <returns>Number of instruction clock cycles</returns>
        public int ROR_AbsoluteX(Instruction instruction)
        {
            ushort address = NextWord();

            byte data = _addressing.ReadAbsoluteIndexedAddressing(address, X);
            byte lsb = (byte)(data & 0x01);

            byte result = (byte)(((data >> 1) & 0x7F) | (_flags.CFlag << 7));
            _addressing.WriteAbsoluteIndexedAddressing(address, X, result);

            _flags.CFlag = lsb;
            _flags.ComputeNFlag(result);
            _flags.ComputeZFlag(result);

            PC += instruction.length;
            return instruction.tCycles;
        }

        /// <summary>
        /// Opcode 0x76 - Perfom rotate right through carry using xero page indexed addressing
        /// </summary>
        /// <param name="instruction">instance of class containing instruction details</param>
        /// <returns>Number of instruction clock cycles</returns>
        public int ROR_ZPageX(Instruction instruction)
        {
            ushort address = NextByte();

            byte data = _addressing.ReadZPageByIndexedAddressing(address, X);
            byte lsb = (byte)(data & 0x01);

            byte result = (byte)(((data >> 1) & 0x7F) | (_flags.CFlag << 7));
            _addressing.WriteZpageByIndexedAddressing(address, X, result);

            _flags.CFlag = lsb;
            _flags.ComputeNFlag(result);
            _flags.ComputeZFlag(result);

            PC += instruction.length;
            return instruction.tCycles;
        }

        #endregion

        #region Arithmetic Instructions
        /// <summary>
        /// Opcode 0x38 - Set Carry Flag to 1
        /// </summary>
        /// <param name="instruction">instance of class containing instruction details</param>
        /// <returns>Number of instruction clock cycles</returns>
        public int SetCarryFlag(Instruction instruction)
        {
            _flags.CFlag = 1;

            PC += instruction.length;
            return instruction.tCycles;
        }

        /// <summary>
        /// Opcode 0x18 - Set Carry flag to 0
        /// </summary>
        /// <param name="instruction">instance of class containing instruction details</param>
        /// <returns>Number of instruction clock cycles</returns>
        public int ClearCarryFlag(Instruction instruction)
        {
            _flags.CFlag = 0;

            PC += instruction.length;
            return instruction.tCycles;
        }

        /// <summary>
        /// Opcode 0xB8 - Set Overflow (V) to 0 
        /// </summary>
        /// <param name="instruction">instance of class containing instruction details</param>
        /// <returns>Number of instruction clock cycles</returns>
        public int ClearVFlag(Instruction instruction)
        {
            _flags.VFlag = 0;

            PC += instruction.length;
            return instruction.tCycles;
        }

        /// <summary>
        /// Opcode 0x69 - Sum: Accumulator + next consecutive byte and carry flag.
        /// </summary>
        /// <param name="instruction">instance of class containing instruction details</param>
        /// <returns>Number of instruction clock cycles</returns>
        public int ADC_Immediate(Instruction instruction)
        {
            int tCycle = 0;
            _alu.PerformADC(NextByte());

            PC += instruction.length;

            //In decimel mode add an extracycle to perform the instruction
            if (Flags.DFlag == 1)
                tCycle = instruction.tCycles + 1;            

            return tCycle;
        }

        /// <summary>
        /// Opcode 0x6D - Sum: Accumulator + absolute memory value + carry flag
        /// </summary>
        /// <param name="instruction">instance of class containing instruction details</param>
        /// <returns>Number of instruction clock cycles</returns>
        public int ADC_Absolute(Instruction instruction)
        {
            int tCycles = 0;
            _alu.PerformADC(_addressing.ReadAbsoluteValue(NextWord()));            

            PC += instruction.length;

            //In decimel mode add an extracycle to perform the instruction
            if (Flags.DFlag == 1)
                tCycles = instruction.tCycles + 1;

            return tCycles;
        }

        /// <summary>
        /// OpCode 0x65 - Sum Accumulator + value from zpage + carry flag
        /// </summary>
        /// <param name="instruction">instance of class containing instruction details</param>
        /// <returns>Number of instruction clock cycles</returns>
        public int ADC_ZPage(Instruction instruction)
        {
            int tCycles = 0;
            _alu.PerformADC(_addressing.ReadZeroPage(NextByte()));            

            //In decimel mode add an extracycle to perform the instruction
            if (Flags.DFlag == 1)
                tCycles = instruction.tCycles + 1;

            PC += instruction.length;
            return tCycles;
        }

        /// <summary>
        /// Opcode 0x7D - Sum Accumulator + absolute valuue with X offset + cflag 
        /// </summary>
        /// <param name="instruction">instance of class containing instruction details</param>
        /// <returns>Number of instruction clock cycles</returns>
        public int ADC_AbsoluteX(Instruction instruction)
        {
            int tCycles = 0;

            ushort address = NextWord();
            _alu.PerformADC(_addressing.ReadAbsoluteIndexedAddressing(address, X));

            //In decimel mode add an extracycle to perform the instruction
            if (Flags.DFlag == 1)
                tCycles = instruction.tCycles + 1;

            //If detect crosspage boundary
            if (_addressing.DetectCrossPageBoundary(address, (ushort)(address + X)))
                tCycles += 1;

            PC += instruction.length;
            return tCycles;
        }

        /// <summary>
        /// Opcode 0x79 - Sum Accumulator + absolute valuue with Y offset + cflag 
        /// </summary>
        /// <param name="instruction">instance of class containing instruction details</param>
        /// <returns>Number of instruction clock cycles</returns>
        public int ADC_AbsoluteY(Instruction instruction)
        {
            int tCycles = 0;

            ushort address = NextWord();
            _alu.PerformADC(_addressing.ReadAbsoluteIndexedAddressing(address, Y));

            //In decimel mode add an extracycle to perform the instruction
            if (Flags.DFlag == 1)
                tCycles = instruction.tCycles + 1;

            //If detect crosspage boundary
            if (_addressing.DetectCrossPageBoundary(address, (ushort)(address + Y)))
                tCycles += 1;

            PC += instruction.length;
            return tCycles;
        }

        /// <summary>
        /// Opcode 0x75 - Sum accumulator + zpage indexed x + cflag
        /// </summary>
        /// <param name="instruction">instance of class containing instruction details</param>
        /// <returns>Number of instruction clock cycles</returns>
        public int ADC_ZPageIndexedX(Instruction instruction)
        {
            int tCycles = 0;
            _alu.PerformADC(_addressing.ReadZPageByIndexedAddressing(NextByte(), X));

            //In decimel mode add an extracycle to perform the instruction
            if (Flags.DFlag == 1)
                tCycles = instruction.tCycles + 1;

            PC += instruction.length;
            return tCycles;
        }

        /// <summary>
        /// Opcode 0x71 - Sum accumulator + value from zpage ind indexed + cFlag
        /// </summary>
        /// <param name="instruction">instance of class containing instruction details</param>
        /// <returns>Number of instruction clock cycles</returns>
        public int ADC_ZPageIndirectIndexedY(Instruction instruction)
        {
            int tCycles = 0;
            ushort address = NextByte();

            _alu.PerformADC(_addressing.ReadZPageByIndirectIndexedAddressing(address, Y));

            //In decimel mode add an extracycle to perform the instruction
            if (Flags.DFlag == 1)
                tCycles = instruction.tCycles + 1;

            //If detect crosspage boundary
            if (_addressing.DetectCrossPageBoundary(address, (ushort)(address + Y)))
                tCycles += 1;

            PC += instruction.length;
            return tCycles;
        }

        /// <summary>
        /// Opcode 0x61
        /// </summary>
        /// <param name="instruction">instance of class containing instruction details</param>
        /// <returns>Number of instruction clock cycles</returns>
        public int ADC_ZPageIndexedIndirectX(Instruction instruction)
        {            
            _alu.PerformADC(_addressing.ReadZPageByIndexedIndirectAddressing(NextByte(), X));

            PC += instruction.length;
            return instruction.tCycles;            
        }

        /// <summary>
        /// Opcode 0xE9 - Subtract from the accumulator the content of next byte plus carry.
        /// The result is always stored in the accumulator
        /// </summary>
        /// <param name="instruction">instance of class containing instruction details</param>
        /// <returns>Number of instruction clock cycles</returns>
        public int SBC_Immediate(Instruction instruction)
        {
            int tCycles = 0;

            _alu.PerformSBC(NextByte());

            if (Flags.DFlag == 1)
                tCycles = instruction.tCycles + 1;

            PC += instruction.length;
            return tCycles;
        }

        /// <summary>
        /// Opcode 0xED - Subtraction from the accumulator the content of nextword plus carry       
        /// The result is always stored in the accumulator
        /// </summary>
        /// <param name="instruction">instance of class containing instruction details</param>
        /// <returns>Number of instruction clock cycles</returns>
        public int SBC_Absolute(Instruction instruction)
        {
            int tCycles = 0;
            ushort address = NextWord();

            _alu.PerformSBC(_addressing.ReadAbsoluteValue(address));

            if (Flags.DFlag == 1)
                tCycles = instruction.tCycles + 1;            

            PC += instruction.length;
            return tCycles;
        }

        /// <summary>
        /// Opcode 0xE5 - Subtract from accumulator the content from ZPage plus carry       
        /// The result is always stored in the accumulator
        /// </summary>
        /// <param name="instruction">instance of class containing instruction details</param>
        /// <returns>Number of instruction clock cycles</returns>
        public int SBC_ZPage(Instruction instruction)
        {
            int tCycles = 0;
            _alu.PerformSBC(_addressing.ReadZeroPage(NextByte()));

            if (Flags.DFlag == 1)
                tCycles = instruction.tCycles + 1;

            PC += instruction.length;
            return tCycles;
        }

        /// <summary>
        /// Opcode 0xFD - Subtract from accumulator the content of memory area located
        /// at address + X plus carry        
        /// </summary>
        /// <param name="instruction">instance of class containing instruction details</param>
        /// <returns>Number of instruction clock cycles</returns>
        public int SBC_AbsoluteX(Instruction instruction)
        {
            int tCycles = 0;
            ushort address = NextWord();

            _alu.PerformSBC(_addressing.ReadAbsoluteIndexedAddressing(address, X));

            if (Flags.DFlag == 1)
                tCycles = instruction.tCycles + 1;

            if (_addressing.DetectCrossPageBoundary(address, (ushort)(address + X)))
                tCycles += 1;

            PC += instruction.length;
            return tCycles;
        }

        /// <summary>
        /// Opcode 0xF9 - Subtract from accumulator the content of memory area located
        /// at address + Y plus carry        
        /// </summary>
        /// <param name="instruction">instance of class containing instruction details</param>
        /// <returns>Number of instruction clock cycles</returns>
        public int SBC_AbsoluteY(Instruction instruction)
        {
            int tCycles = 0;
            ushort address = NextWord();

            _alu.PerformSBC(_addressing.ReadAbsoluteIndexedAddressing(address, Y));

            if (Flags.DFlag == 1)
                tCycles = instruction.tCycles + 1;

            if (_addressing.DetectCrossPageBoundary(address, (ushort)(address + Y)))
                tCycles += 1;

            PC += instruction.length;
            return tCycles;
        }

        /// <summary>
        /// Opcode 0xF5 - Sutract from accumulator the content of ZPage + X, plus carry 
        /// </summary>
        /// <param name="instruction">instance of class containing instruction details</param>
        /// <returns>Number of instruction clock cycles</returns>
        public int SBC_ZPageX(Instruction instruction)
        {
            int tCycles = 0;
            _alu.PerformSBC(_addressing.ReadZPageByIndexedAddressing(NextByte(), X));

            if (Flags.DFlag == 1)
                tCycles = instruction.tCycles + 1;

            PC += instruction.length;
            return tCycles;
        }

        /// <summary>
        /// Opcode 0xF1 - Subtract from accumulator the content of ZPgage indirect indexed + Y,
        /// plus carry
        /// </summary>
        /// <param name="instruction">instance of class containing instruction details</param>
        /// <returns>Number of instruction clock cycles</returns>
        public int SBC_ZPageIndirectIndexedY(Instruction instruction)
        {
            int tCycles = 0;
            ushort address = NextByte();

            _alu.PerformSBC(_addressing.ReadZPageByIndirectIndexedAddressing(address, Y));

            if (Flags.DFlag == 1)
                tCycles = instruction.tCycles + 1;

            PC += instruction.length;
            return tCycles;
        }

        /// <summary>
        /// Opcode 0xE1 - Subtract from accumulator the content of ZPage indexed indirect X
        /// </summary>
        /// <param name="instruction">instance of class containing instruction details</param>
        /// <returns>Number of instruction clock cycles</returns>
        public int SBC_ZPageIndexedIndirectX(Instruction instruction)
        {
            int tCycles = 0;
            ushort address = NextByte();

            _alu.PerformSBC(_addressing.ReadZPageByIndexedIndirectAddressing(address, X));

            if (Flags.DFlag == 1)
                tCycles = instruction.tCycles + 1;

            PC += instruction.length;
            return tCycles;
        }

        /// <summary>
        /// Opcode 0xCA - Decrement X register by 1
        /// </summary>
        /// <param name="instruction">instance of class containing instruction details</param>
        /// <returns>Number of instruction clock cycles</returns>
        public int DecrementX(Instruction instruction)
        {
            X = _alu.PerformDescrementRegister(X);
           
            PC += instruction.length;
            return instruction.tCycles;
        }

        /// <summary>
        /// Opcode 0xCA - Decrement Y register by 1
        /// </summary>
        /// <param name="instruction">instance of class containing instruction details</param>
        /// <returns>Number of instruction clock cycles</returns>
        public int DecrementY(Instruction instruction)
        {
            Y = _alu.PerformDescrementRegister(Y);

            PC += instruction.length;
            return instruction.tCycles;
        }

        /// <summary>
        /// Opcoode 0xE8 - Increment X register by 1
        /// </summary>
        /// <param name="instruction">instance of class containing instruction details</param>
        /// <returns>Number of instruction clock cycles</returns>
        public int IncrementX(Instruction instruction)
        {
            X = _alu.PerformIncrementRegister(X);

            PC += instruction.length;
            return instruction.tCycles;
        }

        /// <summary>
        /// Opcoode 0xE8 - Increment X register by 1
        /// </summary>
        /// <param name="instruction">instance of class containing instruction details</param>
        /// <returns>Number of instruction clock cycles</returns>
        public int IncrementY(Instruction instruction)
        {
            Y = _alu.PerformIncrementRegister(Y);

            PC += instruction.length;
            return instruction.tCycles;
        }

        /// <summary>
        /// Opcode 0xEE - Increment the cntent of memory address retrieved by nextword by 1
        /// </summary>
        /// <param name="instruction">instance of class containing instruction details</param>
        /// <returns>Number of instruction clock cycles</returns>
        public int INC_Absolute(Instruction instruction)
        {
            ushort address = NextWord();
            byte data = _addressing.ReadAbsoluteValue(address);

            byte result = (byte)(data + 1);
            _addressing.WriteAbsoluteValue(address, result);            

            _flags.ComputeNFlag(result);
            _flags.ComputeZFlag(result);

            PC += instruction.length;
            return instruction.tCycles;
        }

        /// <summary>
        /// Opcode 0xE6 - Increment the content of memory address in ZPage retrived by NextByte
        /// </summary>
        /// <param name="instruction">instruction">instance of class containing instruction details</param>
        /// <returns>Number of instruction clock cycles</returns>
        public int INC_Zpage(Instruction instruction)
        {
            ushort address = NextByte();            
            byte data = _addressing.ReadZeroPage(address);

            byte result = (byte)(data + 1);
            _addressing.WriteZeroPage(address, result);            

            _flags.ComputeNFlag(result);
            _flags.ComputeZFlag(result);

            PC += instruction.length;
            return instruction.tCycles;
        }

        /// <summary>
        /// Opcode 0xFE - Increment the content of memory using absolute indexed addressing
        /// </summary>
        /// <param name="instruction">instruction">instance of class containing instruction details<</param>
        /// <returns>Number of instruction clock cycles</returns>
        public int INC_AbsoluteX(Instruction instruction)
        {
            int tCycles = 0;

            ushort address = NextWord();
            byte data = _addressing.ReadAbsoluteIndexedAddressing(address, X);

            byte result = (byte)(data + 1);
            _addressing.WriteAbsoluteIndexedAddressing(address, X, result);

            _flags.ComputeNFlag(result);
            _flags.ComputeZFlag(result);

            PC += instruction.length;
            return tCycles;
        }

        /// <summary>
        /// Opcode 0xF6 - Increment the content of memory using Zero Page indexed addressing
        /// </summary>
        /// <param name="instruction">instruction">instance of class containing instruction details</param>
        /// <returns>Number of instruction clock cycles</returns>
        public int INC_ZPageX(Instruction instruction)
        {
            ushort address = NextByte();
            byte data = _addressing.ReadZPageByIndexedAddressing(address, X);

            byte result = (byte)(data + 1);
            _addressing.WriteZpageByIndexedAddressing(address, X, result);

            _flags.ComputeNFlag(result);
            _flags.ComputeZFlag(result);

            PC += instruction.length;
            return instruction.tCycles;
        }

        /// <summary>
        /// Opcode 0xCE - Decrement the content of memory using absolute addressing
        /// </summary>
        /// <param name="instruction">instruction">instance of class containing instruction details</param>
        /// <returns>Number of instruction clock cycles</returns>
        public int DEC_Absolute(Instruction instruction)
        {
            ushort address = NextWord();
            byte data = _addressing.ReadAbsoluteValue(address);

            byte result = (byte)(data - 1);
            _addressing.WriteAbsoluteValue(address, result);

            _flags.ComputeNFlag(result);
            _flags.ComputeZFlag(result);

            PC += instruction.length;
            return instruction.tCycles;
        }


        /// <summary>
        /// Opcode 0xC6 . Decrement the content of memotry using Zero Page addressing
        /// </summary>
        /// <param name="instruction">instruction">instance of class containing instruction details</param>
        /// <returns>Number of instruction clock cycles</returns>
        public int DEC_ZPage(Instruction instruction)
        {
            ushort address = NextByte();
            byte data = _addressing.ReadZeroPage(address);

            byte result = (byte)(data - 1);
            _addressing.WriteZeroPage(address, result);

            _flags.ComputeNFlag(result);
            _flags.ComputeZFlag(result);

            PC += instruction.length;
            return instruction.tCycles;
        }

        /// <summary>
        /// Opcode 0xDE - Decrement the content of memory using absolute indexed addressing
        /// </summary>
        /// <param name="instruction">instruction">instance of class containing instruction details</param>
        /// <returns>Number of instruction clock cycles</returns>
        public int DEC_AbsoluteX(Instruction instruction)
        {
            ushort address = NextWord();
            byte data = _addressing.ReadAbsoluteIndexedAddressing(address, X);

            byte result = (byte)(data - 1);
            _addressing.WriteAbsoluteIndexedAddressing(address, X, result);

            _flags.ComputeNFlag(result);
            _flags.ComputeZFlag(result);

            PC += instruction.length;
            return instruction.tCycles;
        }

        /// <summary>
        /// Opcode 0xD6 - Decrement the content of memory using Zero Page indexed addressing
        /// </summary>
        /// <param name="instruction">instruction">instance of class containing instruction details</param>
        /// <returns>Number of instruction clock cycles</returns>
        public int DEC_ZpageX(Instruction instruction)
        {
            ushort address = NextByte();
            byte data = _addressing.ReadZPageByIndexedAddressing(address, X);

            byte result = (byte)(data - 1);
            _addressing.WriteZpageByIndexedAddressing(address, X, result);

            _flags.ComputeNFlag(result);
            _flags.ComputeZFlag(result);

            PC += instruction.length;
            return instruction.tCycles;
        }

        /// <summary>
        /// Opcode 0x29 - Perform AND bit a bit using immediate addressing
        /// </summary>
        /// <param name="instruction">instruction">instance of class containing instruction details</param>
        /// <returns>Number of instruction clock cycles</returns>
        public int AND_Immediate(Instruction instruction)
        {
            _alu.PerformAND(A, NextByte());

            PC += instruction.length;
            return instruction.tCycles;
        }

        /// <summary>
        /// Opcode 0x2D - Perform AND bit a bit using absolute addressing
        /// </summary>
        /// <param name="instruction">instruction">instance of class containing instruction details</param>
        /// <returns>Number of instruction clock cycles</returns>
        public int AND_Absolute(Instruction instruction)
        {
            _alu.PerformAND(A, _addressing.ReadAbsoluteValue(NextWord()));

            PC += instruction.length;
            return instruction.tCycles;
        }

        /// <summary>
        /// Opcode 0x25 - Perform AND bit a bit using Zero Page addressing
        /// </summary>
        /// <param name="instruction">instruction">instance of class containing instruction details</param>
        /// <returns>Number of instruction clock cycles</returns>
        public int AND_ZPage(Instruction instruction)
        {
            _alu.PerformAND(A, _addressing.ReadZeroPage(NextByte()));

            PC += instruction.length;
            return instruction.tCycles;
        }

        /// <summary>
        /// Opcode 0x3D - Perform AND bit a bit using abolute indexed addressing 
        /// </summary>
        /// <param name="instruction">instruction">instance of class containing instruction details</param>
        /// <returns>Number of instruction clock cycles</returns>
        public int AND_AbsoluteX(Instruction instruction)
        {
            int tCycles = 0;
            ushort address = NextWord();

            _alu.PerformAND(A, _addressing.ReadAbsoluteIndexedAddressing(address, X));

            if (_addressing.DetectCrossPageBoundary(address, (ushort)(address + X)))
                tCycles = instruction.tCycles + 1;

            PC += instruction.length;
            return tCycles;
        }

        /// <summary>
        /// Opcode 0x39 - Perform AND bit a bit using abolute indexed addressing 
        /// </summary>
        /// <param name="instruction">instruction">instance of class containing instruction details</param>
        /// <returns>Number of instruction clock cycles</returns>
        public int AND_AbsoluteY(Instruction instruction)
        {
            int tCycles = 0;
            ushort address = NextWord();

            _alu.PerformAND(A, _addressing.ReadAbsoluteIndexedAddressing(address, Y));

            if (_addressing.DetectCrossPageBoundary(address, (ushort)(address + Y)))
                tCycles = instruction.tCycles + 1;

            PC += instruction.length;
            return tCycles;
        }

        /// <summary>
        /// Opcode 0x35 - Perform AND bit a bit using zero page indexed addressing
        /// </summary>
        /// <param name="instruction">instruction">instance of class containing instruction details</param>
        /// <returns>Number of instruction clock cycles</returns>
        public int AND_ZPageX(Instruction instruction)
        {
            ushort address = NextByte();
            _alu.PerformAND(A, _addressing.ReadZPageByIndexedAddressing(address, X));

            PC += instruction.length;
            return instruction.tCycles;
        }

        /// <summary>
        /// Opcode - 0x31 Perform  AND bit a bit using zero page indirect indexed addressing
        /// </summary>
        /// <param name="instruction">instruction">instance of class containing instruction details</param>
        /// <returns>Number of instruction clock cycles</returns>
        public int AND_ZPageIndirectIndexedY(Instruction instruction)
        {
            int tCycles = 0;
            ushort address = NextByte();
            
            _alu.PerformAND(A, _addressing.ReadZPageByIndirectIndexedAddressing(address, Y));

            if (_addressing.DetectCrossPageBoundary(address, (ushort)(address + Y)))
                tCycles = instruction.tCycles + 1;

            PC += instruction.length;
            return tCycles;
        }

        /// <summary>
        /// Opcode 0x21- Perform AND bit a bit using zero page indexed indirect addressing
        /// </summary>
        /// <param name="instruction">instruction">instance of class containing instruction details</param>
        /// <returns>Number of instruction clock cycles</returns>
        public int AND_ZPageIndexedIndirectX(Instruction instruction)
        {
            ushort address = NextByte();
            _alu.PerformAND(A, _addressing.ReadZPageByIndexedIndirectAddressing(address, X));

            PC += instruction.length;
            return instruction.tCycles;
        }

        /// <summary>
        /// Opcode 0x49 - Perform Exclusive OR bit a bit using immediate value addressing
        /// </summary>
        /// <param name="instruction">instance of class containing instruction details</param>
        /// <returns>Number of instruction clock cycles</returns>
        public int EOR_Immediate(Instruction instruction)
        {
            _alu.PerformEOR(A, NextByte());

            PC += instruction.length;
            return instruction.tCycles;
        }

        /// <summary>
        /// Opcode 0x4D - Perform Exclusive OR bit a bit using absolute value addressing
        /// </summary>
        /// <param name="instruction">instance of class containing instruction details</param>
        /// <returns>Number of instruction clock cycles</returns>
        public int EOR_Absolute(Instruction instruction)
        {
            ushort address = NextWord();
            _alu.PerformEOR(A, _addressing.ReadAbsoluteValue(address));

            PC += instruction.length;
            return instruction.tCycles;
        }

        /// <summary>
        /// Opcode 0x45 - Perform Exclusive OR bit a bit using zero page addressing
        /// </summary>
        /// <param name="instruction">instance of class containing instruction details</param>
        /// <returns>Number of instruction clock cycles</returns>
        public int EOR_ZPage(Instruction instruction)
        {
            ushort address = NextByte();
            _alu.PerformEOR(A, _addressing.ReadZeroPage(address));

            PC += instruction.length;
            return instruction.tCycles;
        }

        /// <summary>
        /// Opcode 0x5D - Perform Exclusive OR bit a bit using absolute indirect addressing
        /// </summary>
        /// <param name="instruction">instance of class containing instruction details</param>
        /// <returns>instruction">Number of instruction clock cycles</returns>
        public int EOR_AbsoluteX(Instruction instruction)
        {
            int tCycles = 0;
            ushort address = NextWord();

            _alu.PerformEOR(A, _addressing.ReadAbsoluteIndexedAddressing(address, X));

            if (_addressing.DetectCrossPageBoundary(address, (ushort)(address + X)))
                tCycles = instruction.tCycles + 1;

            PC += instruction.length;
            return tCycles;
        }

        /// <summary>
        /// Opcode 0x59 - Perform Exclusive OR bit a bit using absolute indirect addressing
        /// </summary>
        /// <param name="instruction">instance of class containing instruction details</param>
        /// <returns>instruction">Number of instruction clock cycles</returns>
        public int EOR_AbsoluteY(Instruction instruction)
        {
            int tCycles = 0;
            ushort address = NextWord();

            _alu.PerformEOR(A, _addressing.ReadAbsoluteIndexedAddressing(address, Y));

            if (_addressing.DetectCrossPageBoundary(address, (ushort)(address + Y)))
                tCycles = instruction.tCycles + 1;

            PC += instruction.length;
            return tCycles;
        }

        /// <summary>
        /// Opcode 0x55 - Perform Exclusive OR bit a bit using zpage indexed addressing
        /// </summary>
        /// <param name="instruction">instance of class containing instruction details</param>
        /// <returns>instruction">Number of instruction clock cycles</returns>
        public int EOR_ZPageX(Instruction instruction)
        {
            ushort address = NextByte();
            _alu.PerformEOR(A, _addressing.ReadZPageByIndexedAddressing(address, X));

            PC += instruction.length;
            return instruction.tCycles;
        }

        /// <summary>
        /// Opcode 0x51 - Perform Exclusive OR bit a bit using zero page indirect indexed addressing
        /// </summary>
        /// <param name="instruction">instance of class containing instruction details</param>
        /// <returns>instruction">Number of instruction clock cycles</returns>
        public int EOR_ZPageIndirectIndexedY(Instruction instruction)
        {
            int tCycles = 0;
            ushort address = NextByte();

            _alu.PerformEOR(A, _addressing.ReadZPageByIndirectIndexedAddressing(address, Y));

            if (_addressing.DetectCrossPageBoundary(address, (ushort)(address + Y)))
                tCycles = instruction.tCycles + 1;

            PC += instruction.length;
            return tCycles;
        }

        /// <summary>
        /// Opcode 0x41 - Perform Exclusive OR bit a bit using zpage indexed indirect addresing
        /// </summary>
        /// <param name="instruction">instance of class containing instruction details</param>
        /// <returns>instruction">Number of instruction clock cycles</returns>
        public int EOR_ZPageIndexedIndirectX(Instruction instruction)
        {
            ushort address = NextByte();

            _alu.PerformEOR(A, _addressing.ReadZPageByIndexedIndirectAddressing(address, X));

            PC += instruction.length;
            return instruction.tCycles;
        }

        /// <summary>
        /// Opcode 0x09 - Perform OR bit a bit using immediate value
        /// </summary>
        /// <param name="instruction">instance of class containing instruction details</param>
        /// <returns>instruction">Number of instruction clock cycles</returns>
        public int ORA_Immediate(Instruction instruction)
        {
            _alu.PerformOR(A, NextByte());

            PC += instruction.length;
            return instruction.tCycles;
        }

        /// <summary>
        /// Opcode 0x9D - Perform OR bit a bit using absolute value addressing
        /// </summary>
        /// <param name="instruction">instance of class containing instruction details</param>
        /// <returns>instruction">Number of instruction clock cycles</returns>
        public int ORA_Absolute(Instruction instruction)
        {
            ushort address = NextWord();
            _alu.PerformOR(A, _addressing.ReadAbsoluteValue(address));

            PC += instruction.length;
            return instruction.tCycles;
        }

        /// <summary>
        /// Opcode 0x05 - Perform OR bit a bit using zero page addressing
        /// </summary>
        /// <param name="instruction">instance of class containing instruction details</param>
        /// <returns>instruction">Number of instruction clock cycles</returns>
        public int ORA_ZPage(Instruction instruction)
        {
            ushort address = NextByte();
            _alu.PerformOR(A, _addressing.ReadZeroPage(NextByte()));

            PC += instruction.length;
            return instruction.tCycles;
        }

        /// <summary>
        /// Opcode 0x1D - Perform OR bit a bit using absolute indirect addressing
        /// </summary>
        /// <param name="instruction">instance of class containing instruction details</param>
        /// <returns>instruction">Number of instruction clock cycles</returns>
        public int ORA_AbsoluteX(Instruction instruction)
        {
            int tCycles = 0;
            ushort address = NextWord();

            _alu.PerformOR(A, _addressing.ReadAbsoluteIndexedAddressing(address, X));

            if (_addressing.DetectCrossPageBoundary(address, (ushort)(address + X)))
                tCycles = instruction.tCycles + 1;

            PC += instruction.length;
            return tCycles;
        }

        /// <summary>
        /// Opcode 0x19 - Perform OR bit a bit using absolute indirect addressing
        /// </summary>
        /// <param name="instruction">instance of class containing instruction details</param>
        /// <returns>instruction">Number of instruction clock cycles</returns>
        public int ORA_AbsoluteY(Instruction instruction)
        {
            int tCycles = 0;
            ushort address = NextWord();

            _alu.PerformOR(A, _addressing.ReadAbsoluteIndexedAddressing(address, Y));

            if (_addressing.DetectCrossPageBoundary(address, (ushort)(address + Y)))
                tCycles = instruction.tCycles + 1;

            PC += instruction.length;
            return tCycles;
        }

        /// <summary>
        /// Opcode 0x15 - Perform OR bit a bit using zero page addressing
        /// </summary>
        /// <param name="instruction">instance of class containing instruction details</param>
        /// <returns>instruction">Number of instruction clock cycles</returns>
        public int ORA_ZPageX(Instruction instruction)
        {
            ushort address = NextByte();
            _alu.PerformOR(A, _addressing.ReadZPageByIndexedAddressing(address, X));

            PC += instruction.length;
            return instruction.tCycles;
        }

        /// <summary>
        /// Opcode 0x11 - Perform OR bit a bit usinf zero page indirect indexed addressing
        /// </summary>
        /// <param name="instruction">instance of class containing instruction details</param>
        /// <returns>instruction">Number of instruction clock cycles</returns>
        public int ORA_ZPageIndirectIndexedY(Instruction instruction)
        {
            int tCycles = 0;
            ushort address = NextByte();

            _alu.PerformOR(A, _addressing.ReadZPageByIndirectIndexedAddressing(address, Y));

            if (_addressing.DetectCrossPageBoundary(address, (ushort)(address + Y)))
                tCycles = instruction.tCycles + 1;

            PC += instruction.length;
            return tCycles;
        }

        /// <summary>
        /// Opcode 0x01 - Perform OR bit a bit using zero page indexed indirect addressing
        /// </summary>
        /// <param name="instruction">instance of class containing instruction details</param>
        /// <returns>Number of instruction clock cycles</returns>
        public int ORA_ZPageIndexedIndirectX(Instruction instruction)
        {
            ushort address = NextByte();
            _alu.PerformOR(A, _addressing.ReadZPageByIndexedIndirectAddressing(address, X));

            PC += instruction.length;
            return instruction.tCycles;
        }

        /// <summary>
        /// Opcode 0xC9 - Compare accumulator using immediate value
        /// </summary>
        /// <param name="instruction">instance of class containing instruction details</param>
        /// <returns>Number of instruction clock cycles</returns>
        public int CMP_Immediate(Instruction instruction)
        {
            _alu.Compare(A, NextByte());

            PC += instruction.length;
            return instruction.tCycles;
        }

        /// <summary>
        /// Opcode 0xCD - Compare accumulator using absolute addressing
        /// </summary>
        /// <param name="instruction">instance of class containing instruction details</param>
        /// <returns>Number of instruction clock cycles</returns>
        public int CMP_Absolute(Instruction instruction)
        {
            _alu.Compare(A, _addressing.ReadAbsoluteValue(NextWord()));

            PC += instruction.length;
            return instruction.tCycles;
        }

        /// <summary>
        /// Opcode 0xC5 - Compare accumulator using zero page addressing
        /// </summary>
        /// <param name="instruction">instance of class containing instruction details</param>
        /// <returns>Number of instruction clock cycles</returns>
        public int CMP_ZPage(Instruction instruction)
        {
            _alu.Compare(A, _addressing.ReadZeroPage(NextByte()));

            PC += instruction.length;
            return instruction.tCycles;
        }

        /// <summary>
        /// Opcode 0xDD - Compare accumulato using absolute indirect addressing
        /// </summary>
        /// <param name="instruction">instance of class containing instruction details</param>
        /// <returns>Number of instruction clock cycles</returns>
        public int CMP_AbsoluteX(Instruction instruction)
        {
            int tCycles = 0;
            ushort address = NextWord();

            _alu.Compare(A, _addressing.ReadAbsoluteIndexedAddressing(address, X));

            if (_addressing.DetectCrossPageBoundary(address, (ushort)(address + X)))
                tCycles = instruction.tCycles + 1;

            PC += instruction.length;
            return tCycles;
        }

        /// <summary>
        /// Opcode 0xD9 - Compare accumulator using absolute indirect addressing
        /// </summary>
        /// <param name="instruction">instance of class containing instruction details</param>
        /// <returns>Number of instruction clock cycles</returns>
        public int CMP_AbsoluteY(Instruction instruction)
        {
            int tCycles = 0;
            ushort address = NextWord();

            _alu.Compare(A, _addressing.ReadAbsoluteIndexedAddressing(address, Y));

            if (_addressing.DetectCrossPageBoundary(address, (ushort)(address + Y)))
                tCycles = instruction.tCycles + 1;

            PC += instruction.length;
            return tCycles;
        }

        /// <summary>
        /// Opcode 0xD5 - Compare accumulator using zero page indexed addressing
        /// </summary>
        /// <param name="instruction">instance of class containing instruction details</param>
        /// <returns>Number of instruction clock cycles</returns>
        public int CMP_ZPageX(Instruction instruction)
        {
            _alu.Compare(A, _addressing.ReadZPageByIndexedAddressing(NextByte(), X));

            PC += instruction.length;
            return instruction.tCycles;
        }

        /// <summary>
        /// Opcode 0xD1 - Compare accumulator using zero page indirect indexed addressing
        /// </summary>
        /// <param name="instruction">instance of class containing instruction details</param>
        /// <returns>Number of instruction clock cycles</returns>
        public int CMP_ZPageIndirectIndexedY(Instruction instruction)
        {
            int tCycles = 0;
            byte address = NextByte();

            _alu.Compare(A, _addressing.ReadZPageByIndirectIndexedAddressing(address, Y));

            if (_addressing.DetectCrossPageBoundary(address, (ushort)(address + Y)))
                tCycles = instruction.tCycles + 1;

            PC += instruction.length;
            return tCycles;
        }

        /// <summary>
        /// Opcode 0xC1 - Compare accumulator using zero page indexed indirect addressing
        /// </summary>
        /// <param name="instruction">instance of class containing instruction details</param>
        /// <returns>Number of instruction clock cycles</returns>
        public int CMP_ZPageIndexedIndirectX(Instruction instruction)
        {
            ushort address = NextWord();
            _alu.Compare(A, _addressing.ReadZPageByIndexedIndirectAddressing(address, X));

            PC += instruction.length;
            return instruction.tCycles;
        }

        /// <summary>
        /// Opcode 0xE0 - Compare X register using immediate value addressing
        /// </summary>
        /// <param name="instruction">instance of class containing instruction details</param>
        /// <returns>Number of instruction clock cycles</returns>
        public int CPX_Immediate(Instruction instruction)
        {
            _alu.Compare(X, NextByte());

            PC += instruction.length;
            return instruction.tCycles;
        }

        /// <summary>
        /// Opcode 0xEC - Compare X register using absolute addressing
        /// <param name="instruction">instance of class containing instruction details</param>
        /// <returns>Number of instruction clock cycles</returns>
        public int CPX_Absolute(Instruction instruction)
        {
            _alu.Compare(X, _addressing.ReadAbsoluteValue(NextWord()));

            PC += instruction.length;
            return instruction.tCycles;
        }

        /// <summary>
        /// Opcode 0xE4- Compare X register using zero page addressing
        /// </summary>
        /// <param name="instruction">instance of class containing instruction details</param>
        /// <returns>Number of instruction clock cycles</returns>
        public int CPX_ZPage(Instruction instruction)
        {
            _alu.Compare(X, _addressing.ReadZeroPage(NextByte()));

            PC += instruction.length;
            return instruction.tCycles;
        }

        /// <summary>
        /// Opcode 0xC0 - Compare Y register using immediate value
        /// </summary>
        /// <param name="instruction">instance of class containing instruction details</param>
        /// <returns>Number of instruction clock cycles</returns>
        public int CPY_Immediate(Instruction instruction)
        {
            _alu.Compare(Y, NextByte());

            PC += instruction.length;
            return instruction.tCycles;
        }

        /// <summary>
        /// Opcode 0xCC - Compare Y register using absolute addressing
        /// </summary>
        /// <param name="instruction">instance of class containing instruction details</param>
        /// <returns>Number of instruction clock cycles</returns>
        public int CPY_Absolute(Instruction instruction)
        {
            _alu.Compare(Y, _addressing.ReadAbsoluteValue(NextWord()));

            PC += instruction.length;
            return instruction.tCycles;
        }

        /// <summary>
        /// Opcode 0xC4 - Compare Y register usinz zero page addressing
        /// </summary>
        /// <param name="instruction">instance of class containing instruction details</param>
        /// <returns>Number of instruction clock cycles</returns>
        public int CPY_ZPage(Instruction instruction)
        {
            _alu.Compare(Y, _addressing.ReadZeroPage(NextByte()));

            PC += instruction.length;
            return instruction.tCycles;
        }

        #endregion

        #region Jump and Branch Instruction

        /// <summary>
        /// Opcode 0x20 - Jump to a subroutine
        /// </summary>
        /// <param name="instruction">instance of class containing instruction details</param>
        /// <returns>Number of instruction clock cycles</returns>
        public int JSR_Absolute(Instruction instruction)
        {
            ushort address = NextWord();
            _stack.PushWord((ushort)(PC + 2));

            PC = address;
            return instruction.tCycles;
        }

        /// <summary>
        /// Opcode 0x40 - Return from interrupt restoring previous PC and Status Flag
        /// </summary>
        /// <param name="instruction"></param>
        /// <returns></returns>
        public int RTI_Implicit(Instruction instruction)
        {
            _flags.Register = _stack.Pop();
            PC = _stack.PopWord();

            return instruction.tCycles;
        }

        /// <summary>
        /// Opcode 0x60 - Return from sub routine
        /// </summary>
        /// <param name="instruction"></param>
        /// <returns></returns>
        public int RTS_Implicit(Instruction instruction)
        {
            PC = _stack.PopWord();
            //Point to the next instruction to be executed
            PC += 1; 
            return instruction.tCycles;
        }

        /// <summary>
        /// Opcode 0x4C - Jump to the absolute value taken from 2nd and 3th byte of instruction
        /// </summary>
        /// <param name="instruction">instance of class containing instruction details</param>
        /// <returns>Number of instruction clock cycles</returns>
        public int JMP_Absolute(Instruction instruction)
        {
            PC = NextWord();                        
            return instruction.tCycles;
        }

        /// <summary>
        /// Opcode 0x6C - Jump using indirect absolute addressing
        /// </summary>
        /// <param name="instruction">instance of class containing instruction details</param>
        /// <returns>Number of instruction clock cycles</returns>
        public int JMP_IndirectAbsolute(Instruction instruction)
        {
            ushort indirectAddress = NextWord();

            byte lowOrdrerByte = _addressing.ReadAbsoluteValue(indirectAddress);
            byte highOrderByte = _addressing.ReadAbsoluteValue((ushort)(indirectAddress + 1));            
            
            PC = (ushort)(lowOrdrerByte | highOrderByte << 8);
            return instruction.tCycles;
        }

        /// <summary>
        /// Jump backword or forward from current program counter
        /// </summary>
        /// <param name="instruction">instance of class containing instruction details</param>
        /// <returns>Number of instruction clock cycles</returns>
        public int JMP_Relative(Instruction instruction)
        {
            int aCycles = instruction.aCycle;

            sbyte address = (sbyte)NextByte();
            PC = (ushort)(PC + instruction.length + address);

            if (_addressing.DetectCrossPageBoundary(PC, (ushort)(PC + address)))
                aCycles += 1;

            return aCycles;
        }

        /// <summary>
        /// Opcode 0xB0 - Jump if Carry flag is SET (1)
        /// </summary>
        /// <param name="instruction">instance of class containing instruction details</param>
        /// <returns>Number of instruction clock cycles</returns>
        public int BCS_Relative(Instruction instruction)
        {
            if (_flags.CFlag == 1)
            {
                return JMP_Relative(instruction);
            }
            else
            {
                PC += instruction.length;
                return instruction.tCycles;
            }
        }

        /// <summary>
        /// Opcode 0x90 - Jump if Carry Flag is CLEAR (0)
        /// </summary>
        /// <param name="instruction">instance of class containing instruction details</param>
        /// <returns>Number of instruction clock cycles</returns>
        public int BCC_Relative(Instruction instruction)
        {
            if (_flags.CFlag == 0)
            {
                return JMP_Relative(instruction);
            }
            else
            {
                PC += instruction.length;
                return instruction.tCycles;
            }
        }

        /// <summary>
        /// Opcode 0xF0 - Jump if Zero Flag is Set (1)
        /// </summary>
        /// <param name="instruction">instance of class containing instruction details</param>
        /// <returns>Number of instruction clock cycles</returns>
        public int BEQ_Relative(Instruction instruction)
        {
            if (_flags.ZFlag == 1)
            {
                return JMP_Relative(instruction);
            }
            else
            {
                PC += instruction.length;
                return instruction.tCycles;
            }
        }

        /// <summary>
        /// Opcode 0xDO - Jump if Zero Flag is clear (0)
        /// </summary>
        /// <param name="instruction">instance of class containing instruction details</param>
        /// <returns>Number of instruction clock cycles</returns>
        public int BNE_Relative(Instruction instruction)
        {
            if (_flags.ZFlag == 0)
            {
                return JMP_Relative(instruction);
            }
            else
            {
                PC += instruction.length;
                return instruction.tCycles;
            }
        }

        /// <summary>
        /// Opcode 0x30 - Jump if Negative Flag is set (1)
        /// </summary>
        /// <param name="instruction">instance of class containing instruction details</param>
        /// <returns>Number of instruction clock cycles</returns>
        public int BMI_Relative(Instruction instruction)
        {
            if(_flags.NFlag == 1)
            {
                return JMP_Relative(instruction);
            }
            else
            {
                PC += instruction.length;
                return instruction.tCycles;
            }
        }

        /// <summary>
        /// Opcode 0x10 - Jump if Negative Flag is clear (0)
        /// </summary>
        /// <param name="instruction">instance of class containing instruction details</param>
        /// <returns>Number of instruction clock cycles</returns>
        public int BPL_Relative(Instruction instruction)
        {
            if (_flags.NFlag == 0)
            {
                return JMP_Relative(instruction);
            }
            else
            {
                PC += instruction.length;
                return instruction.tCycles;
            }
        }

        /// <summary>
        /// Opcode 0x70 - Jump if overflow flag is set (1)
        /// </summary>
        /// <param name="instruction">instance of class containing instruction details</param>
        /// <returns>Number of instruction clock cycles</returns>
        public int BVS_Relative(Instruction instruction)
        {
            if (_flags.VFlag == 1)
            {
                return JMP_Relative(instruction);
            }
            else
            {
                PC += instruction.length;
                return instruction.tCycles;
            }
        }

        /// <summary>
        /// Opcode 0x50- Jump if overflow flag is clear (0)
        /// </summary>
        /// <param name="instruction">instance of class containing instruction details</param>
        /// <returns>Number of instruction clock cycles</returns>
        public int BVC_Relative(Instruction instruction)
        {
            if (_flags.VFlag == 0)
            {
                return JMP_Relative(instruction);
            }
            else
            {
                PC += instruction.length;
                return instruction.tCycles;
            }
        }

        #endregion


        public int FetchAndExecute()
        {          
            //Fetching instruction
            _opcode = _mcu.ReadByte(PC);
            previousPC = PC;
          
            //Decode and execute instruction
            Instruction currentInstruction = _instructions[_opcode];
            int tCycles = currentInstruction.action.Invoke(currentInstruction);

            return tCycles;
        }
    }
}
