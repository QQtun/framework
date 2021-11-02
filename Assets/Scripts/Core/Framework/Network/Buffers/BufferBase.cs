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
        private MemoryStream _memoryStream;

        public int BufferSize { get; private set; }

        public byte[] Buffer { get; private set; }

        public BufferBase(int bufferSize)
        {
            BufferSize = bufferSize;
            Buffer = new byte[bufferSize];
            _memoryStream = new MemoryStream(Buffer);
            _memoryStream.SetLength(bufferSize);
        }

        public MemoryStream GetStream()
        {
            return _memoryStream;
        }
    }
}