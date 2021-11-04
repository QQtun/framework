using Core.Framework.Network.Data;
using System;
using System.IO;

namespace Core.Framework.Network.Buffers
{
    /// <summary>
    /// Author: chengdundeng
    /// Date: 2019/11/29
    /// Desc: Send Buffer
    /// </summary>
    public class SendBuffer : BufferBase
    {
        /// <summary>
        ///     資料長度
        /// </summary>
        public int DataSize { get; private set; }

        /// <summary>
        ///     已經發送出的長度
        /// </summary>
        public int SentSize { get; private set; }

        /// <summary>
        ///     尚未發送的長度
        /// </summary>
        public int UnsendSize
        {
            get { return DataSize - SentSize; }
        }

        // 是否已經都送出去
        public bool IsAllSent
        {
            get { return SentSize == DataSize; }
        }

        public SendBuffer(int bufferSize)
            : base(bufferSize)
        {
            DataSize = 0;
            SentSize = 0;
        }

        public bool WriteToBuffer(Message message)
        {
            if (DataSize + message.Header.TotalSize > BufferSize)
            {
                // message 寫不進 buffer
                return false;
            }

            // 用stream 方法
            MemoryStream ms = GetStream();

            long begin = ms.Position;

            // header
            message.Header.Serialize(ms);

            // content
            message.Serialize(ms);

            // footer
            long now = ms.Position;
            var footer = Footer.Create(Buffer, (int)begin, (int)(now - begin));
            message.Footer = footer;
            footer.Serialize(ms);

            ms.Flush();

            long end = ms.Position;
            DataSize = (int)end;
#if !NDEBUG
            message.OnWriteBuffer?.Invoke(new ArraySegment<byte>(Buffer, (int)begin, (int)(end - begin)));
#endif
            return true;
        }

        public void IncreaseSentSize(int sentSize)
        {
            SentSize += sentSize;

            if (SentSize > DataSize)
            {
                throw new Exception("SentSize > DataSize");
            }
        }

        public void Reset()
        {
            DataSize = 0;
            SentSize = 0;
            MemoryStream ms = GetStream();
            ms.Seek(0, SeekOrigin.Begin);
            ms.SetLength(0);
        }
    }
}