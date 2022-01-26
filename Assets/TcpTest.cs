using Core.Framework.Network;
using Core.Framework.Network.Buffers;
using Core.Framework.Network.Data;
using Core.Framework.Utility;
using P5.Protobuf;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public class TcpTest : MonoBehaviour, IMessageNameConverter
{
    public string ip;
    public int port;

    private List<TcpClientForClient> _clients = new List<TcpClientForClient>();
    private TcpServerTest _server;
    private MessageFactory _msgFactory;

    void Start()
    {
        MessageNameConverter.Delegate = this;

        System.Threading.ThreadPool.GetMinThreads(out var workerThreads, out var completThreads);
        Debug.Log($"min workerThreads={workerThreads} completThreads={completThreads}");
        System.Threading.ThreadPool.GetMaxThreads(out workerThreads, out completThreads);
        Debug.Log($"max workerThreads={workerThreads} completThreads={completThreads}");

        _msgFactory = new MessageFactory();
        _msgFactory.RegisterProtoBufMessage(1, typeof(UserLoginOn));
        _msgFactory.RegisterXMLStringMessage(2);

        var clientFactory = new ClientFactoryForServer();
        _server = new TcpServerTest(clientFactory, _msgFactory, BufferPoolProvider.Default);
        clientFactory.Server = _server;
        _server.Start(IPAddress.Parse(ip), port);

        //MessageHandlerUtil.Init(typeof(TcpClientForClient));
        //_client = new TcpClientForClient(_msgFactory, BufferPool.Default);

        //var a = uint.MaxValue;
        //Debug.Log($"a={a}");
        //a = a + 1;
        //Debug.Log($"a={a}");
    }

    [Button]
    public void Connect()
    {
        var client = new TcpClientForClient(_msgFactory, BufferPoolProvider.Default);
        _clients.Add(client);
        client.Connect(IPAddress.Parse(ip), port);
    }

    [Button]
    public void Disconnet()
    {
        foreach(var client in _clients)
        {
            if (client.Connected)
            {
                client.Disconnect(DisconnectReason.User);
                break;
            }
        }
    }

    // Update is called once per frame
    private void Update()
    {
        _server?.MainLoop();
        foreach(var client in _clients)
        {
            client?.MainLoop();
        }
    }

    private void OnDestroy()
    {
        foreach (var client in _clients)
        {
            if (client.Connected)
                client?.Disconnect(DisconnectReason.User, true);
        }
        _clients.Clear();

        _server?.Stop(DisconnectReason.User);
    }

    public string Convert(int messageId)
    {
        return messageId.ToString();
    }
}


public class TcpClientForClient : TcpClientBase
{
    public TcpClientForClient(MessageFactory factory, BufferPoolProvider pool) : base(factory, pool)
    {
    }

    protected override void OnConnected()
    {
        Debug.Log("TcpClientForClient OnConnected");

        var content = new UserLoginOn();
        content.UserName = "abc";
        var msg = GoogleProtocolBufMessage.Allocate(1, content);
        var ret = Request(msg);
        _userLoginPing = new ValueTuple<uint, DateTime>(ret.Item2, DateTime.Now);

        var strMsg = StringMessage.Allocate(2);
        strMsg.Append("aaa", "123");
        ret = Request(strMsg);
        _strMsgPing = new ValueTuple<uint, DateTime>(ret.Item2, DateTime.Now);
    }

    protected override void OnDisconnected(DisconnectReason reason)
    {
        Debug.Log("TcpClientForClient OnDisconnected reason=" + reason);
    }

    private int _count = 0;
    private ValueTuple<uint, DateTime> _userLoginPing;

    [MessageHandler(1)]
    private void OnUserLogin(Message msg, UserLoginOn userLoginOn)
    {
        Debug.Log("TcpClientForClient OnUserLogin " + userLoginOn.UserName);
        var diff = DateTime.Now - _userLoginPing.Item2;
        Debug.Log($"diff.TotalMilliseconds={diff.TotalMilliseconds}");


        var content = new UserLoginOn();
        content.UserName = "abc" + _count++;
        var ret = Request(GoogleProtocolBufMessage.Allocate(1, content));
        _userLoginPing = new ValueTuple<uint, DateTime>(ret.Item2, DateTime.Now);
    }

    private ValueTuple<uint, DateTime> _strMsgPing;
    [MessageHandler(2)]
    private void OnStringMsg(Message msg, ArraySeg<string> strs)
    {
        Debug.Log("TcpClientForClient OnStringMsg strs[0]=" + strs[0]);
        var diff = DateTime.Now - _strMsgPing.Item2;
        Debug.Log($"diff.TotalMilliseconds={diff.TotalMilliseconds}");

        var strMsg = StringMessage.Allocate(2);
        strMsg.Append("abc" + _count++);
        var ret = Request(strMsg);
        _strMsgPing = new ValueTuple<uint, DateTime>(ret.Item2, DateTime.Now);
    }
}

public class TcpClientForServer : TcpClientBaseForServer<TcpClientForServer>
{
    public TcpClientForServer(TcpServerBase<TcpClientForServer> server, TcpClient client) 
        : base(server, client)
    {
    }

    protected override void OnConnected()
    {
        Debug.Log("TcpClientForServer OnConnected");
    }

    protected override void OnDisconnected(DisconnectReason reason)
    {
        base.OnDisconnected(reason);
        Debug.Log("TcpClientForServer OnDisconnected reason="+ reason);
    }

    private int _count = 0;

    [MessageHandler(1)]
    private void OnUserLogin(Message msg, UserLoginOn userLoginOn)
    {
        Debug.Log("TcpClientForServer OnUserLogin " + userLoginOn.UserName);

        var content = new UserLoginOn();
        content.UserName = "xyz" + _count++;
        Response(msg.Header.RequsetSerial, GoogleProtocolBufMessage.Allocate(1, content));
    }

    [MessageHandler(2)]
    private void OnStringMsg(Message msg, ArraySeg<string> strs)
    {
        Debug.Log("TcpClientForServer OnStringMsg strs[0]=" + strs[0]);

        var strMsg = StringMessage.Allocate(2);
        strMsg.Append("xyz" + _count++);
        Response(msg.Header.RequsetSerial, strMsg);
    }
}

public class TcpServerTest : TcpServerBase<TcpClientForServer>
{
    public TcpServerTest(ITcpClientFactory<TcpClientForServer> clientFactory, MessageFactory factory, BufferPoolProvider pool)
        : base(clientFactory, factory, pool)
    {
    }
}

public class ClientFactoryForServer : ITcpClientFactory<TcpClientForServer>
{
    public TcpServerBase<TcpClientForServer> Server { get; set; }

    public TcpClientForServer Create(TcpClient client)
    {
        return new TcpClientForServer(Server, client);
    }
}