using System.Net.Sockets;
using System.Net;
using System.Net.Security;
using System;
using System.Security.Cryptography.X509Certificates;
using System.Security.Authentication;
using System.Collections.Generic;

namespace stekelvarken.xmpproxy
{
    public class ServerConn
    {
        protected string _host;

        protected Socket _socket;
        protected NetworkStream _networkStream = null;
        protected SslStream _sslStream = null;
        protected bool _bUseTls = false;
        public myList<byte> _incomingMessages = new myList<byte>();

        public ServerConn(string host)
        {
            _host = host;
        }

        public bool Connect()
        {
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _socket.Connect(_host, 5222);
            //Thread clientThread = new Thread(new ParameterizedThreadStart(ListenForServer));
            //clientThread.Start(client);

            return true;
        }

        public void Send(myList<byte> message)
        {
            if (_bUseTls == false)
            {
                _socket.Send(message.ToArray());
            }
            else
            {
                _sslStream.Write(message.ToArray());
            }
        }

        protected void Recv()
        {
            byte[] message = new byte[4096];
            int bytesRead = 0;

            if (_bUseTls == false)
            {
                bytesRead = _socket.Receive(message);
            }
            else
            {
                bytesRead = _sslStream.Read(message, 0, message.Length);
            }
            if (bytesRead > 0)
            {
                _incomingMessages.AddRange(message, bytesRead);
            }
        }

        public void StartTls()
        {
            _bUseTls = true;
            if (_networkStream == null)
            {
                _networkStream = new NetworkStream(_socket);
            }

            if (_sslStream != null)
            {
                throw new Exception("m_sslStream is already allocated");
            }

            _sslStream = new SslStream(_networkStream, false);
            _sslStream.AuthenticateAsClient(_host, new X509CertificateCollection(), SslProtocols.Tls, false);
        }
    }
}
