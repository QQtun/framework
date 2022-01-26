using Core.Framework.Network.Buffers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace Core.Framework.Network.Data
{
    [StructLayout(LayoutKind.Explicit)]
    public struct Footer
    {
        public const int FooterSize = 8;

        [FieldOffset(0)]
        private long _checkSum;

        [FieldOffset(0)] private byte _byte0;
        [FieldOffset(1)] private byte _byte1;
        [FieldOffset(2)] private byte _byte2;
        [FieldOffset(3)] private byte _byte3;
        [FieldOffset(4)] private byte _byte4;
        [FieldOffset(5)] private byte _byte5;
        [FieldOffset(6)] private byte _byte6;
        [FieldOffset(7)] private byte _byte7;

        public long CheckSum => _checkSum;

        public Footer(int checkSum) : this()
        {
            _checkSum = checkSum;
        }


        public int Serialize(byte[] buffer, int offset)
        {
            if (offset + FooterSize < buffer.Length)
            {
                buffer[offset++] = _byte0;
                buffer[offset++] = _byte1;
                buffer[offset++] = _byte2;
                buffer[offset++] = _byte3;
                buffer[offset++] = _byte4;
                buffer[offset++] = _byte5;
                buffer[offset++] = _byte6;
                buffer[offset++] = _byte7;
                return FooterSize;
            }
            else
            {
                return 0;
            }
        }

        public int Serialize(Stream ms)
        {
            ms.WriteByte(_byte0);
            ms.WriteByte(_byte1);
            ms.WriteByte(_byte2);
            ms.WriteByte(_byte3);
            ms.WriteByte(_byte4);
            ms.WriteByte(_byte5);
            ms.WriteByte(_byte6);
            ms.WriteByte(_byte7);
            return FooterSize;
        }

        public void Deserialize(byte[] data)
        {
            if (data.Length < FooterSize)
            {
                throw new Exception("data lenght not enough!!!");
            }

            int offset = 0;
            _byte0 = data[offset++];
            _byte1 = data[offset++];
            _byte2 = data[offset++];
            _byte3 = data[offset++];
            _byte4 = data[offset++];
            _byte5 = data[offset++];
            _byte6 = data[offset++];
            _byte7 = data[offset++];
        }

        public void Deserialize(Stream ms)
        {
            _byte0 = (byte)ms.ReadByte();
            _byte1 = (byte)ms.ReadByte();
            _byte2 = (byte)ms.ReadByte();
            _byte3 = (byte)ms.ReadByte();
            _byte4 = (byte)ms.ReadByte();
            _byte5 = (byte)ms.ReadByte();
            _byte6 = (byte)ms.ReadByte();
            _byte7 = (byte)ms.ReadByte();
        }

        public static Footer Create(byte[] inputData, int start, int len)
        {
            uint sum = 0;
            uint zeroOffset = 0x30; // ASCII '0'

            for (int i = start; i < start + len; i++)
            {
                int product = inputData[i] & 0x7F; // Take the low 7 bits from the record.
                product *= i + 1; // Multiply by the 1 based position.
                sum += (uint)product; // Add the product to the running sum.
            }

            byte[] result = new byte[8];
            for (int i = 0; i < 8; i++) // if the checksum is reversed, make this:
                                        // for (int i = 7; i >=0; i--) 
            {
                uint current = (uint)(sum & 0x0f); // take the lowest 4 bits.
                current += zeroOffset; // Add '0'
                result[i] = (byte)current;
                sum = sum >> 4; // Right shift the bottom 4 bits off.
            }

            int offset = 0;
            var ret = new Footer();
            ret._byte0 = result[offset++];
            ret._byte1 = result[offset++];
            ret._byte2 = result[offset++];
            ret._byte3 = result[offset++];
            ret._byte4 = result[offset++];
            ret._byte5 = result[offset++];
            ret._byte6 = result[offset++];
            ret._byte7 = result[offset++];
            return ret;
        }

        public static Footer Create(List<BufferBase> buffers)
        {
            uint sum = 0;
            uint zeroOffset = 0x30; // ASCII '0'

            for (int bi = 0; bi < buffers.Count; bi++)
            {
                var buffer = buffers[bi];
                for (int i = 0; i < buffer.DataSize; i++)
                {
                    int product = buffer.Buffer[i] & 0x7F; // Take the low 7 bits from the record.
                    product *= i + 1; // Multiply by the 1 based position.
                    sum += (uint)product; // Add the product to the running sum.
                }
            }

            byte[] result = new byte[8];
            for (int i = 0; i < 8; i++) // if the checksum is reversed, make this:
                                        // for (int i = 7; i >=0; i--) 
            {
                uint current = (uint)(sum & 0x0f); // take the lowest 4 bits.
                current += zeroOffset; // Add '0'
                result[i] = (byte)current;
                sum = sum >> 4; // Right shift the bottom 4 bits off.
            }

            int offset = 0;
            var ret = new Footer();
            ret._byte0 = result[offset++];
            ret._byte1 = result[offset++];
            ret._byte2 = result[offset++];
            ret._byte3 = result[offset++];
            ret._byte4 = result[offset++];
            ret._byte5 = result[offset++];
            ret._byte6 = result[offset++];
            ret._byte7 = result[offset++];
            return ret;
        }
    }
}
