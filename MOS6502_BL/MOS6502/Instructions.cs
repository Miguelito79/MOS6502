using System;
using System.Collections.Generic;

namespace MOS6502_BL.MOS6502
{
    public class Instructions : Dictionary<byte, Instruction>
    {
        public static Instructions Get()
        {
            Instructions instructions = new Instructions();

            #region Miscellaneous / Control instructions            
            instructions.Add(0xEA, new Instruction(0xEA, "NOP", 1, 2, 0));
            instructions.Add(0xD8, new Instruction(0xD8, "CLD", 1, 2, 0));
            instructions.Add(0xF8, new Instruction(0xF8, "SED", 1, 2, 0));
            instructions.Add(0x78, new Instruction(0x78, "SEI", 1, 2, 0));
            instructions.Add(0x58, new Instruction(0x58, "CLI", 1, 2, 0));
            instructions.Add(0x00, new Instruction(0x00, "BRK", 1, 7, 0));
            #endregion

            #region Load Instructions
            instructions.Add(0xA9, new Instruction(0xA9, "LDA imm", 2, 2, 0));
            instructions.Add(0xAD, new Instruction(0xAD, "LDA abs", 3, 4, 0));
            instructions.Add(0xA5, new Instruction(0xA5, "LDA zpg", 2, 3, 0));
            instructions.Add(0xBD, new Instruction(0xBD, "LDA abs,x", 3, 4, 0));
            instructions.Add(0xB9, new Instruction(0xB9, "LDA abs,y", 3, 4, 0));
            instructions.Add(0xB5, new Instruction(0xB5, "LDA zpg,x", 2, 4, 0));
            instructions.Add(0xB1, new Instruction(0xB1, "LDA (zpg),y", 2, 5, 0));
            instructions.Add(0xA1, new Instruction(0xA1, "LDA (zpg,x)", 2, 6, 0));
            instructions.Add(0xA2, new Instruction(0xA2, "LDX imm", 2, 2, 0));
            instructions.Add(0xAE, new Instruction(0xAE, "LDX abs", 3, 4, 0));
            instructions.Add(0xA6, new Instruction(0xA6, "LDX zpg", 2, 3, 0));
            instructions.Add(0xBE, new Instruction(0xBE, "LDX abs,y", 3, 4, 0));
            instructions.Add(0xB6, new Instruction(0xB6, "LDX zpg,y", 2, 4, 0));
            instructions.Add(0xA0, new Instruction(0xA0, "LDY imm", 2, 2, 0));
            instructions.Add(0xAC, new Instruction(0xAC, "LDY abs", 3, 4, 0));
            instructions.Add(0xA4, new Instruction(0xA4, "LDY zpg", 2, 3, 0));
            instructions.Add(0xBC, new Instruction(0xBC, "LDY abs,x", 3, 4, 0));
            instructions.Add(0xB4, new Instruction(0xB4, "LDY zpg,x", 2, 4, 0));
            #endregion

            #region Transfer instructions
            instructions.Add(0xAA, new Instruction(0xAA, "TAX", 1, 2, 0));
            instructions.Add(0x8A, new Instruction(0x8A, "TXA", 1, 2, 0));
            instructions.Add(0xA8, new Instruction(0xA8, "TAY", 1, 2, 0));
            instructions.Add(0x98, new Instruction(0x98, "TYA", 1, 2, 0));
            instructions.Add(0xBA, new Instruction(0xBA, "TSX", 1, 2, 0));
            instructions.Add(0x9A, new Instruction(0x9A, "TXS", 1, 2, 0));
            #endregion

            #region Stack Pointer Instructions
            instructions.Add(0x48, new Instruction(0x48, "PHA", 1, 3, 0));
            instructions.Add(0x68, new Instruction(0x68, "PLA", 1, 3, 0));
            instructions.Add(0x08, new Instruction(0x08, "PHP", 1, 3, 0));
            instructions.Add(0x28, new Instruction(0x28, "PLP", 1, 3, 0));
            #endregion

            #region Store Instructions
            instructions.Add(0x8D, new Instruction(0x8D, "STA abs", 3, 4, 0));
            instructions.Add(0x85, new Instruction(0x85, "STA zpg", 2, 3, 0));
            instructions.Add(0x9D, new Instruction(0x9D, "STA abs,x", 3, 5, 0));
            instructions.Add(0x99, new Instruction(0x99, "STA abs,y", 3, 5, 0));
            instructions.Add(0x95, new Instruction(0x95, "STA zpg,x", 2, 4, 0));
            instructions.Add(0x91, new Instruction(0x91, "STA (zpg),y", 2, 6, 0));
            instructions.Add(0x81, new Instruction(0x81, "STA (zpg,x)", 2, 6, 0));
            instructions.Add(0x8E, new Instruction(0x8E, "STX abs", 3, 4, 0));
            instructions.Add(0x86, new Instruction(0x86, "STX zpg", 2, 3, 0));
            instructions.Add(0x96, new Instruction(0x96, "STX zpg,y", 2, 4, 0));
            instructions.Add(0x8C, new Instruction(0x8C, "STY abs", 3, 4, 0));
            instructions.Add(0x84, new Instruction(0x84, "STY zpg", 2, 3, 0));
            instructions.Add(0x94, new Instruction(0x94, "STY zpg,x", 2, 4, 0));
            #endregion

            #region Arithmetic Instructions
            instructions.Add(0x38, new Instruction(0x38, "SEC", 1, 2, 0));
            instructions.Add(0x18, new Instruction(0x18, "CLC", 1, 2, 0));
            instructions.Add(0xB8, new Instruction(0xB8, "CLV", 1, 2, 0));

            instructions.Add(0x69, new Instruction(0x69, "ADC imm", 2, 2, 0));
            instructions.Add(0x6D, new Instruction(0x6D, "ADC abs", 3, 4, 0));
            instructions.Add(0x65, new Instruction(0x65, "ADC zpg", 2, 3, 0));
            instructions.Add(0x7D, new Instruction(0x7D, "ADC abs,x", 3, 4, 0));
            instructions.Add(0x79, new Instruction(0x79, "ADC abs,y", 3, 4, 0));
            instructions.Add(0x75, new Instruction(0x75, "ADC zpg,x", 2, 4, 0));
            instructions.Add(0x71, new Instruction(0x71, "ADC (zpg),y", 2, 5, 0));
            instructions.Add(0x61, new Instruction(0x61, "ADC (zpg,x)", 2, 6, 0));

            instructions.Add(0xE9, new Instruction(0xE9, "SBC imm", 2, 2, 0));
            instructions.Add(0xED, new Instruction(0xED, "SBC abs", 3, 4, 0));
            instructions.Add(0xE5, new Instruction(0xE5, "SBC zpg", 2, 3, 0));
            instructions.Add(0xFD, new Instruction(0xFD, "SBC abs,x", 3, 4, 0));
            instructions.Add(0xF9, new Instruction(0xF9, "SBC abs,y", 3, 4, 0));
            instructions.Add(0xF5, new Instruction(0xF5, "SBC zpg,x", 2, 4, 0));
            instructions.Add(0xF1, new Instruction(0xF1, "SBC (zpg),y", 2, 5, 0));
            instructions.Add(0xE1, new Instruction(0xE1, "SBC (zpg,x)", 2, 6, 0));

            instructions.Add(0xCA, new Instruction(0xCA, "DEX", 1, 2, 0));
            instructions.Add(0x88, new Instruction(0x88, "DEY", 1, 2, 0));
            instructions.Add(0xE8, new Instruction(0xE8, "INX", 1, 2, 0));
            instructions.Add(0xC8, new Instruction(0xC8, "INY", 1, 2, 0));
            instructions.Add(0xEE, new Instruction(0xEE, "INC abs", 3, 6, 0));
            instructions.Add(0xE6, new Instruction(0xE6, "INC zpg", 2, 5, 0));
            instructions.Add(0xFE, new Instruction(0xFE, "INC abs,x", 3, 7, 0));
            instructions.Add(0xF6, new Instruction(0xF6, "INC zpg,x", 2, 6, 0));
            instructions.Add(0xCE, new Instruction(0xCE, "DEC abs", 3, 6, 0));
            instructions.Add(0xC6, new Instruction(0xC6, "DEC zpg", 2, 5, 0));
            instructions.Add(0xDE, new Instruction(0xDE, "DEC abs,x", 3, 7, 0));
            instructions.Add(0xD6, new Instruction(0xD6, "DEC zpg,x", 2, 6, 0));

            instructions.Add(0x29, new Instruction(0x29, "AND imm", 2, 2, 0));
            instructions.Add(0x2D, new Instruction(0x2D, "AND abs", 3, 4, 0));
            instructions.Add(0x25, new Instruction(0x25, "AND zpg", 2, 3, 0));
            instructions.Add(0x3D, new Instruction(0x3D, "AND abs,x", 3, 4, 0));
            instructions.Add(0x39, new Instruction(0x39, "AND abs,y", 3, 4, 0));
            instructions.Add(0x35, new Instruction(0x35, "AND zpg,x", 2, 4, 0));
            instructions.Add(0x31, new Instruction(0x31, "AND (zpg),y", 2, 5, 0));
            instructions.Add(0x21, new Instruction(0x21, "AND (zpg,x)", 2, 6, 0));

            instructions.Add(0x49, new Instruction(0x49, "EOR imm", 2, 2, 0));
            instructions.Add(0x4D, new Instruction(0x4D, "EOR abs", 3, 4, 0));
            instructions.Add(0x45, new Instruction(0x45, "EOR zpg", 2, 3, 0));
            instructions.Add(0x5D, new Instruction(0x5D, "EOR abs,x", 3, 4, 0));
            instructions.Add(0x59, new Instruction(0x59, "EOR abs,y", 3, 4, 0));
            instructions.Add(0x55, new Instruction(0x55, "EOR zpg,x", 2, 4, 0));
            instructions.Add(0x51, new Instruction(0x51, "EOR (zpg),y", 2, 5, 0));
            instructions.Add(0x41, new Instruction(0x41, "EOR (zpg,x)", 2, 6, 0));

            instructions.Add(0x09, new Instruction(0x09, "ORA imm", 2, 2, 0));
            instructions.Add(0x0D, new Instruction(0x0D, "ORA abs", 3, 4, 0));
            instructions.Add(0x05, new Instruction(0x05, "ORA zpg", 2, 3, 0));
            instructions.Add(0x1D, new Instruction(0x1D, "ORA abs,x", 3, 4, 0));
            instructions.Add(0x19, new Instruction(0x19, "ORA abs,Y", 3, 4, 0));
            instructions.Add(0x15, new Instruction(0x15, "ORA zpg,x", 2, 4, 0));
            instructions.Add(0x11, new Instruction(0x11, "ORA (zpg),y", 2, 5, 0));
            instructions.Add(0x01, new Instruction(0x01, "ORA (zpg,x)", 2, 6, 0));

            instructions.Add(0xC9, new Instruction(0xC9, "CMP imm", 2, 2, 0));
            instructions.Add(0xCD, new Instruction(0xCD, "CMP abs", 3, 4, 0));
            instructions.Add(0xC5, new Instruction(0xC5, "CMP zpg", 2, 3, 0));
            instructions.Add(0xDD, new Instruction(0xDD, "CMP abs,x", 3, 4, 0));
            instructions.Add(0xD9, new Instruction(0xD9, "CMP abs,y", 3, 4, 0));
            instructions.Add(0xD5, new Instruction(0xD5, "CMP zpg,x", 2, 4, 0));
            instructions.Add(0xD1, new Instruction(0xD1, "CMP (zpg),y", 2, 5, 0));
            instructions.Add(0xC1, new Instruction(0xC1, "CMP (zpg,x)", 2, 6, 0));
            instructions.Add(0xE0, new Instruction(0xE0, "CPX imm", 2, 2, 0));
            instructions.Add(0xEC, new Instruction(0xEC, "CPX abs", 3, 4, 0));
            instructions.Add(0xE4, new Instruction(0xE4, "CPX zpg", 2, 3, 0));
            instructions.Add(0xC0, new Instruction(0xC0, "CPY imm", 2, 2, 0));
            instructions.Add(0xCC, new Instruction(0xCC, "CPY abs", 3, 4, 0));
            instructions.Add(0xC4, new Instruction(0xC4, "CPY zpg", 2, 3, 0));
            #endregion

            #region Shift and Rotation Instructions
            instructions.Add(0x2C, new Instruction(0x2C, "BIT abs", 3, 4, 0));
            instructions.Add(0x24, new Instruction(0x24, "BIT zpg", 2, 3, 0));

            instructions.Add(0x0E, new Instruction(0x0E, "ASL abs", 3, 6, 0));
            instructions.Add(0x06, new Instruction(0x06, "ASL zpg", 2, 5, 0));
            instructions.Add(0x0A, new Instruction(0x0A, "ASL acc", 1, 2, 0));
            instructions.Add(0x1E, new Instruction(0x1E, "ASL abs,x", 3, 7, 0));
            instructions.Add(0x16, new Instruction(0x16, "ASL zpg,x", 2, 6, 0));

            instructions.Add(0x4E, new Instruction(0x4E, "LSR abs", 3, 6, 0));
            instructions.Add(0x46, new Instruction(0x46, "LSR zpg", 2, 5, 0));
            instructions.Add(0x4A, new Instruction(0x4A, "LSR acc", 1, 2, 0));
            instructions.Add(0x5E, new Instruction(0x5E, "LSR abs,x", 3, 7, 0));
            instructions.Add(0x56, new Instruction(0x56, "LSR zpg,x", 2, 6, 0));

            instructions.Add(0x2E, new Instruction(0x2E, "ROL abs", 3, 6, 0));
            instructions.Add(0x26, new Instruction(0x26, "ROL ZPG", 2, 5, 0));
            instructions.Add(0x2A, new Instruction(0x2A, "ROL acc", 1, 2, 0));
            instructions.Add(0x3E, new Instruction(0x3E, "ROL abs,x", 3, 7, 0));
            instructions.Add(0x36, new Instruction(0x36, "ROL zpg,x", 2, 6, 0));

            instructions.Add(0x6E, new Instruction(0x6E, "ROL abs", 3, 6, 0));
            instructions.Add(0x66, new Instruction(0x66, "ROL ZPG", 2, 5, 0));
            instructions.Add(0x6A, new Instruction(0x6A, "ROL acc", 1, 2, 0));
            instructions.Add(0x7E, new Instruction(0x7E, "ROL abs,x", 3, 7, 0));
            instructions.Add(0x76, new Instruction(0x76, "ROL zpg,x", 2, 6, 0));

            #endregion

            #region Jump and Branch Instruction
            instructions.Add(0x20, new Instruction(0x20, "JSR abs", 3, 6, 0));
            instructions.Add(0x40, new Instruction(0x40, "RTI", 1, 6, 0));
            instructions.Add(0x60, new Instruction(0x60, "RTS", 1, 6, 0));

            instructions.Add(0x4C, new Instruction(0x4C, "JMP abs", 3, 3, 0));
            instructions.Add(0x6C, new Instruction(0x6C, "JMP (abs)", 3, 5, 0));            
            
            instructions.Add(0xB0, new Instruction(0xB0, "BCS rel", 2, 2, 3));
            instructions.Add(0x90, new Instruction(0x90, "BCC rel", 2, 2, 3));
            instructions.Add(0xF0, new Instruction(0xF0, "BEQ rel", 2, 2, 3));
            instructions.Add(0xD0, new Instruction(0xD0, "BNE rel", 2, 2, 3));
            instructions.Add(0x30, new Instruction(0x30, "BMI rel", 2, 2, 3));
            instructions.Add(0x10, new Instruction(0x10, "BPL rel", 2, 2, 3));
            instructions.Add(0x70, new Instruction(0x70, "BVS rel", 2, 2, 3));
            instructions.Add(0x50, new Instruction(0x50, "BVC rel", 2, 2, 3));
            #endregion

            return instructions;
        }

        public void SetAction(CPU cpu)
        {
            #region Miscellaneous / Control Instructions            
            this[0xEA].action = new Func<Instruction, int>(cpu.NOP_Implicit);
            this[0xD8].action = new Func<Instruction, int>(cpu.CLD_Implicit);
            this[0xF8].action = new Func<Instruction, int>(cpu.SED_Implicit);
            this[0x78].action = new Func<Instruction, int>(cpu.SEI_Implicit);
            this[0x58].action = new Func<Instruction, int>(cpu.CLI_Implicit);
            this[0x00].action = new Func<Instruction, int>(cpu.BRK_Implicit);
            #endregion

            #region Load Instructions
            this[0xA9].action = new Func<Instruction, int>(cpu.LDA_Immediate);
            this[0xAD].action = new Func<Instruction, int>(cpu.LDA_Absolute);
            this[0xA5].action = new Func<Instruction, int>(cpu.LDA_ZPage);
            this[0xBD].action = new Func<Instruction, int>(cpu.LDA_AbsoluteX);
            this[0xB9].action = new Func<Instruction, int>(cpu.LDA_AbsoluteY);
            this[0xB5].action = new Func<Instruction, int>(cpu.LDA_ZPageX);
            this[0xB1].action = new Func<Instruction, int>(cpu.LDA_ZPageIndirectIndexedY);
            this[0xA1].action = new Func<Instruction, int>(cpu.LDA_ZPageIndexedIndirectX);            
            this[0xA2].action = new Func<Instruction, int>(cpu.LDX_Immediate);
            this[0xAE].action = new Func<Instruction, int>(cpu.LDX_Absolute);
            this[0xA6].action = new Func<Instruction, int>(cpu.LDX_ZPage);
            this[0xBE].action = new Func<Instruction, int>(cpu.LDX_AbsoluteY);
            this[0xB6].action = new Func<Instruction, int>(cpu.LDX_ZpageY);
            this[0xA0].action = new Func<Instruction, int>(cpu.LDY_Immediate);
            this[0xAC].action = new Func<Instruction, int>(cpu.LDY_Absolute);
            this[0xA4].action = new Func<Instruction, int>(cpu.LDY_ZPage);
            this[0xBC].action = new Func<Instruction, int>(cpu.LDY_AbsoluteX);
            this[0xB4].action = new Func<Instruction, int>(cpu.LDY_ZpageX);
            #endregion

            #region Transfer Instructions
            this[0xAA].action = new Func<Instruction, int>(cpu.TAX_Implicit);
            this[0x8A].action = new Func<Instruction, int>(cpu.TXA_Implicit);
            this[0xA8].action = new Func<Instruction, int>(cpu.TAY_Implicit);
            this[0x98].action = new Func<Instruction, int>(cpu.TYA_Implicit);
            this[0xBA].action = new Func<Instruction, int>(cpu.TSX_Implicit);
            this[0x9A].action = new Func<Instruction, int>(cpu.TXS_Implicit);
            #endregion

            #region Stack Pointer Instructions
            this[0x48].action = new Func<Instruction, int>(cpu.PHA_Implicit);
            this[0x68].action = new Func<Instruction, int>(cpu.PLA_Implicit);
            this[0x08].action = new Func<Instruction, int>(cpu.PHP_Implicit);
            this[0x28].action = new Func<Instruction, int>(cpu.PLP_Implicit);
            #endregion

            #region Store Instructions
            this[0x8D].action = new Func<Instruction, int>(cpu.STA_Absolute);
            this[0x85].action = new Func<Instruction, int>(cpu.STA_ZPage);
            this[0x9D].action = new Func<Instruction, int>(cpu.STA_AbsoluteX);
            this[0x99].action = new Func<Instruction, int>(cpu.STA_AbsoluteY);
            this[0x95].action = new Func<Instruction, int>(cpu.STA_ZPageIndexedX);
            this[0x91].action = new Func<Instruction, int>(cpu.STA_ZPageIndirectIndexedY);
            this[0x81].action = new Func<Instruction, int>(cpu.STA_ZPageIndexedIndirectX);
            this[0x8E].action = new Func<Instruction, int>(cpu.STX_Absolute);
            this[0x86].action = new Func<Instruction, int>(cpu.STX_ZPage);
            this[0x96].action = new Func<Instruction, int>(cpu.STX_ZPageIndexedY);
            this[0x8C].action = new Func<Instruction, int>(cpu.STY_Absolute);
            this[0x84].action = new Func<Instruction, int>(cpu.STY_ZPage);
            this[0x94].action = new Func<Instruction, int>(cpu.STY_ZPageIndexedX);
            #endregion

            #region Arithmetic Instructions
            this[0x38].action = new Func<Instruction, int>(cpu.SetCarryFlag);
            this[0x18].action = new Func<Instruction, int>(cpu.ClearCarryFlag);
            this[0xb8].action = new Func<Instruction, int>(cpu.ClearVFlag);

            this[0x69].action = new Func<Instruction, int>(cpu.ADC_Immediate);
            this[0x6D].action = new Func<Instruction, int>(cpu.ADC_Absolute);
            this[0x65].action = new Func<Instruction, int>(cpu.ADC_ZPage);
            this[0x7D].action = new Func<Instruction, int>(cpu.ADC_AbsoluteX);
            this[0x79].action = new Func<Instruction, int>(cpu.ADC_AbsoluteY);
            this[0x75].action = new Func<Instruction, int>(cpu.ADC_ZPageIndexedX);
            this[0x71].action = new Func<Instruction, int>(cpu.ADC_ZPageIndirectIndexedY);
            this[0x61].action = new Func<Instruction, int>(cpu.ADC_ZPageIndexedIndirectX);

            this[0xE9].action = new Func<Instruction, int>(cpu.SBC_Immediate);
            this[0xED].action = new Func<Instruction, int>(cpu.SBC_Absolute);
            this[0xE5].action = new Func<Instruction, int>(cpu.SBC_ZPage);
            this[0xFD].action = new Func<Instruction, int>(cpu.SBC_AbsoluteX);
            this[0xF9].action = new Func<Instruction, int>(cpu.SBC_AbsoluteY);
            this[0xF5].action = new Func<Instruction, int>(cpu.SBC_ZPageX);
            this[0xF1].action = new Func<Instruction, int>(cpu.SBC_ZPageIndirectIndexedY);
            this[0xE1].action = new Func<Instruction, int>(cpu.SBC_ZPageIndexedIndirectX);

            this[0xCA].action = new Func<Instruction, int>(cpu.DecrementX);
            this[0x88].action = new Func<Instruction, int>(cpu.DecrementY);
            this[0xE8].action = new Func<Instruction, int>(cpu.IncrementX);
            this[0xC8].action = new Func<Instruction, int>(cpu.IncrementY);
            this[0xEE].action = new Func<Instruction, int>(cpu.INC_Absolute);
            this[0xE6].action = new Func<Instruction, int>(cpu.INC_Zpage);
            this[0xFE].action = new Func<Instruction, int>(cpu.INC_AbsoluteX);
            this[0xF6].action = new Func<Instruction, int>(cpu.INC_ZPageX);
            this[0xCE].action = new Func<Instruction, int>(cpu.DEC_Absolute);
            this[0xC6].action = new Func<Instruction, int>(cpu.DEC_ZPage);
            this[0xDE].action = new Func<Instruction, int>(cpu.DEC_AbsoluteX);
            this[0xD6].action = new Func<Instruction, int>(cpu.DEC_ZpageX);

            this[0x29].action = new Func<Instruction, int>(cpu.AND_Immediate);
            this[0x2D].action = new Func<Instruction, int>(cpu.AND_Absolute);
            this[0x25].action = new Func<Instruction, int>(cpu.AND_ZPage);
            this[0x3D].action = new Func<Instruction, int>(cpu.AND_AbsoluteX);
            this[0x39].action = new Func<Instruction, int>(cpu.AND_AbsoluteY);
            this[0x35].action = new Func<Instruction, int>(cpu.AND_ZPageX);
            this[0x31].action = new Func<Instruction, int>(cpu.AND_ZPageIndirectIndexedY);
            this[0x21].action = new Func<Instruction, int>(cpu.AND_ZPageIndexedIndirectX);

            this[0x49].action = new Func<Instruction, int>(cpu.EOR_Immediate);
            this[0x4D].action = new Func<Instruction, int>(cpu.EOR_Absolute);
            this[0x45].action = new Func<Instruction, int>(cpu.EOR_ZPage);
            this[0x5D].action = new Func<Instruction, int>(cpu.EOR_AbsoluteX);
            this[0x59].action = new Func<Instruction, int>(cpu.EOR_AbsoluteY);
            this[0x55].action = new Func<Instruction, int>(cpu.EOR_ZPageX);
            this[0x51].action = new Func<Instruction, int>(cpu.EOR_ZPageIndirectIndexedY);
            this[0x41].action = new Func<Instruction, int>(cpu.EOR_ZPageIndexedIndirectX);

            this[0x09].action = new Func<Instruction, int>(cpu.ORA_Immediate);
            this[0x0D].action = new Func<Instruction, int>(cpu.ORA_Absolute);
            this[0x05].action = new Func<Instruction, int>(cpu.ORA_ZPage);
            this[0x1D].action = new Func<Instruction, int>(cpu.ORA_AbsoluteX);
            this[0x19].action = new Func<Instruction, int>(cpu.ORA_AbsoluteY);
            this[0x15].action = new Func<Instruction, int>(cpu.ORA_ZPageX);
            this[0x11].action = new Func<Instruction, int>(cpu.ORA_ZPageIndirectIndexedY);
            this[0x01].action = new Func<Instruction, int>(cpu.ORA_ZPageIndexedIndirectX);

            this[0xC9].action = new Func<Instruction, int>(cpu.CMP_Immediate);
            this[0xCD].action = new Func<Instruction, int>(cpu.CMP_Absolute);
            this[0xC5].action = new Func<Instruction, int>(cpu.CMP_ZPage);
            this[0xDD].action = new Func<Instruction, int>(cpu.CMP_AbsoluteX);
            this[0xD9].action = new Func<Instruction, int>(cpu.CMP_AbsoluteY);
            this[0xD5].action = new Func<Instruction, int>(cpu.CMP_ZPageX);
            this[0xD1].action = new Func<Instruction, int>(cpu.CMP_ZPageIndirectIndexedY);
            this[0xC1].action = new Func<Instruction, int>(cpu.CMP_ZPageIndexedIndirectX);
            this[0xE0].action = new Func<Instruction, int>(cpu.CPX_Immediate);
            this[0xEC].action = new Func<Instruction, int>(cpu.CPX_Absolute);
            this[0xE4].action = new Func<Instruction, int>(cpu.CPX_ZPage);
            this[0xC0].action = new Func<Instruction, int>(cpu.CPY_Immediate);
            this[0xCC].action = new Func<Instruction, int>(cpu.CPY_Absolute);
            this[0xC4].action = new Func<Instruction, int>(cpu.CPY_ZPage);

            #endregion

            #region Shift and Rotation Instructions
            this[0x2C].action = new Func<Instruction, int>(cpu.BIT_Absolute);
            this[0x24].action = new Func<Instruction, int>(cpu.BIT_ZPage);

            this[0x0E].action = new Func<Instruction, int>(cpu.ASL_Absolute);
            this[0x06].action = new Func<Instruction, int>(cpu.ASL_ZPage);
            this[0x0A].action = new Func<Instruction, int>(cpu.ASL_Accumulator);
            this[0x1E].action = new Func<Instruction, int>(cpu.ASL_AbsoluteX);
            this[0x16].action = new Func<Instruction, int>(cpu.ASL_ZPageX);

            this[0x4E].action = new Func<Instruction, int>(cpu.LSR_Absolute);
            this[0x46].action = new Func<Instruction, int>(cpu.LSR_ZPage);
            this[0x4A].action = new Func<Instruction, int>(cpu.LSR_Accumulator);
            this[0x5E].action = new Func<Instruction, int>(cpu.LSR_AbsoluteX);
            this[0x56].action = new Func<Instruction, int>(cpu.LSR_ZPageX);

            this[0x2E].action = new Func<Instruction, int>(cpu.ROL_Absolute);
            this[0x26].action = new Func<Instruction, int>(cpu.ROL_ZPage);
            this[0x2A].action = new Func<Instruction, int>(cpu.ROL_Accumulator);
            this[0x3E].action = new Func<Instruction, int>(cpu.ROL_AbsoluteX);
            this[0x36].action = new Func<Instruction, int>(cpu.ROL_ZPageX);

            this[0x6E].action = new Func<Instruction, int>(cpu.ROR_Absolute);
            this[0x66].action = new Func<Instruction, int>(cpu.ROR_ZPage);
            this[0x6A].action = new Func<Instruction, int>(cpu.ROR_Accumulator);
            this[0x7E].action = new Func<Instruction, int>(cpu.ROR_AbsoluteX);
            this[0x76].action = new Func<Instruction, int>(cpu.ROR_ZPageX);
            #endregion

            #region Jump and Branch Instruction
            this[0x20].action = new Func<Instruction, int>(cpu.JSR_Absolute);
            this[0x40].action = new Func<Instruction, int>(cpu.RTI_Implicit);
            this[0x60].action = new Func<Instruction, int>(cpu.RTS_Implicit);

            this[0x4C].action = new Func<Instruction, int>(cpu.JMP_Absolute);
            this[0x6C].action = new Func<Instruction, int>(cpu.JMP_IndirectAbsolute);

            this[0xB0].action = new Func<Instruction, int>(cpu.BCS_Relative);
            this[0x90].action = new Func<Instruction, int>(cpu.BCC_Relative);
            this[0xF0].action = new Func<Instruction, int>(cpu.BEQ_Relative);
            this[0xD0].action = new Func<Instruction, int>(cpu.BNE_Relative);
            this[0x30].action = new Func<Instruction, int>(cpu.BMI_Relative);
            this[0x10].action = new Func<Instruction, int>(cpu.BPL_Relative);
            this[0x70].action = new Func<Instruction, int>(cpu.BVS_Relative);
            this[0x50].action = new Func<Instruction, int>(cpu.BVC_Relative);
            #endregion            
        }
    }

    public class Instruction
    {
        public Instruction(byte opcode, string mnemonic, ushort length, int tCycle, int aCycle)
        {
            this.opcode = opcode;
            this.mnemonic = mnemonic;
            this.length = length;
            this.tCycles = tCycle;
            this.aCycle = aCycle;
        }

        /// <summary>
        /// Hexadecimal Opcode
        /// </summary>
        public byte opcode
        {
            get;
            private set;
        }

        /// <summary>
        /// Mnemonic code
        /// </summary>
        public string mnemonic
        {
            get;
            private set;
        }

        /// <summary>
        /// Instruction Length
        /// </summary>
        public ushort length
        {
            get;
            private set;
        }

        //Duration of instruuction in cycles
        public int tCycles
        {
            get;
            private set;
        }

        /// <summary>
        /// Alternative duration in cycle
        /// </summary>
        public int aCycle
        {
            get;
            private set;
        }

        public Func<Instruction, int> action
        {
            get;
            set;
        }        
    }
}
