using Core.Framework.Network;
using Core.Framework.Network.Buffers;
using Core.Framework.Network.Data;
using Core.Framework.Utility;
using P5.Protobuf;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public class TcpTest : MonoBehaviour, IMessageNameConverter
{
    public string ip;
    public int port;

    private TcpClietTest _client;
    private TcpServerTest _server;
    private MessageFactory _msgFactory;

    void Start()
    {
        MessageNameConverter.Converter = this;

        _msgFactory = new MessageFactory();
        _msgFactory.RegisterProtoBufMessage(1, typeof(UserLoginOn));
        _msgFactory.RegisterXMLStringMessage(2);

        var clientFactory = new ClientForServerFactory();
        clientFactory.Factory = _msgFactory;
        clientFactory.Pool = ContentBufferPool.Default;
        _server = new TcpServerTest(clientFactory, _msgFactory, ContentBufferPool.Default);
        clientFactory.Server = _server;
        _server.Start(IPAddress.Parse(ip), port);

        _client = new TcpClietTest(_msgFactory, ContentBufferPool.Default);
    }

    public void Connect()
    {
        if (_client != null)
            _client.Disconnect(DisconnectReason.User);
        _client.Connect(IPAddress.Parse(ip), port);
    }

    // Update is called once per frame
    void Update()
    {
        _server?.MainLoop();
        _client?.MainLoop();
    }

    private void OnDestroy()
    {
        _server?.Stop(DisconnectReason.User);
        _client?.Disconnect(DisconnectReason.User);
    }

    public string Convert(int messageId)
    {
        return messageId.ToString();
    }
}


public class TcpClietTest : TcpClientBase
{
    public TcpClietTest(MessageFactory factory, ContentBufferPool pool) : base(factory, pool)
    {
    }

    protected override void OnConnected()
    {
        Debug.Log("TcpClietTest OnConnected");

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
    }

    protected override void OnReceiveMessage(Message msg)
    {
    }
}

public class TcpClientForServerTest : TcpClientBase
{
    public TcpServerBase Server { get; }

    public TcpClientForServerTest(TcpServerBase server, TcpClient client, MessageFactory factory, ContentBufferPool pool) 
        : base(client, factory, pool)
    {
        Server = server;
    }

    protected override void OnConnected()
    {
        Debug.LogError("TcpClientForServerTest OnConnected");
    }

    protected override void OnDisconnected(DisconnectReason reason)
    {
    }

    protected override void OnReceiveMessage(Message msg)
    {
        if(msg.MessageId == 1)
        {
            var data = msg.GetData<UserLoginOn>();
            Debug.Log($"UserName={data.UserName}");
        }
        if(msg.MessageId == 2)
        {
            var array = msg.GetData<ArraySeg<string>>();
            Debug.Log($"0={array[0]} 1={array[1]}");
        }
    }
}

public class TcpServerTest : TcpServerBase
{
    public TcpServerTest(ITcpClientFactory clientFactory, MessageFactory factory, ContentBufferPool pool)
        : base(clientFactory, factory, pool)
    {
    }
}

public class ClientForServerFactory : ITcpClientFactory
{
    public TcpServerBase Server { get; set; }
    public MessageFactory Factory { get; set; }
    public ContentBufferPool Pool { get; set; }

    public TcpClientBase Create(TcpClient client)
    {
        return new TcpClientForServerTest(Server, client, Factory, Pool);
    }
}