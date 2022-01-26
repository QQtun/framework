using System.Collections.Generic;

namespace Core.Framework.Network.Buffers
{
    public class BufferBasePool
    {
        public static readonly BufferBasePool Default = new BufferBasePool(256);

        private Queue<BufferBase> _pool = new Queue<BufferBase>();

        public int BufferSize { get; }

        public BufferBasePool(int size)
        {
            BufferSize = size;
        }

        public BufferBase Alloc()
        {
            lock (_pool)
            {
                if (_pool.Count > 0)
                    return _pool.Dequeue();
                return new BufferBase(BufferSize);
            }
        }

        public void Dealloc(BufferBase buffer)
        {
            buffer.Reset();
            lock (_pool)
            {
                _pool.Enqueue(buffer);
            }
        }
    }

    public class BufferStreamPool
    {
        public static readonly BufferStreamPool Default = new BufferStreamPool(BufferBasePool.Default);

        private Queue<BufferStream> _pool = new Queue<BufferStream>();

        public BufferBasePool BufferBasePool { get; }

        public BufferStreamPool(BufferBasePool bufferPool)
        {
            BufferBasePool = bufferPool;
        }

        public BufferStream Alloc()
        {
            lock (_pool)
            {
                if (_pool.Count > 0)
                    return _pool.Dequeue();
                return new BufferStream(BufferBasePool);
            }
        }

        public void Dealloc(BufferStream stream)
        {
            stream.Clear();
            lock (_pool)
            {
                _pool.Enqueue(stream);
            }
        }
    }

    public class BufferPoolProvider
    {
        public static readonly BufferPoolProvider Default = new BufferPoolProvider(BufferBasePool.Default);

        public BufferBasePool BufferBasePool { get; private set; }
        public BufferStreamPool BufferStreamPool { get; private set; }

        public BufferPoolProvider(BufferBasePool bufferPool)
        {
            BufferBasePool = bufferPool;
            BufferStreamPool = new BufferStreamPool(BufferBasePool);
        }
    }
}
