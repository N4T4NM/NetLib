using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace NetLib.Tcp
{
    public abstract class TcpNetwork
    {
        /// <summary>
        /// Current Socket object
        /// </summary>
        public Socket Socket { get; set; }

        /// <summary>
        /// Send array of bytes through current socket
        /// </summary>
        /// <param name="bytes">Data to send</param>
        /// <param name="handle">Success handler</param>
        /// <param name="error">Error handler</param>
        public virtual void SendBytesAsync(byte[] bytes, Action handle, Action<Exception> error)
        {
            try
            {
                Socket.BeginSend(bytes, 0, bytes.Length, SocketFlags.None, EndBytesSending, new object[] { handle, error });
            }
            catch (Exception ex)
            {
                error?.Invoke(ex);
            }
        }

        private void EndBytesSending(IAsyncResult ar)
        {
            Action handle = (ar.AsyncState as object[])[0] as Action;
            Action<Exception> error = (ar.AsyncState as object[])[1] as Action<Exception>;
            try
            {
                Socket.EndSend(ar);
                handle?.Invoke();
            }
            catch (Exception ex)
            {
                error?.Invoke(ex);
            }
        }

        /// <summary>
        /// Receive array of bytes from current socket
        /// </summary>
        /// <param name="handle">Success handler</param>
        /// <param name="error">Error handler</param>
        /// <param name="forever">If true, the socket will always receive all bytes</param>
        public virtual void ReceiveBytesAsync(Action<NetworkMessage> handle, Action<Exception> error, bool forever=false)
        {
            try
            {
                byte[] buffer = new byte[1024];
                MemoryStream memory = new MemoryStream();
                Socket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, EndBytesReceiving,
                    new object[] { buffer, memory, handle, error, forever });
            }
            catch (Exception ex)
            {
                error?.Invoke(ex);
            }
        }

        private void EndBytesReceiving(IAsyncResult ar)
        {
            byte[] buffer = (ar.AsyncState as object[])[0] as byte[];
            MemoryStream memory = (ar.AsyncState as object[])[1] as MemoryStream;
            Action<NetworkMessage> handle = (ar.AsyncState as object[])[2] as Action<NetworkMessage>;
            Action<Exception> error = (ar.AsyncState as object[])[3] as Action<Exception>;
            bool forever = (bool)(ar.AsyncState as object[])[4];
            try
            {
                int sz = Socket.EndReceive(ar);
                memory.Write(buffer, 0, sz);
                if (sz > 1000 && Socket.Available > 0)
                    Socket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, EndBytesReceiving,
                    new object[] { buffer, memory, handle, error, forever });
                else
                {
                    if (memory.ToArray().Length == 0)
                    {
                        error?.Invoke(new Exception("Null bytes"));
                        memory.Close();
                        memory.Dispose();
                        this.Close();
                    }
                    else
                    {
                        NetworkMessage msg = new NetworkMessage()
                        {
                            bytes = memory.ToArray(),
                            client = this
                        };
                        memory.Close();
                        memory.Dispose();

                        handle?.Invoke(msg);
                    }
                    if(forever)
                    {
                        buffer = new byte[1024];
                        memory = new MemoryStream();
                        Socket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, EndBytesReceiving,
                        new object[] { buffer, memory, handle, error, forever });
                    }
                }
            }
            catch (Exception ex)
            {
                memory.Close();
                memory.Dispose();

                error?.Invoke(ex);
            }
        }

        /// <summary>
        /// Close and dispose current socket object
        /// </summary>
        public virtual void Close()
        {
            Socket.Close();
            Socket.Dispose();
        }

        /// <summary>
        /// Get string address of Socket object
        /// </summary>
        /// <returns>Socket remote address</returns>
        public override string ToString()
        {
            IPEndPoint endPoint = this.Socket.RemoteEndPoint as IPEndPoint;
            string address = endPoint.Address.ToString();
            return address.Substring(address.LastIndexOf(':') + 1);
        }

        public struct NetworkMessage
        {
            public byte[] bytes;
            public object client;

            /// <summary>
            /// Get string from byte array
            /// </summary>
            /// <returns>Converted UTF8 string</returns>
            public override string ToString()
            {
                return Encoding.UTF8.GetString(bytes);
            }
        }
    }
}
