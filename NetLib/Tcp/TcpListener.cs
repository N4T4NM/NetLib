using System;
using System.Net;
using System.Net.Sockets;

namespace NetLib.Tcp
{
    public class TcpListener : TcpNetwork
    {
        /// <summary>
        /// Create new socket, bind to desired port and set listening
        /// </summary>
        /// <param name="port">Port to listen</param>
        public void BindAndListen(int port)
        {
            Socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
            Socket.Bind(new IPEndPoint(IPAddress.Any, port));
            Socket.Listen(100);
        }

        /// <summary>
        /// Create new socket, bind to desired address/port and set listening
        /// </summary>
        /// <param name="address">Address to listen</param>
        /// <param name="port">Port to listen</param>
        public void BindAndListen(IPAddress address, int port)
        {
            Socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
            Socket.Bind(new IPEndPoint(address, port));
            Socket.Listen(100);
        }

        /// <summary>
        /// Receive connection from current socket (the current socket must be already listening)
        /// </summary>
        /// <param name="handle">Success handler</param>
        /// <param name="error">Error handler</param>
        /// <param name="forever">If true, the listener will always accept new connections</param>
        public void ReceiveConnectionAsync(Action<TcpClient> handle, Action<Exception> error, bool forever=false)
        {
            try
            {
                Socket.BeginAccept(EndAcceptTcpClient, new object[] { handle, error, forever });
            } catch(Exception ex)
            {
                error?.Invoke(ex);
            }
        }

        private void EndAcceptTcpClient(IAsyncResult ar)
        {
            Action<TcpClient> handle = (ar.AsyncState as object[])[0] as Action<TcpClient>;
            Action<Exception> error = (ar.AsyncState as object[])[1] as Action<Exception>;
            bool forever = (bool)(ar.AsyncState as object[])[2];

            try
            {
                Socket socket = Socket.EndAccept(ar);
                TcpClient TcpClient = new TcpClient() { Socket = socket };
                handle?.Invoke(TcpClient);

                if(forever)
                    Socket.BeginAccept(EndAcceptTcpClient, new object[] { handle, error, forever });
            } catch(Exception ex)
            {
                error?.Invoke(ex);
            }
        }
    }
}
