using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Core.Framework.Network.Data
{
    /// <summary>
    /// Author: chengdundeng
    /// Date: 2019/11/29
    /// Desc: 每個Message的標頭, 記錄著封包編號，封包長度, 遠端伺服器類型
    /// Desc: carlhsieh: 重新改寫為記憶體映射模式
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    public struct Header
    {
        public const ushort HeaderSize = 16;

        [FieldOffset(0)]
        private int _messageId;
        [FieldOffset(4)]
        private int _contentSize;
        [FieldOffset(8)]
        private long _time;

        [FieldOffset(0)] private byte _byte0;
        [FieldOffset(1)] private byte _byte1;
        [FieldOffset(2)] private byte _byte2;
        [FieldOffset(3)] private byte _byte3;
        [FieldOffset(4)] private byte _byte4;
        [FieldOffset(5)] private byte _byte5;
        [FieldOffset(6)] private byte _byte6;
        [FieldOffset(7)] private byte _byte7;
        [FieldOffset(8)] private byte _byte8;
        [FieldOffset(9)] private byte _byte9;
        [FieldOffset(10)] private byte _byte10;
        [FieldOffset(11)] private byte _byte11;
        [FieldOffset(12)] private byte _byte12;
        [FieldOffset(13)] private byte _byte13;
        [FieldOffset(14)] private byte _byte14;
        [FieldOffset(15)] private byte _byte15;

        /// <summary>
        ///     header衍生資料，表示後面資料量
        /// </summary>
        public int ContentSize
        {
            get { return _contentSize; } 
        }

        /// <summary>
        ///     header資料，表示 header+content總共多少資料量
        /// </summary>
        public int TotalSize
        {
            get { return _contentSize + HeaderSize; } 
        }

        /// <summary>
        ///     header資料 表示本次Message的ID
        /// </summary>
        public int MessageId
        {
            get { return _messageId; }
        }

        public Header(int messageId, int contentSize = 0) : this()
        {
            _messageId = messageId;
            _contentSize = contentSize;
        }

        public int Serialize(byte[] buffer, int offset)
        {
            if (offset + HeaderSize < buffer.Length)
            {
                buffer[offset++] = _byte0;
                buffer[offset++] = _byte1;
                buffer[offset++] = _byte2;
                buffer[offset++] = _byte3;
                buffer[offset++] = _byte4;
                buffer[offset++] = _byte5;
                buffer[offset++] = _byte6;
                buffer[offset++] = _byte7;
                buffer[offset++] = _byte8;
                buffer[offset++] = _byte9;
                buffer[offset++] = _byte10;
                buffer[offset++] = _byte11;
                buffer[offset++] = _byte12;
                buffer[offset++] = _byte13;
                buffer[offset++] = _byte14;
                buffer[offset++] = _byte15;
                return HeaderSize;
            }
            else
            {
                return 0;
            }
        }

        public int Serialize(MemoryStream ms)
        {
            ms.WriteByte(_byte0);
            ms.WriteByte(_byte1);
            ms.WriteByte(_byte2);
            ms.WriteByte(_byte3);
            ms.WriteByte(_byte4);
            ms.WriteByte(_byte5);
            ms.WriteByte(_byte6);
            ms.WriteByte(_byte7);
            ms.WriteByte(_byte8);
            ms.WriteByte(_byte9);
            ms.WriteByte(_byte10);
            ms.WriteByte(_byte11);
            ms.WriteByte(_byte12);
            ms.WriteByte(_byte13);
            ms.WriteByte(_byte14);
            ms.WriteByte(_byte15);
            return HeaderSize;
        }

        public void Deserialize(byte[] data)
        {
            if (data.Length < HeaderSize)
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
            _byte8 = data[offset++];
            _byte9 = data[offset++];
            _byte10 = data[offset++];
            _byte11 = data[offset++];
            _byte12 = data[offset++];
            _byte13 = data[offset++];
            _byte14 = data[offset++];
            _byte15 = data[offset++];
        }

        public void Deserialize(MemoryStream ms)
        {
            _byte0 = (byte)ms.ReadByte();
            _byte1 = (byte)ms.ReadByte();
            _byte2 = (byte)ms.ReadByte();
            _byte3 = (byte)ms.ReadByte();
            _byte4 = (byte)ms.ReadByte();
            _byte5 = (byte)ms.ReadByte();
            _byte6 = (byte)ms.ReadByte();
            _byte7 = (byte)ms.ReadByte();
            _byte8 = (byte)ms.ReadByte();
            _byte9 = (byte)ms.ReadByte();
            _byte10 = (byte)ms.ReadByte();
            _byte11 = (byte)ms.ReadByte();
            _byte12 = (byte)ms.ReadByte();
            _byte13 = (byte)ms.ReadByte();
            _byte14 = (byte)ms.ReadByte();
            _byte15 = (byte)ms.ReadByte();
        }
    }
}