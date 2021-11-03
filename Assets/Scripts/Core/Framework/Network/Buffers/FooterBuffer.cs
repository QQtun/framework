using Core.Framework.Network.Data;
using System;
using System.IO;

namespace Core.Framework.Network.Buffers
{
    /// <summary>
    /// Author: chengdundeng
    /// Date: 2019/11/29
    /// Desc: Header Buffer
    /// </summary>
    public class FooterBuffer : ReceiveBuffer
    {
        /// <summary>
        ///     buffer還剩多少空間可以用
        /// </summary>
        public int UnreceiveSize
        {
            get { return Footer.FooterSize - ReceivedSize; }
        }

        /// <summary>
        ///     buffer是否已經收滿資料
        /// </summary>
        public bool IsEnough
        {
            get { return ReceivedSize == Footer.FooterSize; }
        }

        public FooterBuffer()
            : base(Footer.FooterSize)
        {
        }

        public Footer CreateFooter()
        {
            if (!IsEnough)
            {
                throw new Exception("data is not enough!");
            }

            MemoryStream ms = GetStream();
            ms.Seek(0, SeekOrigin.Begin);
            ms.SetLength(ReceivedSize);

            Footer footer = new Footer();
            footer.Deserialize(ms);
            return footer;
        }
    }
}