using System.IO;

namespace Core.Framework.Network.Buffers
{
    /// <summary>
    /// Author: chengdundeng
    /// Date: 2019/11/29
    /// Desc: Receive Buffer
    /// </summary>
    public class ReceiveBuffer : BufferBase
    {
        /// <summary>
        ///     buffer已經收了多少可用資料
        /// </summary>
        public int ReceivedSize { get; private set; }

        /// <summary>
        ///     開始讀取的位置
        /// </summary>
        public int ReadStartPosition { get; private set; }

        /// <summary>
        ///     開始接收的位置
        /// </summary>
        public int WriteStartPosition { get; private set; }

        /// <summary>
        ///     buffer還剩多少空間可以用
        /// </summary>
        public int RemainingBufferSize
        {
            get { return BufferSize - WriteStartPosition; }
        }

        public System.ArraySegment<byte> GetWriteSegment()
        {
            return new System.ArraySegment<byte>(Buffer, WriteStartPosition, BufferSize - WriteStartPosition);
        }

        public ReceiveBuffer(int bufferSize) : base(bufferSize)
        {
            ReceivedSize = 0;
            WriteStartPosition = 0;
            ReadStartPosition = 0;
        }

        public void IncreaseReceivedSize(int receiveSize)
        {
            ReceivedSize += receiveSize;
            WriteStartPosition += receiveSize;

            if (WriteStartPosition >= BufferSize)
            {
                WriteStartPosition %= BufferSize;
            }
        }

        public void FreeReadBlock(int blockSize)
        {
            ReadStartPosition += blockSize;
            ReceivedSize -= blockSize;

            if (ReadStartPosition >= BufferSize)
            {
                ReadStartPosition = ReadStartPosition % BufferSize;
            }
        }

        public virtual void Reset()
        {
            ReceivedSize = 0;
            WriteStartPosition = 0;
            ReadStartPosition = 0;

            MemoryStream ms = GetStream();
            ms.Seek(0, SeekOrigin.Begin);
            ms.SetLength(BufferSize);
        }
    }
}