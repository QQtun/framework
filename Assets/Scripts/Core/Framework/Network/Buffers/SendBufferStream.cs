using Core.Framework.Network.Data;
using System;
using System.Collections.Generic;
using System.IO;

namespace Core.Framework.Network.Buffers
{
    public class SendBufferStream : Stream
    {
        public override bool CanRead => false;

        public override bool CanSeek => false;

        public override bool CanWrite => true;

        public override long Length => _length;

        public override long Position { get => _length; set => throw new NotImplementedException(); }

        private SendBufferPool SendBufferPool { get; }

        private List<SendBuffer> _sendBuffers;
        private long _length;

        public SendBufferStream(SendBufferPool sendBufferPool)
        {
            SendBufferPool = sendBufferPool;
            _sendBuffers = new List<SendBuffer>(5);
        }

        public override void Flush()
        {
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            while (count != 0)
            {
                if (_sendBuffers.Count == 0
                    || _sendBuffers[_sendBuffers.Count - 1].BufferSize == _sendBuffers[_sendBuffers.Count - 1].DataSize)
                {
                    _sendBuffers.Add(SendBufferPool.Alloc());
                }

                var sendBuffer = _sendBuffers[_sendBuffers.Count - 1];
                sendBuffer.WriteToBuffer(buffer, offset, count, out var realWriteCount);
                offset += realWriteCount;
                count -= realWriteCount;
                _length += realWriteCount;
            }
        }

        public void Write(Message message)
        {
            // header
            message.Header.Serialize(this);

            // content
            message.Serialize(this);

            // footer
            var footer = Footer.Create(_sendBuffers);
            message.Footer = footer;
            footer.Serialize(this);
        }

        public void ForeachBuffer(Action<SendBuffer> handler)
        {
            for (int i = 0; i < _sendBuffers.Count; i++)
            {
                handler?.Invoke(_sendBuffers[i]);
            }
        }

        public void Clear()
        {
            for (int i = 0; i < _sendBuffers.Count; i++)
            {
                SendBufferPool.Dealloc(_sendBuffers[i]);
            }
            _sendBuffers.Clear();
            _length = 0;
        }
    }
}
