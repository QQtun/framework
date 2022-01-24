using Core.Framework.Network;
using Core.Framework.Network.Buffers;
using Core.Framework.Network.Data;
using Core.Framework.Utility;
using P5.Protobuf;
using Sirenix.OdinInspector;
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
        MessageNameConverter.Converter = this;

        System.Threading.ThreadPool.GetMinThreads(out var workerThreads, out var completThreads);
        Debug.Log($"min workerThreads={workerThreads} completThreads={completThreads}");
        System.Threading.ThreadPool.GetMaxThreads(out workerThreads, out completThreads);
        Debug.Log($"max workerThreads={workerThreads} completThreads={completThreads}");

        _msgFactory = new MessageFactory();
        _msgFactory.RegisterProtoBufMessage(1, typeof(UserLoginOn));
        _msgFactory.RegisterXMLStringMessage(2);

        var clientFactory = new ClientFactoryForServer();
        _server = new TcpServerTest(clientFactory, _msgFactory, BufferPool.Default);
        clientFactory.Server = _server;
        _server.Start(IPAddress.Parse(ip), port);

        MessageHandlerUtil.Init(typeof(TcpClientForClient));
        //_client = new TcpClientForClient(_msgFactory, BufferPool.Default);
    }

    [Button]
    public void Connect()
    {
        var client = new TcpClientForClient(_msgFactory, BufferPool.Default);
        _clients.Add(client);
        client.Connect(IPAddress.Parse(ip), port);
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
        _server?.Stop(DisconnectReason.User);

        foreach (var client in _clients)
        {
            client?.Disconnect(DisconnectReason.User, true);
        }
        _clients.Clear();
    }

    public string Convert(int messageId)
    {
        return messageId.ToString();
    }
}


public class TcpClientForClient : TcpClientBase
{
    public TcpClientForClient(MessageFactory factory, BufferPool pool) : base(factory, pool)
    {
    }

    protected override void OnConnected()
    {
        Debug.Log("TcpClientForClient OnConnected");

        var content = new UserLoginOn();
        content.UserName = "abc";
        var msg = GoogleProtocolBufMessage.Allocate(1, content);
        Send(msg);

        var strMsg = StringMessage.Allocate(2);
        strMsg.Append("aaa", "123");
        Send(strMsg);
    }

    protected override void OnDisconnected(DisconnectReason reason)
    {
        Debug.Log("TcpClientForClient OnDisconnected");
    }

    private static int count = 0;

    protected override void OnReceiveMessage(Message msg)
    {
        Debug.Log("TcpClientForClient OnReceiveMessage");

        var content = new UserLoginOn();
        content.UserName = "abc" + count++;
        Send(GoogleProtocolBufMessage.Allocate(1, content));
    }
}

public class TcpClientForServer : TcpClientBase
{
    public TcpServerBase<TcpClientForServer> Server { get; }

    public TcpClientForServer(TcpServerBase<TcpClientForServer> server, TcpClient client) 
        : base(client, server.MessageFactory, server.BufferPool)
    {
        Server = server;
    }

    protected override void OnConnected()
    {
        Debug.LogError("TcpClientForServer OnConnected");
    }

    protected override void OnDisconnected(DisconnectReason reason)
    {
        Debug.Log("TcpClientForServer OnDisconnected");
    }

    private static int count = 0;

    protected override void OnReceiveMessage(Message msg)
    {
        Debug.Log("TcpClientForServer OnReceiveMessage");
        if (msg.MessageId == 1)
        {
            var data = msg.GetData<UserLoginOn>();
            Debug.Log($"UserName={data.UserName}");

            var content = new UserLoginOn();
            content.UserName = "xyz" + count++;
            Send(GoogleProtocolBufMessage.Allocate(1, content));
        }
        if(msg.MessageId == 2)
        {
            var array = msg.GetData<ArraySeg<string>>();
            Debug.Log($"0={array[0]} 1={array[1]}");
        }
    }
}

public class TcpServerTest : TcpServerBase<TcpClientForServer>
{
    public TcpServerTest(ITcpClientFactory<TcpClientForServer> clientFactory, MessageFactory factory, BufferPool pool)
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