using Core.Framework.Network.Data;

namespace Core.Framework.Network.Buffers
{
    /// <summary>
    /// Author: chengdundeng
    /// Date: 2019/11/29
    /// Desc: Content Buffer
    /// </summary>
    public class ContentBuffer : ReceiveBuffer
    {
        /// <summary>
        ///     本次Message的Header
        /// </summary>
        public Header Header { get; set; }

        /// <summary>
        ///     buffer是否已經收到足夠資料
        /// </summary>
        public bool IsEnough
        {
            get { return ReceivedSize == Header.ContentSize; }
        }

        /// <summary>
        ///     還沒收到的資料量
        /// </summary>
        public int UnreceivedContentSize
        {
            get { return Header.ContentSize - ReceivedSize; }
        }

        public ContentBuffer(int bufferSize)
            : base(bufferSize)
        {
        }
    }
}