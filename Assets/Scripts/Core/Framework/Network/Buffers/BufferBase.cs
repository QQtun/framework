using System;
using System.IO;

namespace Core.Framework.Network.Buffers
{
    /// <summary>
    /// Author: chengdundeng
    /// Date: 2019/11/29
    /// Desc: Buffer Base
    /// </summary>
    public class BufferBase
    {
        public int BufferSize => Buffer.Length;

        public byte[] Buffer { get; private set; }

        public int DataSize { get; set; }

        public int ReadPosition { get; set; }

        public int WritePosition { get; set; }

        public int RamainingReadSize => DataSize - ReadPosition;

        public int RemainingWriteSize => BufferSize - WritePosition;

        public BufferBase(int bufferSize)
        {
            Buffer = new byte[bufferSize];
        }

        public int Write(byte[] buffer, int offset, int len)
        {
            var writeSize = Math.Min(RemainingWriteSize, len);
            if (writeSize == 0)
                return writeSize;

            Array.Copy(buffer, offset, Buffer, WritePosition, writeSize);
            WritePosition += writeSize;
            DataSize += writeSize;
            return writeSize;
        }

        public int Read(byte[] buffer, int offset, int len)
        {
            var readSize = Math.Min(RamainingReadSize, len);
            if (readSize == 0)
                return readSize;

            Array.Copy(Buffer, ReadPosition, buffer, offset, readSize);
            ReadPosition += readSize;
            return readSize;
        }

        public void Reset()
        {
            DataSize = 0;
            WritePosition = 0;
            ReadPosition = 0;
        }
    }
}