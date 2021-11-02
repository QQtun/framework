using LogUtil;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace Core.Framework.Network
{
    public class TcpAccepter
    {
        private TcpListener _tcpLitener;
        private Queue<TcpClient> _newClients = new Queue<TcpClient>();

        public event Action<TcpClient> OnAccept;

        public void Start(IPAddress ip, int port)
        {
            try
            {
                if (_tcpLitener != null)
                {
                    Stop();
                }

                _tcpLitener = new TcpListener(ip, port);
                _tcpLitener.Start();
                _tcpLitener.BeginAcceptTcpClient(OnAcceptTcpClientAsync, _tcpLitener);
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
            }
        }

        public void Stop()
        {
            if (_tcpLitener != null)
            {
                lock(_tcpLitener)
                {
                    _tcpLitener.Stop();
                    _tcpLitener = null;
                }
            }
        }

        public void MainLoop()
        {
            lock(_newClients)
            {
                while (_newClients.Count > 0)
                {
                    OnAccept?.Invoke(_newClients.Dequeue());
                }
            }
        }

        private void OnAcceptTcpClientAsync(IAsyncResult ar)
        {
            try
            {
                var tcpLitener = (TcpListener)ar.AsyncState;
                var newTcpClient = tcpLitener.EndAcceptTcpClient(ar);
                tcpLitener.BeginAcceptTcpClient(OnAcceptTcpClientAsync, tcpLitener);

                lock(_newClients)
                {
                    _newClients.Enqueue(newTcpClient);
                }
            }
            catch (ObjectDisposedException ex)
            {
                Debug.Log(ex);
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                Stop();
            }
        }
    }
}
