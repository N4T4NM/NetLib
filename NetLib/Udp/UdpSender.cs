using System;
using System.Net;
using System.Net.Sockets;

namespace NetLib.Udp
{
    public class UdpSender
    {
        /// <summary>
        /// Send bytes to target host and port
        /// </summary>
        /// <param name="bytes">Bytes to send</param>
        /// <param name="host">Target host</param>
        /// <param name="port">Target port</param>
        /// <param name="handle">Success handler</param>
        /// <param name="error">Error handler</param>
        public void SendBytesAsync(byte[] bytes, string host, int port, Action handle, Action<Exception> error)
        {
            UdpClient client = new UdpClient();
            client.BeginSend(bytes, bytes.Length, host, port, EndSendBytes, new object[] { client, handle, error });
        }

        private void EndSendBytes(IAsyncResult ar)
        {
            UdpClient client = (ar.AsyncState as object[])[0] as UdpClient;
            Action handle = (ar.AsyncState as object[])[1] as Action;
            Action<Exception> error = (ar.AsyncState as object[])[2] as Action<Exception>;

            try
            {
                client.EndSend(ar);
                client.Close();
                client.Dispose();
                handle?.Invoke();
            }
            catch (Exception ex)
            {
                error?.Invoke(ex);
            }
        }

        /// <summary>
        /// Broadcast bytes through network
        /// </summary>
        /// <param name="bytes">Bytes to broadcast</param>
        /// <param name="port">Target port</param>
        /// <param name="handle">Sucess handler </param>
        /// <param name="error">Error handler</param>
        public void BroadcastBytesAsync(byte[] bytes, int port, Action handle, Action<Exception> error)
        {
            UdpClient client = new UdpClient();
            client.EnableBroadcast = true;           
            client.BeginSend(bytes, bytes.Length, new IPEndPoint(IPAddress.Broadcast, port), EndBroadcastBytes, new object[] { client, bytes, handle, error });
        }

        private void EndBroadcastBytes(IAsyncResult ar)
        {
            UdpClient client = (ar.AsyncState as object[])[0] as UdpClient;
            byte[] bytes = (ar.AsyncState as object[])[1] as byte[];
            Action handle = (ar.AsyncState as object[])[2] as Action;
            Action<Exception> error = (ar.AsyncState as object[])[3] as Action<Exception>;
            try
            {
                client.EndSend(ar);
                client.Close();
                client.Dispose();
                handle?.Invoke();
            }
            catch (Exception ex)
            {
                error?.Invoke(ex);
            }
        }
    }
}
