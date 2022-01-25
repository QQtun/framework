using Core.Framework.Network.Data;
using LogUtil;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Core.Framework.Network
{
    public class MessageHandlerAttribute : Attribute
    {
        public int MessageId { get; }

        public MessageHandlerAttribute(int messageID)
        {
            MessageId = messageID;
        }
    }

    public static class MessageHandlerUtil
    {
        public struct HandlerInfo
        {
            public MethodInfo methodInfo;
            public Type paramType;
        }

        private static Dictionary<Type, Dictionary<int, HandlerInfo>> s_typeToHandlersDic
            = new Dictionary<Type, Dictionary<int, HandlerInfo>>();

        public static void Init(Type type)
        {
            if (type == null)
                return;
            if (s_typeToHandlersDic.ContainsKey(type))
                return;

            var handlers = new Dictionary<int, HandlerInfo>();
            foreach (var method in type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                var attr = method.GetCustomAttribute<MessageHandlerAttribute>();
                if (attr != null)
                {
                    if (handlers.TryGetValue(attr.MessageId, out var _))
                    {
                        Debug.LogError($"duplicate message handler method={method.Name} MessageId={attr.MessageId}!!");
                    }
                    else
                    {
                        var paramArray = method.GetParameters();
                        if (paramArray.Length == 1)
                        {
                            var msgParam = paramArray[0];
                            if (msgParam.ParameterType.IsSubclassOf(typeof(Message)))
                            {
                                handlers.Add(attr.MessageId, new HandlerInfo()
                                {
                                    methodInfo = method,
                                });
                            }
                        }
                        else if (paramArray.Length == 2)
                        {
                            var msgParam = paramArray[0];
                            var dataParam = paramArray[1];
                            if (msgParam.ParameterType == typeof(Message)
                                && typeof(Google.Protobuf.IMessage).IsAssignableFrom(dataParam.ParameterType))
                            {
                                handlers.Add(attr.MessageId, new HandlerInfo()
                                {
                                    methodInfo = method,
                                    paramType = dataParam.ParameterType
                                });
                            }
                            else if (msgParam.ParameterType == typeof(Message)
                                && dataParam.ParameterType == typeof(Utility.ArraySeg<string>))
                            {
                                handlers.Add(attr.MessageId, new HandlerInfo()
                                {
                                    methodInfo = method,
                                    paramType = dataParam.ParameterType
                                });
                            }
                        }
                    }
                }
            }
            if (handlers.Count > 0)
            {
                lock (s_typeToHandlersDic)
                {
                    s_typeToHandlersDic[type] = handlers;
                }
            }
        }

        public static HandlerInfo? GetHandlerMethod(Type handlerType, int messageId)
        {
            Dictionary<int, HandlerInfo> handlerDic;
            lock (s_typeToHandlersDic)
            {
                if (!s_typeToHandlersDic.TryGetValue(handlerType, out handlerDic))
                {
                    return null;
                }
            }
            if (!handlerDic.TryGetValue(messageId, out var m))
            {
                return null;
            }
            return m;
        }



        private static object[] _invokeArrayCache1 = new object[1];
        private static object[] _invokeArrayCache2 = new object[2];

        public static bool TryInvokeHandler(object obj, Message msg)
        {
            var h = GetHandlerMethod(obj.GetType(), msg.MessageId);
            if (!h.HasValue)
            {
                return false;
            }
            var handlerInfo = h.Value;
            if (handlerInfo.paramType == null)
            {
                _invokeArrayCache1[0] = msg;
                handlerInfo.methodInfo.Invoke(obj, _invokeArrayCache1);
            }
            else
            {
                _invokeArrayCache2[0] = msg;
                _invokeArrayCache2[1] = msg.GetData(handlerInfo.paramType);
                handlerInfo.methodInfo.Invoke(obj, _invokeArrayCache2);
            }
            return true;
        }
    }
}
