using Core.Framework.Network.Data;
using System;
using System.Collections.Generic;
using System.IO;

namespace Core.Framework.Network.Buffers
{
    public class SendBufferStream : Stream
    {
        public override bool CanRead => true;

        public override bool CanSeek => true;

        public override bool CanWrite => true;

        public override long Length => _length;

        public bool CanReadHeader => RemainingDataSize >= Header.HeaderSize;

        public bool CanReadFooter => RemainingDataSize >= Footer.FooterSize;

        public long RemainingDataSize => Length - Position;

        public override long Position
        {
            get => _curIndex * SendBufferPool.BufferSize + _curOffset;
            set => Seek(value, SeekOrigin.Begin);
        }

        private SendBufferPool SendBufferPool { get; }

        private List<SendBuffer> _sendBuffers;
        private int _curIndex;
        private int _curOffset;

        private long _length;
        private int? _readingMessageLen;
        private bool _messageParserRead;

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
            if (_readingMessageLen.HasValue)
            {
                if (_messageParserRead)
                    return 0;

                count = _readingMessageLen.Value;
                _messageParserRead = true;
            }

            var readLen = 0;
            for (; _curIndex < _sendBuffers.Count; )
            {
                var bf = _sendBuffers[_curIndex];
                var len = Math.Min(count, bf.DataSize - _curOffset);
                len = Math.Max(0, len);
                len = Math.Min(len, bf.DataSize);
                Array.Copy(bf.Buffer, _curOffset, buffer, offset, len);
                offset += len;
                count -= len;
                readLen += len;
                _curOffset += len;
                if(bf.DataSize - _curOffset == 0)
                {
                    _curIndex++;
                    _curOffset = 0;
                }
                if (count == 0)
                    break;
            }
            return readLen;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            if(origin == SeekOrigin.End)
            {
                var newPos = _length + offset;
                newPos = Math.Max(0, newPos);
                newPos = Math.Min(_length, newPos);
                for (int i = 0; ;i++)
                {
                    if(newPos < SendBufferPool.BufferSize)
                    {
                        _curIndex = i;
                        _curOffset = (int)newPos;
                        break;
                    }
                    else
                    {
                        newPos -= SendBufferPool.BufferSize;
                    }
                }
            }
            else if(origin == SeekOrigin.Current)
            {
                var newPos = _curIndex * SendBufferPool.BufferSize + offset;
                _curIndex = 0;
                while (newPos >= SendBufferPool.BufferSize)
                {
                    newPos -= SendBufferPool.BufferSize;
                    _curIndex++;
                }
                _curOffset = (int)newPos;
            }
            else
            {
                // begin
                _curIndex = 0;
                var newPos = Math.Min(_length, offset);
                newPos = Math.Max(0, newPos);
                while (newPos >= SendBufferPool.BufferSize)
                {
                    newPos -= SendBufferPool.BufferSize;
                    _curIndex++;
                }
                _curOffset = (int)newPos;
            }

            return _curIndex * SendBufferPool.BufferSize + _curOffset;
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
            _curIndex = 0;
            _curOffset = 0;
        }

        public bool CanReadContent(int contentSize)
        {
            return RemainingDataSize >= contentSize;
        }

        public Header ReadHeaer()
        {
            var header = new Header();
            header.Deserialize(this);
            return header;
        }

        public Footer ReadFooter()
        {
            var footer = new Footer();
            footer.Deserialize(this);
            return footer;
        }

        public Message ReadMessage(MessageFactory factory, Header header)
        {
            _readingMessageLen = header.ContentSize;
            _messageParserRead = false;
            var msg = factory.CreateMessage(header, this);
            _readingMessageLen = null;
            return msg;
        }

        public void ReleaseUnuseBuffer()
        {
            for (int i = _sendBuffers.Count - 1; i >= 0; i--)
            {
                if (i < _curIndex)
                {
                    var buffer = _sendBuffers[i];
                    _length -= buffer.DataSize;
                    _curIndex--;
                    SendBufferPool.Dealloc(buffer);
                    _sendBuffers.RemoveAt(i);
                }
            }
            if (_sendBuffers.Count == 1 && _length == _curOffset)
            {
                Clear();
            }
        }
    }
}
