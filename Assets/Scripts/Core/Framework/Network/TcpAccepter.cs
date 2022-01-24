using LogUtil;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Core.Framework.Network
{
    public class TcpAccepter
    {
        private class TcpObject
        {
            public bool accepting;
            public TcpListener tcpLitener;
        }

        private class StopState
        {
            public bool invoke;
        }

        private TcpObject _tcpObject = new TcpObject();
        private StopState _stopState = new StopState();
        private Queue<TcpClient> _newClients = new Queue<TcpClient>();

        public event Action<TcpClient> OnAccept;
        public event Action OnStop;

        public void Start(IPAddress ip, int port)
        {
            try
            {
                lock(_tcpObject)
                {
                    Stop(true);
                }

                lock(_newClients)
                {
                    _newClients.Clear();
                }

                lock (_tcpObject)
                {
                    _tcpObject.tcpLitener = new TcpListener(ip, port);
                    _tcpObject.tcpLitener.Start();

                    ThreadPool.QueueUserWorkItem(AcceptTcpClientThreaded, _tcpObject);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
            }
        }

        public void Stop(bool invokeNow = false)
        {
            lock(_tcpObject)
            {
                if (_tcpObject.tcpLitener == null)
                {
                    return;
                }
                _tcpObject.tcpLitener.Stop();
                _tcpObject.tcpLitener = null;
                _tcpObject.accepting = false;
            }

            lock (_newClients)
            {
                _newClients.Clear();
            }

            if (invokeNow)
            {
                OnStop?.Invoke();
            }
            else
            {
                lock(_stopState)
                {
                    _stopState.invoke = true;
                }
            }
        }

        public void MainLoop()
        {
            lock (_newClients)
            {
                while (_newClients.Count > 0)
                {
                    OnAccept?.Invoke(_newClients.Dequeue());
                }
            }
            lock (_stopState)
            {
                if(_stopState.invoke)
                {
                    _stopState.invoke = false;
                    OnStop?.Invoke();
                }
            }
        }

        private void AcceptTcpClientThreaded(object state)
        {
            TcpObject tcpObject = state as TcpObject;
            TcpListener tcpListener;
            lock (tcpObject)
            {
                tcpListener = tcpObject.tcpLitener;
                if (tcpListener == null)
                    return;
                if (tcpObject.accepting)
                    return;

                tcpObject.accepting = true;
            }

            try
            {
                while(true)
                {
                    var newTcpClient = tcpListener.AcceptTcpClient();

                    lock (_newClients)
                    {
                        _newClients.Enqueue(newTcpClient);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                lock (tcpObject)
                {
                    tcpObject.accepting = false;
                }
                TryStop(tcpObject, tcpListener);
            }
        }

        private void TryStop(TcpObject tcpObject, TcpListener tcpClient)
        {
            var needStop = false;
            lock (tcpObject)
            {
                needStop = tcpObject.tcpLitener != null && tcpObject.tcpLitener == tcpClient;
            }
            if (needStop)
                Stop();
        }
    }
}
