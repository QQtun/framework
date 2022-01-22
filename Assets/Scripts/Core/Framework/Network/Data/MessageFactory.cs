using Google.Protobuf;
using Core.Framework.Network.Buffers;
using Core.Framework.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using LogUtil;

namespace Core.Framework.Network.Data
{
    /// <summary>
    /// Author: chengdundeng
    /// Date: 2019/11/29
    /// Desc: 記錄著MessageId跟Message Type的對應 並可以產生Message實體
    /// </summary>
    public class MessageFactory
    {
        private HashSet<int> _strMsgSet = new HashSet<int>();

        // type 是 ProtoBuf的類型
        private Dictionary<int, Type> _msgIdToProtocolBufferTypeDic = new Dictionary<int, Type>();

        // type 是 IStructMessage類型
        private Dictionary<int, Type> _msgIdToTypeDic = new Dictionary<int, Type>();

        // messageId對應到type, type可能是ProtoBuf的類型 也可能是 IStructMessage類型
        private Dictionary<Type, int> _msgTypeToIdDic = new Dictionary<Type, int>();

        private object _lock = new object();

        private MemoryStream _tmpMS = new MemoryStream();

        public void RegisterMessages<T>(string namespacePrefix)
            where T : struct, IConvertible, IFormattable
        {
            Type enumType = typeof(T);
            Type googleMessage = typeof(IMessage);
            Type structMessage = typeof(IStructMessage);
            var values = Enum.GetValues(enumType);
            var names = Enum.GetNames(enumType);
            for (int i = 0; i < values.Length; ++i)
            {
                int intValue = (int) values.GetValue(i);
                string className = (string) names.GetValue(i);
                Type messageClassType = Type.GetType(namespacePrefix + className);
                if (messageClassType == null)
                {
                    var fi = enumType.GetField(className);
                    var attributes = (Google.Protobuf.Reflection.OriginalNameAttribute[])fi.GetCustomAttributes(
                     typeof(Google.Protobuf.Reflection.OriginalNameAttribute), false);
                    if (attributes.Length > 0)
                    {
                        var realClassName = attributes[0].Name;
                        realClassName = realClassName.Remove(0, 1);
                        messageClassType = Type.GetType(namespacePrefix + realClassName);
                    }
                    else
                    {
                        Debug.LogWarningFormat("message Id:{0} don't have message class", className, LogTag.Network);
                    }
                }

                if (messageClassType == null)
                {
                    //Debug.LogWarningFormat("message Id:{0} don't have message class", className, LogTag.Network);
                }
                else if (googleMessage.IsAssignableFrom(messageClassType))
                {
                    RegisterProtoBufMessage(intValue, messageClassType);
                    //Debug.LogFormat("Register Protobuf Message [{0}] {1}", intValue, messageClassType.Name, LogTag.Network);
                }
                else if (structMessage.IsAssignableFrom(messageClassType))
                {
                    RegisterCustomerClassMessage(intValue, messageClassType);
                    Debug.LogFormat("Register Custom Message [{0}] {1}", intValue, messageClassType.Name, LogTag.Network);
                }
                else
                {
                    Debug.LogWarningFormat("message class:{0} is not a legal class", className, LogTag.Network);
                }
            }
        }

        /// <summary>
        /// MessageId 對應 資料結構類型
        /// </summary>
        public IReadOnlyDictionary<int, Type> ProtobufMessageIdMap
        {
            get
            {
                return _msgIdToProtocolBufferTypeDic;
            }
        }

        public IReadOnlyDictionary<int, Type> CustomTypeMessageIdMap
        {
            get { return _msgIdToTypeDic; }
        }

        /// <summary>
        ///     記錄封包編號對應ProtoBuf的資料類型
        /// </summary>
        /// <param name="messageId">封包編號</param>
        /// <param name="type">封包編號所對應的類型</param>
        public void RegisterProtoBufMessage(int messageId, Type type)
        {
            lock (_lock)
            {
                if (!_msgIdToProtocolBufferTypeDic.ContainsKey(messageId))
                {
                    _msgIdToProtocolBufferTypeDic.Add(messageId, type);
                }

                if (!_msgTypeToIdDic.ContainsKey(type))
                {
                    _msgTypeToIdDic.Add(type, messageId);
                }
            }
        }

        /// <summary>
        ///     記錄非ProtoBuf，封包編號對應資料類型
        /// </summary>
        /// <param name="messageId"></param>
        /// <param name="type"></param>
        public void RegisterCustomerClassMessage(int messageId, Type type)
        {
            lock (_lock)
            {
                if (!_msgIdToTypeDic.ContainsKey(messageId))
                {
                    _msgIdToTypeDic.Add(messageId, type);
                }

                if (!_msgTypeToIdDic.ContainsKey(type))
                {
                    _msgTypeToIdDic.Add(type, messageId);
                }
            }
        }

        public void RegisterXMLStringMessage(int messageId)
        {
            lock (_lock)
            {
                _strMsgSet.Add(messageId);
            }
        }

        /// <summary>
        ///     取得類型對應到的封包編號
        /// </summary>
        /// <param name="messageType"></param>
        /// <returns></returns>
        public int GetMessageId(Type messageType)
        {
            int id;
            lock (_lock)
            {
                _msgTypeToIdDic.TryGetValue(messageType, out id);
            }
            return id;
        }

        /// <summary>
        ///     取得封包編號對應到的封包類型
        /// </summary>
        /// <param name="messageId"></param>
        /// <returns></returns>
        public Type GetMessageType(int messageId)
        {
            lock (_lock)
            {
                Type type;
                if (_msgIdToProtocolBufferTypeDic.TryGetValue(messageId, out type))
                {
                    return type;
                }
                if (_msgIdToTypeDic.TryGetValue(messageId, out type))
                {
                    return type;
                }
                throw new Exception("unknown message id");
            }
        }

        /// <summary>
        ///     創建封包實體
        /// </summary>
        /// <param name="header">封包標頭, 包含封包編號與長度</param>
        /// <param name="contentBuffer">binary資料來源</param>
        /// <returns></returns>
        public Message CreateMessage(Header header, ContentBuffer contentBuffer)
        {
            if (contentBuffer == null)
            {
                return CreateMessage(header, null, 0);
            }
            else
            {
                MemoryStream ms = contentBuffer.GetStream();
                ms.SetLength(contentBuffer.ReceivedSize);
                return CreateMessage(header, ms);
            }
        }

        /// <summary>
        ///     創建封包實體
        /// </summary>
        /// <param name="header">封包標頭, 包含封包編號與長度</param>
        /// <param name="data">binary資料來源</param>
        /// <param name="length">data可用長度</param>
        /// <returns></returns>
        public Message CreateMessage(Header header, byte[] data, int length)
        {
            lock (_lock)
            {
                Type type;
                Message msg = null;
                if (_msgIdToProtocolBufferTypeDic.TryGetValue(header.MessageId, out type))
                {
                    msg = GoogleProtocolBufMessage.Allocate(header);
                }
                else if (_msgIdToTypeDic.TryGetValue(header.MessageId, out type))
                {
                    msg = CustomerClassMessage.Allocate(header);
                }
                else if(_strMsgSet.Contains(header.MessageId))
                {
                    msg = StringMessage.Allocate(header);
                }
                else
                {
                    Debug.LogWarningFormat("unknown message id: {0}", header.MessageId, LogTag.Network);
                    msg = RawDataMessage.Allocate(header);
                }
                if (msg != null && data != null)
                {
                    msg.Deserialize(this, data, length);
                }
                return msg;
            }
        }

        /// <summary>
        ///     創建封包實體
        /// </summary>
        /// <param name="messageId">封包編號</param>
        /// <param name="data">binary資料來源</param>
        /// <returns></returns>
        public Message CreateMessage(int messageId, byte[] data)
        {
            if (data == null)
            {
                return CreateMessage(messageId, null, 0);
            }
            else
            {
                return CreateMessage(messageId, data, data.Length);
            }
        }

        /// <summary>
        ///     創建封包實體
        /// </summary>
        /// <param name="messageId">封包編號</param>
        /// <param name="data">binary資料來源</param>
        /// <param name="length">data可用長度</param>
        /// <returns></returns>
        public Message CreateMessage(int messageId, byte[] data, int length)
        {
            lock (_lock)
            {
                Type type;
                Message msg = null;
                if (_msgIdToProtocolBufferTypeDic.TryGetValue(messageId, out type))
                {
                    msg = GoogleProtocolBufMessage.Allocate(messageId);
                }
                else if (_msgIdToTypeDic.TryGetValue(messageId, out type))
                {
                    msg = CustomerClassMessage.Allocate(messageId);
                }
                else if (_strMsgSet.Contains(messageId))
                {
                    msg = StringMessage.Allocate(messageId);
                }
                else
                {
                    Debug.LogWarningFormat("unknown message id: {0}", messageId, LogTag.Network);
                    msg = RawDataMessage.Allocate(messageId);
                }
                if (msg != null && data != null)
                {
                    msg.Deserialize(this, data, length);
                }
                return msg;
            }
        }

        /// <summary>
        ///     建封包實體
        /// </summary>
        /// <param name="messageId">封包編號</param>
        /// <param name="ms">binary資料來源</param>
        /// <returns></returns>
        public Message CreateMessage(int messageId, MemoryStream ms)
        {
            lock (_lock)
            {
                Type type;
                Message msg = null;
                if (_msgIdToProtocolBufferTypeDic.TryGetValue(messageId, out type))
                {
                    msg = GoogleProtocolBufMessage.Allocate(messageId);
                }
                else if (_msgIdToTypeDic.TryGetValue(messageId, out type))
                {
                    msg = CustomerClassMessage.Allocate(messageId);
                }
                else if (_strMsgSet.Contains(messageId))
                {
                    msg = StringMessage.Allocate(messageId);
                }
                else
                {
                    Debug.LogWarningFormat("unknown message id: {0}", messageId, LogTag.Network);
                    msg = RawDataMessage.Allocate(messageId);
                }
                if (msg != null && ms != null)
                {
                    msg.Deserialize(this, ms);
                }
                return msg;
            }
        }

        /// <summary>
        ///     建封包實體
        /// </summary>
        /// <param name="header">封包標頭, 包含封包編號與長度</param>
        /// <param name="ms">binary資料來源</param>
        /// <returns></returns>
        public Message CreateMessage(Header header, MemoryStream ms)
        {
            lock (_lock)
            {
                Type type;
                Message msg = null;
                if (_msgIdToProtocolBufferTypeDic.TryGetValue(header.MessageId, out type))
                {
                    msg = GoogleProtocolBufMessage.Allocate(header);
                }
                else if (_msgIdToTypeDic.TryGetValue(header.MessageId, out type))
                {
                    msg = CustomerClassMessage.Allocate(header);
                }
                else if (_strMsgSet.Contains(header.MessageId))
                {
                    msg = StringMessage.Allocate(header);
                }
                else
                {
                    Debug.LogWarningFormat("unknown message id: {0} [{1}]", header.MessageId,
                        MessageNameConverter.Convert(header.MessageId),
                        LogTag.Network);
                    msg = RawDataMessage.Allocate(header);
                }
                if (msg != null && ms != null)
                {
                    msg.Deserialize(this, ms);
                }
                return msg;
            }
        }

        /// <summary>
        ///     創建封包實體
        /// </summary>
        /// <param name="header">封包標頭, 包含封包編號與長度</param>
        /// <param name="contentBuffer">binary資料來源</param>
        /// <returns></returns>
        public Message CreateMessage(Header header, List<ContentBuffer> buffers)
        {
            if (buffers == null || buffers.Count == 0)
            {
                return CreateMessage(header, null, 0);
            }
            else
            {
                lock(_tmpMS)
                {
                    _tmpMS.Seek(0, SeekOrigin.Begin);
                    _tmpMS.Position = 0;
                    _tmpMS.SetLength(0);

                    foreach (var buf in buffers)
                    {
                        _tmpMS.Write(buf.Buffer, buf.ReadStartPosition, buf.ReceivedSize);
                    }

                    _tmpMS.Seek(0, SeekOrigin.Begin);
                    _tmpMS.Position = 0;
                    _tmpMS.SetLength(header.ContentSize);
                    return CreateMessage(header, _tmpMS);
                }
            }
        }
    }
}