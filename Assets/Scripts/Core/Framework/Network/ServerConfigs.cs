using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Core.Framework.Network
{
    [Serializable]
    public class ServerConfig
    {
        public string name;
        public int bufferSize = 131072;
        public int retryCount = 0;
        public int connectTimeout = 10000;
        public int sendTimeout = 10000;
        public int receiveTimeout = 10000;
        public bool noDelay = true;
    }

    [Serializable]
    public class ServerSet
    {
        public string name;
        public string ip;
        public int port;
    }

    [CreateAssetMenu(fileName = "ServerConfigs", menuName = "Config/Server Configs")]
    public class ServerConfigs : ScriptableObject
    {
        public List<ServerConfig> configs = new List<ServerConfig>();
        public List<ServerSet> serverSets = new List<ServerSet>();

        public ServerSet GetServerSet(string name)
        {
            return serverSets.Find(s => s.name == name);
        }
        public ServerConfig GetConfig(string name)
        {
            return configs.Find(s => s.name == name);
        }
    }
}