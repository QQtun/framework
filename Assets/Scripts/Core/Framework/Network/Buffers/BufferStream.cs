using Core.Framework.Network.Data;
using System;
using System.Collections.Generic;
using System.IO;

namespace Core.Framework.Network.Buffers
{
    public class BufferStream : Stream
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
            get => _curIndex * BufferBasePool.BufferSize + _curOffset;
            set => Seek(value, SeekOrigin.Begin);
        }

        private BufferBasePool BufferBasePool { get; }

        private List<BufferBase> _pages;
        private int _curIndex;
        private int _curOffset;

        private long _length;

        // ReadMessage時只能Read正確的長度 讀完時回0, 不然無法正確序列化 
        private int? _readingMessageLen;

        public BufferStream(BufferBasePool sendBufferPool)
        {
            BufferBasePool = sendBufferPool;
            _pages = new List<BufferBase>(5);
        }

        public override void Flush()
        {
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (_readingMessageLen.HasValue)
            {
                if (_readingMessageLen.Value == 0)
                    return 0;

                count = _readingMessageLen.Value;
            }

            var readLen = 0;
            for (; _curIndex < _pages.Count; )
            {
                var page = _pages[_curIndex];
                var len = Math.Min(count, page.RamainingReadSize);
                len = Math.Max(0, len);
                page.Read(buffer, offset, len);

                offset += len;
                count -= len;
                readLen += len;
                _curOffset += len;

                if(page.RamainingReadSize == 0)
                {
                    _curIndex++;
                    _curOffset = 0;
                }

                if (count == 0)
                    break;
            }

            if(_readingMessageLen.HasValue)
            {
                _readingMessageLen -= readLen;
            }
            return readLen;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            long newPos = 0;
            if(origin == SeekOrigin.End)
            {
                newPos = _length + offset;
            }
            else if(origin == SeekOrigin.Current)
            {
                newPos = _curIndex * BufferBasePool.BufferSize + _curOffset + offset;
            }
            else
            {
                // begin
                newPos = offset;
            }
            newPos = Math.Max(0, newPos);
            newPos = Math.Min(_length, newPos);

            _curIndex = 0;
            while (newPos >= BufferBasePool.BufferSize)
            {
                newPos -= BufferBasePool.BufferSize;
                _curIndex++;
            }
            _curOffset = (int)newPos;

            for (int i = 0; i < _pages.Count; i++)
            {
                var page = _pages[i];
                if (i < _curIndex)
                {
                    page.ReadPosition = page.DataSize;
                    page.WritePosition = page.DataSize;
                }
                else if(i > _curIndex)
                {
                    page.ReadPosition = 0;
                    page.WritePosition = 0;
                }
                else
                {
                    page.ReadPosition = _curOffset;
                    page.WritePosition = _curOffset;
                }
            }

            return Position;
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public void Write(BufferBase buffer)
        {
            Write(buffer.Buffer, buffer.WritePosition, buffer.RemainingWriteSize);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            while (count != 0)
            {
                if (_pages.Count == 0
                    || _pages[_pages.Count - 1].RemainingWriteSize == 0)
                {
                    _pages.Add(BufferBasePool.Alloc());
                }

                var page = _pages[_pages.Count - 1];
                var realWriteCount = page.Write(buffer, offset, count);
                offset += realWriteCount;
                count -= realWriteCount;
                _length += realWriteCount;
                _curOffset += realWriteCount;
                if(page.RemainingWriteSize == 0)
                {
                    _curIndex++;
                    _curOffset = 0;
                }
            }
        }

        public void Write(Message message)
        {
            // header
            message.Header.Serialize(this);

            // content
            message.Serialize(this);

            // footer
            var footer = Footer.Create(_pages);
            message.Footer = footer;
            footer.Serialize(this);
        }

        public void ForeachBuffer(Action<BufferBase> handler)
        {
            for (int i = 0; i < _pages.Count; i++)
            {
                handler?.Invoke(_pages[i]);
            }
        }

        public void Clear()
        {
            for (int i = 0; i < _pages.Count; i++)
            {
                BufferBasePool.Dealloc(_pages[i]);
            }
            _pages.Clear();
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
            var msg = factory.CreateMessage(header, this);
            _readingMessageLen = null;
            return msg;
        }

        public void ReleaseUnuseBuffer()
        {
            if(RemainingDataSize == 0)
            {
                Clear();
            }
            else
            {
                for (int i = _pages.Count - 1; i >= 0; i--)
                {
                    if (i < _curIndex)
                    {
                        var buffer = _pages[i];
                        _length -= buffer.DataSize;
                        _curIndex--;
                        BufferBasePool.Dealloc(buffer);
                        _pages.RemoveAt(i);
                    }
                }
            }
        }
    }
}
