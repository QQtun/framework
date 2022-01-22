using System.Collections.Generic;

namespace Core.Framework.Network.Buffers
{
    public class ContentBufferPool
    {
        public static readonly ContentBufferPool Default = new ContentBufferPool(256);

        private Queue<ContentBuffer> _pool = new Queue<ContentBuffer>();

        public int BufferSize { get; }

        public ContentBufferPool(int size)
        {
            BufferSize = size;
        }

        public ContentBuffer Alloc()
        {
            lock(_pool)
            {
                if (_pool.Count > 0)
                    return _pool.Dequeue();
                return new ContentBuffer(BufferSize);
            }
        }

        public void Dealloc(ContentBuffer buffer)
        {
            buffer.Reset();
            lock (_pool)
            {
                _pool.Enqueue(buffer);
            }
        }
    }

    public class HeaderBufferPool
    {
        public static readonly HeaderBufferPool Default = new HeaderBufferPool();

        private Queue<HeaderBuffer> _pool = new Queue<HeaderBuffer>();

        //public int BufferSize { get; }

        public HeaderBufferPool()
        {
        }

        public HeaderBuffer Alloc()
        {
            lock (_pool)
            {
                if (_pool.Count > 0)
                    return _pool.Dequeue();
                return new HeaderBuffer();
            }
        }

        public void Dealloc(HeaderBuffer buffer)
        {
            buffer.Reset();
            lock(_pool)
            {
                _pool.Enqueue(buffer);
            }
        }
    }

    public class FooterBufferPool
    {
        public static readonly FooterBufferPool Default = new FooterBufferPool();

        private Queue<FooterBuffer> _pool = new Queue<FooterBuffer>();

        //public int BufferSize { get; }

        public FooterBufferPool()
        {
        }

        public FooterBuffer Alloc()
        {
            lock (_pool)
            {
                if (_pool.Count > 0)
                    return _pool.Dequeue();
                return new FooterBuffer();
            }
        }

        public void Dealloc(FooterBuffer buffer)
        {
            buffer.Reset();
            lock (_pool)
            {
                _pool.Enqueue(buffer);
            }
        }
    }

    public class SendBufferPool
    {
        public static readonly SendBufferPool Default = new SendBufferPool(
            ContentBufferPool.Default.BufferSize + Data.Header.HeaderSize + Data.Footer.FooterSize);

        private Queue<SendBuffer> _pool = new Queue<SendBuffer>();

        public int BufferSize { get; }

        public SendBufferPool(int size)
        {
            BufferSize = size;
        }

        public SendBuffer Alloc()
        {
            lock (_pool)
            {
                if (_pool.Count > 0)
                    return _pool.Dequeue();
                return new SendBuffer(BufferSize);
            }
        }

        public void Dealloc(SendBuffer buffer)
        {
            buffer.Reset();
            lock (_pool)
            {
                _pool.Enqueue(buffer);
            }
        }
    }

    public class BufferPool
    {
        public static readonly BufferPool Default = new BufferPool(
            HeaderBufferPool.Default, ContentBufferPool.Default, FooterBufferPool.Default, SendBufferPool.Default);

        public ContentBufferPool ContentBufferPool { get; private set; }
        public HeaderBufferPool HeaderBufferPool { get; private set; }
        public FooterBufferPool FooterBufferPool { get; private set; }
        public SendBufferPool SendBufferPool { get; private set; }

        public BufferPool(HeaderBufferPool header, ContentBufferPool content, FooterBufferPool footer, SendBufferPool sendBuffer)
        {
            HeaderBufferPool = header;
            ContentBufferPool = content;
            FooterBufferPool = footer;
            SendBufferPool = sendBuffer;
        }
    }
}
