
using System.Net.Sockets;
using System;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Security.Authentication;
using System.Net;
using System.Threading;
using System.Collections.Generic;
namespace stekelvarken.xmpproxy
{
    public class Connection
    {
        protected string _host = "";

        protected Socket _socket;
        protected NetworkStream _networkStream = null;
        protected SslStream _sslStream = null;
        protected bool _bUseTls = false;
        protected TcpListener _tcpListener;
        protected myList<byte> _incomingMessages = new myList<byte>();
        protected Mutex _messagesMutex = new Mutex();
        protected X509Certificate _cert;

        public Connection()
        {
        }

        public Connection(string host)
        {
            _host = host;
        }

        public void Connect()
        {
            if (_host.Length > 0)
            {
                _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                _socket.Connect(_host, 5222);
                _networkStream = new NetworkStream(_socket);
            }
            else
            {
                _tcpListener = new TcpListener(IPAddress.Any, 16667);
                _tcpListener.Start();
                TcpClient client = _tcpListener.AcceptTcpClient();
                _networkStream = client.GetStream();
            }
            Thread clientThread = new Thread(new ParameterizedThreadStart(HandleClient));
            clientThread.Start();
        }

        protected void HandleClient(object p)
        {
            byte[] message = new byte[4096];
            int bytesRead;
            Console.WriteLine("Got a client");
            while (true)
            {
                bytesRead = Recv();
                Console.WriteLine("Got a message {0}", bytesRead);

                if (bytesRead == 0)
                {
                    //the client has disconnected from the server
                    break;
                }
            }
        }

        protected int Recv()
        {
            byte[] message = new byte[4096];
            int bytesRead = 0;

            if (_bUseTls == false)
            {
                bytesRead = _networkStream.Read(message, 0, message.Length);
            }
            else
            {
                bytesRead = _sslStream.Read(message, 0, message.Length);
            }
            if (bytesRead > 0)
            {
                if (_messagesMutex.WaitOne(1000) == true)
                {
                    _incomingMessages.AddRange(message, bytesRead);
                    _messagesMutex.ReleaseMutex();
                }
            }
            return bytesRead;
        }

        public void Send(myList<byte> message)
        {
            if (_bUseTls == false)
            {
                _networkStream.Write(message.ToArray(), 0, message.Count);
            }
            else
            {
                _sslStream.Write(message.ToArray());
            }
        }

        public myList<byte> GetMessages()
        {
            myList<byte> ret = null;
            if (_messagesMutex.WaitOne(1000) == true)
            {
                if (_incomingMessages.Count > 0)
                {
                    ret = new myList<byte>(_incomingMessages);
                    _incomingMessages.Clear();
                }
                _messagesMutex.ReleaseMutex();
            }
            return ret;
        }

        public void StartTls()
        {
            if (_sslStream != null)
            {
                throw new Exception("m_sslStream is already allocated");
            }

            if (_host.Length > 0)
            {
                _sslStream = new SslStream(_networkStream, false);
                _sslStream.AuthenticateAsClient(_host, new X509CertificateCollection(), SslProtocols.Tls, false);
            }
            else
            {
                //_cert = new X509Certificate2(@"C:\dl\sw\testcert.pfx", "mypassword");
                //X509Store store = new X509Store(StoreLocation.LocalMachine);
                //store.Open(OpenFlags.ReadOnly);
                //X509Certificate2Collection coll = new X509Certificate2Collection();

                //_cert = store.Certificates[2];
                _cert = X509Certificate.CreateFromCertFile(@"C:\dl\sw\teamlab.cer");

                _sslStream = new SslStream(_networkStream, false, ValidateServerCertificate, ChooseClientCertificate);

                _sslStream.AuthenticateAsServer(_cert, false, SslProtocols.Tls, false);
            }
            _bUseTls = true;
        }

        protected bool ValidateServerCertificate(object sender,
                                         X509Certificate certificate,
                                         X509Chain chain,
                                         SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }

        protected X509Certificate ChooseClientCertificate(object sender,
            string targetHost,
            X509CertificateCollection localCertificates,
            X509Certificate remoteCertificate,
            string[] acceptableIssuers)
        {
            //_cert = new X509Certificate2(@"C:\dl\sw\testcert.pfx", "mypassword");
            return _cert;
        }
    }
}
