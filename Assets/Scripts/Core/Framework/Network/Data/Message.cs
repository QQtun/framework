using Google.Protobuf;
using Core.Framework.Memory;
using Core.Framework.Network.Serializer;
using System;
using System.IO;
using System.Reflection;
using LogUtil;
using System.Text;
using Core.Framework.Utility;

namespace Core.Framework.Network.Data
{
    /// <summary>
    /// Author: chengdundeng
    /// Date: 2019/11/29
    /// Desc: 封包基礎類型，包含標頭與資料本體
    /// </summary>
    public abstract class Message : IRefCountedObject
    {
        private IRefCountedObjectPool _poolObj;
        
        public Header Header { get; protected set; }

        /// <summary>
        ///     封包編號
        /// </summary>
        public int MessageId { get; protected set; }

        /// <summary>
        ///     資料本體
        /// </summary>
        public object Data { get; protected set; }

        /// <summary>
        ///     時間戳記
        /// </summary>
        public DateTime? TimeStamp { get; protected set; }

        /// <summary>
        ///     被發送時的回調
        /// </summary>
        public Action<Message> OnSend { get; set; }

        /// <summary>
        ///     輸出回調
        /// </summary>
        public Action<ArraySegment<byte>> OnWriteBuffer { get; set; }

        public int UseCount { get; private set; }

        public abstract int InstanceId { get; protected set; }

        protected Message(IRefCountedObjectPool pool)
        {
            UseCount = 0;
            _poolObj = pool;
        }

        protected void Init(int messageId, int contextSize = 0)
        {
            Header = new Header(messageId, contextSize);
            MessageId = messageId;
            TimeStamp = DateTime.UtcNow;
            Data = null;
            OnSend = null;
            OnWriteBuffer = null;
        }

        protected void Init(Header header)
        {
            Header = header;
            MessageId = header.MessageId;
            TimeStamp = DateTime.UtcNow;
            Data = null;
            OnSend = null;
            OnWriteBuffer = null;
        }

        protected virtual void OnRelease()
        {
            OnSend = null;
            OnWriteBuffer = null;
        }

        public int Serialize(MemoryStream ms)
        {
            return DoSerialize(ms);
        }

        public void Deserialize(MessageFactory factory, byte[] data, int size)
        {
            DoDeserialize(factory, data, size);
        }

        public void Deserialize(MessageFactory factory, MemoryStream ms)
        {
            DoDeserialize(factory, ms);
        }

        public virtual T GetData<T>()
        {
            return (T)Data;
        }

        public virtual object GetData(Type type)
        {
            return Convert.ChangeType(Data, type);
        }

        public void UpdateTimestamp(DateTime? timestamp = null)
        {
            TimeStamp = timestamp ?? DateTime.UtcNow;
        }

        protected abstract void DoDeserialize(MessageFactory factory, byte[] data, int size);

        protected abstract void DoDeserialize(MessageFactory factory, MemoryStream ms);

        protected abstract byte[] DoSerialize();

        protected abstract int DoSerialize(MemoryStream ms);

        /// <summary>
        ///     增加引用次數, 避免物件被回收
        /// </summary>
        public virtual void Retain()
        {
            ++UseCount;
        }

        /// <summary>
        ///     減少引用次數, 當物件不使用時呼叫, 當 UseCount從1減為0時發生回收
        /// </summary>
        public void Release()
        {
            if (UseCount > 0)
            {
                --UseCount;
            }
            else
            {
                return;
            }

            if (UseCount == 0 && _poolObj != null)
            {
                lock (_poolObj)
                {
                    OnRelease();
                    _poolObj.Free(this);
                }
            }
        }
    }

    /// <summary>
    /// Author: chengdundeng
    /// Date: 2019/11/29
    /// Desc: IStructMessage 的封包載體
    /// </summary>
    public class CustomerClassMessage : Message
    {
        protected static ObjectPool<CustomerClassMessage>.Factory factoryFunc;
        private static RefCountedObjectPool<CustomerClassMessage> s_poolRefCountedObject;
        private static int s_poolSize = 50;

        private static readonly string MethodNameOfParseFrom = "Deserialize";
        private static readonly Type[] TypeArrayCache = { typeof(byte[]), typeof(int) };
        private static readonly Type[] TypeArrayCache2 = { typeof(MemoryStream) };
        private static readonly object[] ObjectArrayCache = new object[2];
        private static readonly object[] ObjectArrayCache2 = new object[1];

        private static int s_instanceId;
        private static object s_idLock = new object();

        public sealed override int InstanceId { get; protected set; }

        public IStructMessage Message { get; private set; }

        static CustomerClassMessage()
        {
            factoryFunc = () => new CustomerClassMessage();
        }

        private CustomerClassMessage() : base(s_poolRefCountedObject)
        {
        }

        /// <summary>
        ///     改變保存物件最大數量
        /// </summary>
        /// <param name="size"></param>
        public static void SetReserveSize(int size)
        {
            s_poolSize = size;
            if (s_poolRefCountedObject != null)
            {
                s_poolRefCountedObject.ReservedCount = s_poolSize;
            }
        }

        /// <summary>
        ///     要求配置物件 ( 取代new功能 )
        /// </summary>
        /// <returns></returns>
        private static CustomerClassMessage Allocate()
        {
            if (s_poolRefCountedObject == null)
            {
                if (factoryFunc == null)
                {
                    Debug.LogError(string.Format("Need initialize for factoryFunc of {0}", typeof(CustomerClassMessage).Name));
                }

                s_poolRefCountedObject = ObjectPoolManager.Instance.GetPool<CustomerClassMessage>() ??
                                         ObjectPoolManager.Instance.CreatePool(s_poolSize, factoryFunc);
            }

            lock (s_poolRefCountedObject)
            {
                CustomerClassMessage obj = s_poolRefCountedObject.Allocate();
                lock (s_idLock)
                {
                    obj.InstanceId = ++s_instanceId;
                }
                obj.Retain();
                return obj;
            }
        }

        public static CustomerClassMessage Allocate(int messageId)
        {
            var msg = Allocate();
            msg.Init(messageId);
            return msg;
        }

        public static CustomerClassMessage Allocate(Header header)
        {
            var msg = Allocate();
            msg.Init(header);
            return msg;
        }

        public static CustomerClassMessage Allocate(int messageId, IStructMessage message)
        {
            var msg = Allocate();
            msg.Init(messageId, message?.CalculateSize() ?? 0);
            msg.Data = message;
            msg.Message = message;
            return msg;
        }

        public static CustomerClassMessage Allocate(Header header, IStructMessage message)
        {
            var msg = Allocate();
            msg.Init(header);
            msg.Data = message;
            msg.Message = message;
            return msg;
        }

        protected override void DoDeserialize(MessageFactory factory, byte[] data, int size)
        {
            Type type = factory.GetMessageType(MessageId);

            if (type != null)
            {
                MethodInfo info = type.GetMethod(MethodNameOfParseFrom, TypeArrayCache);
                if (info != null)
                {
                    ObjectArrayCache[0] = data;
                    ObjectArrayCache[1] = size;
                    Data = info.Invoke(null, ObjectArrayCache);
                    Message = Data as IStructMessage;
                }
            }
        }

        protected override void DoDeserialize(MessageFactory factory, MemoryStream ms)
        {
            Type type = factory.GetMessageType(MessageId);

            if (type != null)
            {
                MethodInfo info = type.GetMethod(MethodNameOfParseFrom, TypeArrayCache2);
                if (info != null)
                {
                    ObjectArrayCache2[0] = ms;
                    Data = info.Invoke(null, ObjectArrayCache2);
                    Message = Data as IStructMessage;
                }
            }
        }

        protected override byte[] DoSerialize()
        {
            if (Message != null)
            {
                return Message.Serialize();
            }
            return null;
        }

        protected override int DoSerialize(MemoryStream ms)
        {
            if (Message != null)
            {
                return Message.Serialize(ms);
            }
            return -1;
        }
    }

    /// <summary>
    /// Author: chengdundeng
    /// Date: 2019/11/29
    /// Desc: Google.Protobuf的IMessage的封包載體
    /// </summary>
    public class GoogleProtocolBufMessage : Message
    {
        private static readonly MemoryStream MemoryStreamCache = new MemoryStream();
        protected static ObjectPool<GoogleProtocolBufMessage>.Factory factoryFunc;
        private static RefCountedObjectPool<GoogleProtocolBufMessage> s_poolRefCountedObject;
        private static int s_poolSize = 50;

        private static int s_instanceId;
        private static object s_idLock = new object();

        public sealed override int InstanceId { get; protected set; }

        static GoogleProtocolBufMessage()
        {
            factoryFunc = () => new GoogleProtocolBufMessage();
        }

        private GoogleProtocolBufMessage() : base(s_poolRefCountedObject)
        {
        }

        /// <summary>
        ///     改變保存物件最大數量
        /// </summary>
        /// <param name="size"></param>
        public static void SetReserveSize(int size)
        {
            s_poolSize = size;
            if (s_poolRefCountedObject != null)
            {
                s_poolRefCountedObject.ReservedCount = s_poolSize;
            }
        }

        /// <summary>
        ///     要求配置物件 ( 取代new功能 )
        /// </summary>
        /// <returns></returns>
        protected static GoogleProtocolBufMessage Allocate()
        {
            if (s_poolRefCountedObject == null)
            {
                if (factoryFunc == null)
                {
                    Debug.LogError(string.Format("Need initialize for factoryFunc of {0}", typeof(GoogleProtocolBufMessage).Name));
                }
                s_poolRefCountedObject = ObjectPoolManager.Instance.GetPool<GoogleProtocolBufMessage>() ??
                ObjectPoolManager.Instance.CreatePool(s_poolSize, factoryFunc);
            }

            lock (s_poolRefCountedObject)
            {
                GoogleProtocolBufMessage obj = s_poolRefCountedObject.Allocate();
                lock (s_idLock)
                {
                    obj.InstanceId = ++s_instanceId;
                }
                obj.Retain();
                return obj;
            }
        }

        public static GoogleProtocolBufMessage Allocate(int messageId)
        {
            var msg = Allocate();
            msg.Init(messageId);
            return msg;
        }

        public static GoogleProtocolBufMessage Allocate(Header header)
        {
            var msg = Allocate();
            msg.Init(header);
            return msg;
        }

        public static GoogleProtocolBufMessage Allocate(int messageId, IMessage message)
        {
            var msg = Allocate();
            msg.Init(messageId, message?.CalculateSize() ?? 0);
            msg.Data = message;
            return msg;
        }

        public static GoogleProtocolBufMessage Allocate(Header header, IMessage message)
        {
            var msg = Allocate();
            msg.Init(header);
            msg.Data = message;
            return msg;
        }

        public IMessage Message { get { return Data as IMessage; } }

        protected override void DoDeserialize(MessageFactory factory, byte[] data, int size)
        {
            Type type = factory.GetMessageType(MessageId);

            if (type != null)
            {
                MemoryStream ms = new MemoryStream(data, 0, size);
                Data = GoogleProtocolSerializer.Deserialize(ms, type);
            }
        }

        protected override void DoDeserialize(MessageFactory factory, MemoryStream ms)
        {
            Type type = factory.GetMessageType(MessageId);

            if (type != null)
            {
                Data = GoogleProtocolSerializer.Deserialize(ms, type);
            }
        }

        protected override byte[] DoSerialize()
        {
            MemoryStreamCache.Position = 0;
            MemoryStreamCache.SetLength(0);

            GoogleProtocolSerializer.Serialize(MemoryStreamCache, Message);

            int size = (int)MemoryStreamCache.Length;
            byte[] data = new byte[size];
            MemoryStreamCache.Seek(0, SeekOrigin.Begin);
            MemoryStreamCache.Read(data, 0, size);

            return data;
        }

        protected override int DoSerialize(MemoryStream ms)
        {
            long startPos = ms.Position;
            GoogleProtocolSerializer.Serialize(ms, Message);
            long endPos = ms.Position;

            return (int)(endPos - startPos);
        }
    }

    public class StringMessage : Message
    {
        protected static ObjectPool<StringMessage>.Factory factoryFunc;
        private static RefCountedObjectPool<StringMessage> s_poolRefCountedObject;
        private static int s_poolSize = 50;

        private static int s_instanceId;
        private static object s_idLock = new object();

        private int _fieldCount;
        private string[] _fields;
        private ArraySeg<string> _fieldArray = new ArraySeg<string>();
        private StringBuilder _stringBuilder;
        private byte[] _deserializeBufferCache;
        private string _stringCache;

        public sealed override int InstanceId { get; protected set; }

        public ArraySeg<string> Fields
        {
            get
            {
                _fieldArray.Update(_fields, 0, _fieldCount);
                return _fieldArray;
            }
        }

        static StringMessage()
        {
            factoryFunc = () => new StringMessage();
        }

        private StringMessage() : base(s_poolRefCountedObject)
        {
            _stringBuilder = new StringBuilder();
        }

        private static StringMessage Allocate()
        {
            if (s_poolRefCountedObject == null)
            {
                if (factoryFunc == null)
                {
                    Debug.LogError(string.Format("Need initialize for factoryFunc of {0}", typeof(StringMessage).Name));
                }

                s_poolRefCountedObject = ObjectPoolManager.Instance.GetPool<StringMessage>() ??
                                         ObjectPoolManager.Instance.CreatePool(s_poolSize, factoryFunc);
            }

            lock (s_poolRefCountedObject)
            {
                StringMessage obj = s_poolRefCountedObject.Allocate();
                lock (s_idLock)
                {
                    obj.InstanceId = ++s_instanceId;
                }
                obj.Retain();
                return obj;
            }
        }

        public static StringMessage Allocate(int msgId)
        {
            var msg = Allocate();
            msg.Init(msgId);
            msg._fieldCount = 0;
            return msg;
        }

        public static StringMessage Allocate(Header header)
        {
            var msg = Allocate();
            msg.Init(header);
            msg._fieldCount = 0;
            return msg;
        }

        public void Append(params string[] field)
        {
            if (_fields == null)
            {
                _fields = new string[field.Length + 10];
                _fieldCount = 0;
            }

            if (_fields.Length - _fieldCount < field.Length)
            {
                var newFields = new string[_fields.Length + 10];
                Array.Copy(_fields, newFields, _fieldCount);
                _fields = newFields;
            }

            for (int i = 0; i < field.Length; i++)
            {
                _fields[_fieldCount + i] = field[i];
            }
            _fieldCount += field.Length;

            _stringBuilder.Clear();
            if (_fields != null && _fieldCount > 0)
            {
                for (int i = 0; i < _fieldCount; i++)
                {
                    if (i == 0)
                        _stringBuilder.Append(_fields[i]);
                    else
                        _stringBuilder.AppendFormat(":{0}", _fields[i]);
                }
            }
            _stringCache = _stringBuilder.ToString();
            _stringCache = _stringCache ?? string.Empty;
            var size = Encoding.UTF8.GetByteCount(_stringCache);
            Header = new Header(Header.MessageId, size);
        }

        protected override void DoDeserialize(MessageFactory factory, byte[] data, int size)
        {
            var str = Encoding.UTF8.GetString(data, 0, size);

            var fields = str.Split(':');
            if (_fields != null)
                Array.Clear(_fields, 0, _fields.Length);
            if (_fields == null || _fields.Length < fields.Length)
                _fields = str.Split(':');
            else
                Array.Copy(fields, 0, _fields, 0, fields.Length);
            _fieldCount = fields.Length;
            Data = Fields;
        }

        protected override void DoDeserialize(MessageFactory factory, MemoryStream ms)
        {
            if (_deserializeBufferCache == null || _deserializeBufferCache.Length < ms.Length)
                _deserializeBufferCache = new byte[ms.Length + 100];
            Array.Clear(_deserializeBufferCache, 0, _deserializeBufferCache.Length);

            int length = (int)ms.Length;
            ms.Read(_deserializeBufferCache, 0, length);
            var str = Encoding.UTF8.GetString(_deserializeBufferCache, 0, length);

            var fields = str.Split(':');
            if (_fields != null)
                Array.Clear(_fields, 0, _fields.Length);
            if (_fields == null || _fields.Length < fields.Length)
                _fields = str.Split(':');
            else
                Array.Copy(fields, 0, _fields, 0, fields.Length);
            _fieldCount = fields.Length;
            Data = Fields;
        }

        protected override byte[] DoSerialize()
        {
            var strBytes = Encoding.UTF8.GetBytes(_stringCache);
            return strBytes;
        }

        protected override int DoSerialize(MemoryStream ms)
        {
            var strBytes = Encoding.UTF8.GetBytes(_stringCache);
            ms.Write(strBytes, 0, strBytes.Length);
            return strBytes.Length;
        }
    }

    public class RawDataMessage : Message
    {
        private static readonly MemoryStream MemoryStreamCache = new MemoryStream();
        protected static ObjectPool<RawDataMessage>.Factory factoryFunc;
        private static RefCountedObjectPool<RawDataMessage> s_poolRefCountedObject;
        private static int s_poolSize = 50;

        private static int s_instanceId;
        private static object s_idLock = new object();

        private byte[] _buffer;
        private int _bufferCount;
        private ArraySeg<byte> _rawData = new ArraySeg<byte>();

        public ArraySeg<byte> RawData
        {
            get
            {
                _rawData.Update(_buffer, 0, _bufferCount);
                return _rawData;
            }
        }

        static RawDataMessage()
        {
            factoryFunc = () => new RawDataMessage();
        }

        public RawDataMessage() : base(s_poolRefCountedObject)
        {
        }

        public sealed override int InstanceId { get; protected set; }

        public static RawDataMessage Allocate(int msgId)
        {
            var msg = Allocate();
            msg.Init(msgId);
            return msg;
        }

        public static RawDataMessage Allocate(Header header)
        {
            var msg = Allocate();
            msg.Init(header);
            return msg;
        }
        public static RawDataMessage Allocate(int msgId, byte[] data)
        {
            var msg = Allocate();
            msg.Init(msgId, data?.Length ?? 0);

            if(data != null)
            {
                if (msg._buffer == null || msg._buffer.Length < data.Length)
                    msg._buffer = data;
                else
                    Array.Copy(data, msg._buffer, data.Length);
                msg._bufferCount = data.Length;
            }

            msg.Data = msg.RawData;
            return msg;
        }

        private static RawDataMessage Allocate()
        {
            if (s_poolRefCountedObject == null)
            {
                if (factoryFunc == null)
                {
                    Debug.LogError(string.Format("Need initialize for factoryFunc of {0}", typeof(RawDataMessage).Name));
                }

                s_poolRefCountedObject = ObjectPoolManager.Instance.GetPool<RawDataMessage>() ??
                                         ObjectPoolManager.Instance.CreatePool(s_poolSize, factoryFunc);
            }

            lock (s_poolRefCountedObject)
            {
                RawDataMessage obj = s_poolRefCountedObject.Allocate();
                lock (s_idLock)
                {
                    obj.InstanceId = ++s_instanceId;
                }
                obj.Retain();
                return obj;
            }
        }

        protected override void DoDeserialize(MessageFactory factory, byte[] data, int size)
        {
            if (_buffer == null || _buffer.Length < size)
                _buffer = new byte[size];
            Array.Copy(data, _buffer, size);
            _bufferCount = size;
            Data = RawData;
        }

        protected override void DoDeserialize(MessageFactory factory, MemoryStream ms)
        {
            int size = (int)ms.Length;
            if (_buffer == null || _buffer.Length < size)
                _buffer = new byte[size];
            ms.Read(_buffer, 0, size);
            _bufferCount = size;
            Data = RawData;
        }

        protected override byte[] DoSerialize()
        {
            MemoryStreamCache.Position = 0;
            MemoryStreamCache.SetLength(0);

            MemoryStreamCache.Write(_buffer, 0, _bufferCount);

            int size = (int)MemoryStreamCache.Length;
            byte[] outBuffer = new byte[size];
            MemoryStreamCache.Seek(0, SeekOrigin.Begin);
            MemoryStreamCache.Read(outBuffer, 0, size);
            return outBuffer;
        }

        protected override int DoSerialize(MemoryStream ms)
        {

            long startPos = ms.Position;
            ms.Write(_buffer, 0, _bufferCount);
            long endPos = ms.Position;

            return (int)(endPos - startPos);
        }
    }
}