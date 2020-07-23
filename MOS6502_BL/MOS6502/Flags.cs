using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MOS6502_BL.MOS6502
{
    public class Flags
    {
        private byte _statusRegister;
        public byte Register
        {
            get { return _statusRegister; }
            set { _statusRegister = (byte)( (value & ~(1 << 5)) | 0x20); }
        }

        public Flags(byte initValue)
        {
            _statusRegister = initValue;
        }

        //BIT 0 - Carry flag
        public int CFlag
        {
            get { return _statusRegister & 0x01; }
            set { _statusRegister = (byte)((_statusRegister & ~(1)) | value); }
        }

        //BIT 1 - Zero flag
        public int ZFlag
        {
            get { return (_statusRegister & 0x02) >> 1; }
            set { _statusRegister = (byte)((_statusRegister & ~(1 << 1)) | (value << 1)); }
        }

        //BIT 2 - IRQ Disabled
        public int IRQDisabled
        {
            get { return (_statusRegister & 0x04) >> 2; }
            set { _statusRegister = (byte)((_statusRegister & ~(1 << 2)) | (value << 2)); }
        }

        //BIT 3 - DecimalMode
        public int DFlag
        {
            get { return (_statusRegister & 0x08) >> 3; }
            set { _statusRegister = (byte)((_statusRegister & ~(1 << 3)) | (value << 3)); }
        }

        //BIT 4 - BreakCommand (Manually interrupt issued by BRK instruction)
        public int BreakCommand
        {
            get { return (_statusRegister & 0x10) >> 4; }
            set { _statusRegister = (byte)((_statusRegister & ~(1 << 4)) | (value << 4)); }
        }

        //BIT 5 - Unused ALWAYS 1

        //BIT 6 - Overflow
        public int VFlag
        {
            get { return (_statusRegister & 0x40) >> 6; }
            set { _statusRegister = (byte)((_statusRegister & ~(1 << 6)) | (value << 6)); }
        }

        //BIT 7 - Negative Flag
        public int NFlag
        {
            get { return (_statusRegister & 0x80) >> 7; }
            set { _statusRegister = (byte)((_statusRegister & ~(1 << 7)) | (value << 7)); }
        }

        public void ComputeCFlagFromADC(ushort data)
        {
            if (DFlag == 1)
                //Operting in BCD 
                CFlag = data > 0x9F ? 1 : 0;
            else
                //Operating in binary mode
                CFlag = (data > 0xFF) ? 1 : 0;
        }

        public void ComputeCFlagFromSBC(ushort data)
        {
            CFlag = data > 0xFF ? 1 : 0;
        }

        public void ComputeZFlag(ushort data)
        {            
            ZFlag = (data & 0xFF) == 0 ? 1 : 0;
        }

        public void ComputeNFlag(ushort data)
        {
            NFlag = (data & 0x80) >> 7 == 1 ? 1 : 0;
        }

        public void ComputeVFlag(byte firstOperand, byte secondOperand, ushort result)
        {
            VFlag = (((firstOperand ^ result) & (secondOperand ^ result)) & 0x80) != 0 ? 1 : 0;
        }
    }
}
