using LogUtil;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;

namespace Core.Framework.Network
{
    public class HostAddressHelper
    {
        private static readonly Dictionary<string, IPAddress> IpCacheV4 = new Dictionary<string, IPAddress>();
        private static readonly Dictionary<string, IPAddress> IpCacheV6 = new Dictionary<string, IPAddress>();

        public enum Addressfam
        {
            IPv4,
            IPv6
        }

#if UNITY_IOS
        [DllImport("__Internal")]
        private static extern string getIPv6(string host);
#endif

        public static string GetIPv6(string host)
        {
#if UNITY_IOS && !UNITY_EDITOR
		    return getIPv6 (host);
#else
            return host + "&&ipv4";
#endif
        }

        // Get IP type and synthesize IPv6, if needed, for iOS
        private static void GetIPType(string serverIp, out String newServerIp, out AddressFamily IPType)
        {
            IPType = AddressFamily.InterNetwork;
            newServerIp = serverIp;
            try
            {
                string IPv6 = GetIPv6(serverIp);
                if (!string.IsNullOrEmpty(IPv6))
                {
                    string[] tmp = System.Text.RegularExpressions.Regex.Split(IPv6, "&&");
                    if (tmp != null && tmp.Length >= 2)
                    {
                        string type = tmp[1];
                        if (type == "ipv6")
                        {
                            newServerIp = tmp[0];
                            IPType = AddressFamily.InterNetworkV6;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogErrorFormat("GetIPv6 error: {0}", e.Message);
            }
        }

        // Get IP address by AddressFamily and domain
        private static IPAddress GetIPAddress(string hostName, Addressfam AF)
        {
            IPAddress address = null;
            if (AF == Addressfam.IPv6 && !System.Net.Sockets.Socket.OSSupportsIPv6)
                return null;
            if (string.IsNullOrEmpty(hostName))
                return null;
            System.Net.IPHostEntry host;
            try
            {
                host = System.Net.Dns.GetHostEntry(hostName);
                foreach (System.Net.IPAddress ip in host.AddressList)
                {
                    if (AF == Addressfam.IPv4)
                    {
                        if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                        {
                            address = ip;
                        }
                    }
                    else if (AF == Addressfam.IPv6)
                    {
                        if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)
                        {
                            address = ip;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogErrorFormat("GetIPAddress error: {0}", e.Message);
            }
            return address;
        }

        // Check IP or not
        private static bool IsIPAddress(string data)
        {
            Match match = Regex.Match(data, @"\b\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}\b");
            return match.Success;
        }

        // Get Matching IP
        // Detect IP or domain & convert
        public static IPAddress GetServerIP(string server, AddressFamily family)
        {
            IPAddress address = null;

            Dictionary<string, IPAddress> ipCache;
            switch (family)
            {
                case AddressFamily.InterNetworkV6:
                    ipCache = IpCacheV6;
                    break;

                default:
                    ipCache = IpCacheV4;
                    break;
            }

            if (ipCache.TryGetValue(server, out address))
            {
                return address;
            }

            string convertedHost = "";
            var convertedFamily = AddressFamily.InterNetwork;
            if (IsIPAddress(server))
            {
                // v4 -> v6 ?
                if (family == convertedFamily)
                {
                    if (IPAddress.TryParse(server, out address))
                    {
                        ipCache[server] = address;
                        return address;
                    }
                }
                else if (family == AddressFamily.InterNetworkV6)
                {
                    GetIPType(server, out convertedHost, out convertedFamily);
                    if (IPAddress.TryParse(convertedHost, out address))
                    {
                        ipCache[server] = address;
                        return address;
                    }
                }
            }
            else
            {
                if (family == AddressFamily.InterNetworkV6)
                {
                    address = GetIPAddress(server, Addressfam.IPv6);
                    if (address == null)
                        address = GetIPAddress(server, Addressfam.IPv4);
                    else
                        convertedFamily = AddressFamily.InterNetworkV6;
                }
                else
                {
                    address = GetIPAddress(server, Addressfam.IPv4);
                }

                if (address == null)
                {
                    Debug.LogErrorFormat("Can't get IP address");
                }
            }

            if (address != null)
            {
                Debug.LogFormat("Converting to {0}, protocol {1}", address.ToString(), convertedFamily);
                ipCache[server] = address;
            }
            return address;
        }
    }
}