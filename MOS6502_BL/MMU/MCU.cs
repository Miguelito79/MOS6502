using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MOS6502_BL.MMU
{
    public class MCU
    {
        public byte[][] buffer;

        public MCU()
        {
            buffer = new byte[1][];
            buffer[0] = new byte[64 * 1024];
        }

        public byte ReadByte(ushort address)
        {
            return buffer[0][address];
        }

        public void WriteByte(ushort address, byte data)
        {
            buffer[0][address] = data;
        }

        public ushort ReadWord(ushort address)
        {
            byte lowOrder = buffer[0][address];
            byte highOrder = buffer[0][address + 1];

            return (ushort)((highOrder << 8) | lowOrder);
        }

        public bool LoadFileInMemoryAt(string path, long offset)
        {
            bool response = true;

            try
            {
                long startingAddress = offset;
                using (FileStream stream = new FileStream(path, FileMode.Open))
                {
                    int numBytesToRead = (int)stream.Length;
                    if (numBytesToRead > 0)
                    {
                        do
                        {
                            byte value = (byte)stream.ReadByte();
                            buffer[0][startingAddress] = value;

                            numBytesToRead -= 1;
                            startingAddress += 1;
                        } while (numBytesToRead > 0);
                    }
                }
            }
            catch
            {
                response = false;
            }

            return response;
        }
    }
}
