using System.Net.Sockets;

namespace NetLib.Tcp
{
    public class TcpClient : TcpNetwork
    {
        /// <summary>
        /// Create new Socket object and connect to remote server
        /// </summary>
        /// <param name="host">Target host</param>
        /// <param name="port">Target port</param>
        public void Connect(string host, int port)
        {
            Socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
            Socket.Connect(host, port);
        }
    }
}
