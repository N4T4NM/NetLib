using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace NetLib.Udp
{
    public class UdpReceiver
    {
        /// <summary>
        /// Receive bytes at port
        /// </summary>
        /// <param name="port">Port to listen</param>
        /// <param name="handle">Success handler</param>
        /// <param name="error">Error handler</param>
        /// <param name="forever">Receive bytes forever</param>
        public void ReceiveBytesAsync(int port, Action<NetworkMessage> handle, Action<Exception> error, bool forever = false)
        {
            UdpClient client = new UdpClient(port);
            IPEndPoint ip = new IPEndPoint(IPAddress.Any, 0);

            client.BeginReceive(EndReceiveBytes, new object[] { client, ip, handle, error, forever });
        }

        private void EndReceiveBytes(IAsyncResult ar)
        {
            UdpClient client = (ar.AsyncState as object[])[0] as UdpClient;
            IPEndPoint ip = (ar.AsyncState as object[])[1] as IPEndPoint;
            Action<NetworkMessage> handle = (ar.AsyncState as object[])[2] as Action<NetworkMessage>;
            Action<Exception> error = (ar.AsyncState as object[])[3] as Action<Exception>;
            bool forever = (bool)(ar.AsyncState as object[])[4];

            try
            {
                byte[] bytes = client.EndReceive(ar, ref ip);
                NetworkMessage msg = new NetworkMessage()
                {
                    bytes = bytes,
                    sender = ip
                };
                handle?.Invoke(msg);
                if (forever)
                    client.BeginReceive(EndReceiveBytes, new object[] { client, ip, handle, error, forever });
            }
            catch (Exception ex)
            {
                error?.Invoke(ex);
            }
        }

        public struct NetworkMessage
        {
            /// <summary>
            /// Received bytes
            /// </summary>
            public byte[] bytes;
            /// <summary>
            /// Remote end point
            /// </summary>
            public IPEndPoint sender;

            /// <summary>
            /// Convert message to string
            /// </summary>
            /// <returns>Decoded UTF8 string</returns>
            public override string ToString()
            {
                return Encoding.UTF8.GetString(bytes);
            }
        }
    }
}
