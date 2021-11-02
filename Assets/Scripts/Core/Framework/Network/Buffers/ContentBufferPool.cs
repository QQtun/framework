using System.Collections.Generic;

namespace Core.Framework.Network.Buffers
{
    public class ContentBufferPool
    {
        public static readonly ContentBufferPool Default = new ContentBufferPool(1024 * 4);

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
            _pool.Enqueue(buffer);
        }
    }
}
