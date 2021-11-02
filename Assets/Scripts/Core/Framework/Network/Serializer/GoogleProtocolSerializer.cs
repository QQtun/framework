using Google.Protobuf;
using LogUtil;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Core.Framework.Network.Serializer
{
    /// <summary>
    ///     序列與解序列化訊息物件
    /// </summary>
    public class GoogleProtocolSerializer
    {
        private static object s_lock = new object();
        private static Dictionary<Type, MessageParser> s_typeToParserDic = new Dictionary<Type, MessageParser>();
        private static Dictionary<Type, PropertyInfo> s_typeToRetCodePropertyDic = new Dictionary<Type, PropertyInfo>();
        private static bool s_initialized = false;

        /// <summary>
        ///
        /// </summary>
        /// <param name="msgTypes">所有Message類別</param>
        /// <param name="force"></param>
        public static void Initialize(List<Type> msgTypes, bool force = false)
        {
            lock (s_lock)
            {
                if (!force && s_initialized)
                    return;

                foreach (var type in msgTypes)
                {
                    MessageParser parser;

                    if (!s_typeToParserDic.TryGetValue(type, out parser))
                    {
                        PropertyInfo property = type.GetProperty("Parser", BindingFlags.Static | BindingFlags.Public);
                        if (property != null)
                        {
                            parser = property.GetValue(null, null) as MessageParser;
                            if (parser != null)
                            {
                                //IGG.Debug.LogFormat("Log Parser for type {0}", type.Name, CommonUtility.LogTag.Network);
                                s_typeToParserDic.Add(type, parser);
                            }
                        }

                        var propertyRetCode = type.GetProperty("RetCode", BindingFlags.Public | BindingFlags.Instance);
                        if (propertyRetCode != null)
                        {
                            s_typeToRetCodePropertyDic.Add(type, propertyRetCode);
                        }
                        else
                        {
                            propertyRetCode = type.GetProperty("Result", BindingFlags.Public | BindingFlags.Instance);
                            if (propertyRetCode != null)
                            {
                                Debug.LogWarningFormat("Incorrect property {0} of message type {1}", propertyRetCode.Name, type.Name, LogTag.System);
                                s_typeToRetCodePropertyDic.Add(type, propertyRetCode);
                            }
                        }
                    }
                }

                s_initialized = true;
            }
        }

        /// <summary>
        ///     取出Result的值
        /// </summary>
        /// <param name="message">目標封包</param>
        /// <param name="nRetCode">結果值</param>
        /// <returns></returns>
        public static bool GetRetCode(IMessage message, out int nRetCode)
        {
            //Initialize();

            lock (s_lock)
            {
                PropertyInfo property;
                if (s_typeToRetCodePropertyDic.TryGetValue(message.GetType(), out property))
                {
                    nRetCode = (int)property.GetValue(message, null);
                    return true;
                }
                nRetCode = 0;
                return false;
            }
        }

        /// <summary>
        ///     binary資料轉成封包結構
        /// </summary>
        /// <param name="s">binary資料來源</param>
        /// <param name="type">目標結構類型</param>
        /// <returns>傳入type所產生的實體物件</returns>
        public static IMessage Deserialize(Stream s, Type type)
        {
            //Initialize();

            lock (s_lock)
            {
                MessageParser parser;
                if (!s_typeToParserDic.TryGetValue(type, out parser))
                {
                    PropertyInfo property = type.GetProperty("Parser", BindingFlags.Static | BindingFlags.Public);
                    if (property != null)
                    {
                        parser = property.GetValue(null, null) as MessageParser;
                        if (parser != null)
                        {
                            s_typeToParserDic.Add(type, parser);
                        }
                    }
                }

                if (parser == null)
                    return null;
                return parser.ParseFrom(s);
            }
        }

        /// <summary>
        ///     將封包轉成binary寫入Stream
        /// </summary>
        /// <param name="s">輸出目標</param>
        /// <param name="obj">欲輸出的封包本體</param>
        public static void Serialize(Stream s, IMessage obj)
        {
            obj.WriteTo(s);
        }
    }
}