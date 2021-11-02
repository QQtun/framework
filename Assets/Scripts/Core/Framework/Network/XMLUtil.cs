using System;
using System.Text;

namespace Core.Framework.Network
{
    public class XMLUtil
    {
        public const int MaxPacketSize = 131072;
        public const long Before1970Ticks = 621356256000000000L;

        private const string Key2 = "ag";

        private static readonly char[] NetWorkKeyByte =
{
            'd',
            '0',
            'g',
            'x',
            '#',
            '3',
            '1',
            '%',
            'm',
            'k',
            '1',
            '2',
        };

        private static string sNetWorkKey = "";
        private static byte sKey;

        private static byte[] sCRCBuffer = new byte[MaxPacketSize];

        static XMLUtil()
        {
            byte[] bytes = BitConverter.GetBytes(1695843216);
            for (int i = 0; i < bytes.Length; i++)
            {
                sKey += bytes[i];
            }
        }
        public static string NetWorkKey
        {
            get
            {
                if (sNetWorkKey.Length == 0)
                {
                    byte[] bt = new byte[NetWorkKeyByte.Length - 1];
                    for (int i = 1; i < NetWorkKeyByte.Length; i++)
                    {
                        bt[i - 1] = (byte)NetWorkKeyByte[i];
                    }
                    sNetWorkKey = System.Text.Encoding.Default.GetString(bt) + Key2;
                }
                return sNetWorkKey;
            }
        }

        public static void SortBytes(byte[] bytesData, int offsetTo, int count)
        {
            byte b = sKey;
            int num = offsetTo;
            while (num < offsetTo + count)
            {
                do
                {
                    bytesData[num] = (byte)(bytesData[num] ^ b);
                    num++;
                }
                while ((b | 0xFF) == 0);
            }
        }

        public static string NetWorkDecrypt(string input, bool bDecrypt = true)
        {
            if (string.IsNullOrEmpty(input)) return "";
            if (bDecrypt)
            {
                byte[] b = Convert.FromBase64String(input);
                RC4Helper.RC4(b, NetWorkKey);
                return Encoding.UTF8.GetString(b);
            }
            return input;
        }

        public static string NetWorkEncrypt(string input, bool bEncorypt = true)
        {
            try
            {
                if (string.IsNullOrEmpty(input)) return "";
                if (bEncorypt)
                {
                    byte[] b = System.Text.Encoding.UTF8.GetBytes(input);
                    RC4Helper.RC4(b, NetWorkKey);
                    return Convert.ToBase64String(b);
                }
            }
            catch (Exception e)
            {
                LogUtil.Debug.LogError(e);
            }
            return input;
        }

        public static bool CalculateCRC(short msgId, byte[] buffer, int offset, int count, out byte[] result)
        {
            var ret = CalculateCRC(msgId, buffer, offset, count, out ArraySegment<byte> outBuffer);
            result = new byte[outBuffer.Count];
            Array.Copy(outBuffer.Array, outBuffer.Offset, result, 0, outBuffer.Count);
            return ret;
        }

        public static bool CalculateCRC(short msgId, byte[] buffer, int offset, int count, out ArraySegment<byte> result)
        {
            if (count + 11 > sCRCBuffer.Length)
            {
                result = new ArraySegment<byte>();
                return false;
            }

            Array.Clear(sCRCBuffer, 0, count + 11);
            Array.Copy(buffer, offset, sCRCBuffer, 11, count);
            result = new ArraySegment<byte>(sCRCBuffer, 0, count + 11);

            ushort num = 1;
            int num2 = 1;
            int num3 = 1;
            uint num4 = 1u;
            uint num5 = 1u;
            int num6 = 1;
            int value = count + 2 + 1 + 4;
            while (true)
            {
                Array.Copy(BitConverter.GetBytes(value), 0, sCRCBuffer, 0, 4);
                num = (ushort)msgId;
                if ((uint)((int)num4 - num2) > uint.MaxValue)
                {
                    goto IL_00f1;
                }
                goto IL_0100;
            IL_00f1:
                num = (ushort)(num + SessionData.CmdOffset);
                if ((uint)(num2 + num3) <= uint.MaxValue)
                {
                    goto IL_00df;
                }
                goto IL_0100;
            IL_00df:
                if ((uint)((int)num5 + num6) >= 0u)
                {
                    if ((uint)num6 < 0u)
                    {
                        continue;
                    }
                    goto IL_0021;
                }
                goto IL_0100;
            IL_0021:
                Array.Copy(BitConverter.GetBytes(num), 0, sCRCBuffer, 4, 2);
                DateTime now = DateTime.Now;
                while (true)
                {
                    num2 = (int)((now.Ticks - Before1970Ticks) / 10000000);
                    byte[] bytes = BitConverter.GetBytes(num2);
                    CRC32 cRC = new CRC32();
                    if ((uint)num2 < 0u)
                    {
                        break;
                    }
                    while (true)
                    {
                        if ((uint)((int)num4 - num3) <= uint.MaxValue)
                        {
                            cRC.Update(bytes);
                            if ((uint)(num2 + num) < 0u)
                            {
                                break;
                            }
                            num3 = 11;
                            cRC.Update(sCRCBuffer, num3, count);
                        }
                        num4 = cRC.GetValue() % 255u;
                        num5 = (uint)(msgId % 255);
                        num6 = (int)(num4 ^ num5);
                        Array.Copy(BitConverter.GetBytes((byte)num6), 0, sCRCBuffer, 6, 1);
                        Array.Copy(bytes, 0, sCRCBuffer, 7, 4);
                        if (num5 >= 0)
                        {
                            return true; ;
                        }
                    }
                }
                goto IL_00df;
            IL_0100:
                if (msgId <= 100)
                {
                    goto IL_0021;
                }
                goto IL_00f1;
            }
        }

        public class RC4Helper
        {
            private static byte[] s = new byte[256];

            private static byte[] k = new byte[256];

            public static void RC4(byte[] bytesData, int offset, int count, string key)
            {
                byte[] bytes = System.Text.Encoding.UTF8.GetBytes(key);
                RC4(bytesData, offset, count, bytes);
            }

            public static void RC4(byte[] bytesData, int offset, int count, byte[] key)
            {
                lock (s)
                {
                    int i;
                    for (i = 0; i < 256; i++)
                    {
                        s[i] = (byte)i;
                        k[i] = key[i % key.GetLength(0)];
                    }
                    int num = 0;
                    for (i = 0; i < 256; i++)
                    {
                        num = (num + s[i] + k[i]) % 256;
                        byte b = s[i];
                        s[i] = s[num];
                        s[num] = b;
                    }
                    i = (num = 0);
                    for (int j = offset; j < offset + count; j++)
                    {
                        i = (i + 1) % 256;
                        num = (num + s[i]) % 256;
                        byte b = s[i];
                        s[i] = s[num];
                        s[num] = b;
                        int num2 = (s[i] + s[num]) % 256;
                        bytesData[j] ^= s[num2];
                    }
                }
            }

            public static void RC4(byte[] bytesData, byte[] key)
            {
                RC4(bytesData, 0, bytesData.Length, key);
            }

            public static void RC4(byte[] bytesData, string key)
            {
                byte[] bytes = System.Text.Encoding.UTF8.GetBytes(key);
                RC4(bytesData, bytes);
            }
        }

        public class SessionData
        {
            public const ushort CmdOffset = 0;

            public const short GenerateKeyCmdId = 24;

            public const short MinOffsetCmdId = 100;

            public const short OffsetChgCmdId = 21;
        }

        public class CRC32
        {
            private uint mCrc;

            private static uint[] sCrcTable = MakeCrcTable();

            private static uint[] MakeCrcTable()
            {
                uint[] array = new uint[256];
                for (int i = 0; i < 256; i++)
                {
                    uint num = (uint)i;
                    int num2 = 8;
                    while (--num2 >= 0)
                    {
                        num = (((num & 1) == 0) ? (num >> 1) : (0xEDB88320u ^ (num >> 1)));
                    }
                    array[i] = num;
                }
                return array;
            }

            public uint GetValue()
            {
                return mCrc & 0xFFFFFFFFu;
            }

            public void Reset()
            {
                mCrc = 0u;
            }

            public void Update(byte[] buf)
            {
                uint num = 0u;
                int num2 = buf.Length;
                uint num3 = ~mCrc;
                while (--num2 >= 0)
                {
                    num3 = sCrcTable[(num3 ^ buf[num++]) & 0xFF] ^ (num3 >> 8);
                }
                mCrc = ~num3;
            }

            public void Update(byte[] buf, int off, int len)
            {
                uint num = ~mCrc;
                while (--len >= 0)
                {
                    num = sCrcTable[(num ^ buf[off++]) & 0xFF] ^ (num >> 8);
                }
                mCrc = ~num;
            }
        }
    }
}