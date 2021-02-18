using System;
using System.IO;
using System.IO.Compression;
using System.Net.Sockets;

namespace NetLib.Tcp
{
    public class TcpStream
    {
        /// <summary>
        /// Socket used to create stream
        /// </summary>
        public Socket Socket { get; private set; }
        private NetworkStream stream;

        /// <summary>
        /// Create TcpStream instance
        /// </summary>
        /// <param name="client">Stream source client</param>
        public TcpStream(TcpClient client)
        {
            this.Socket = client.Socket;
            this.stream = new NetworkStream(this.Socket);
        }

        /// <summary>
        /// Read bytes
        /// </summary>
        /// <param name="handle">Success handle</param>
        /// <param name="error">Error handle</param>
        /// <param name="forever">Run forever</param>
        public void ReadAsync(Action<byte[]> handle, Action<Exception> error, bool forever = false)
        {
            try
            {
                byte[] buffer = new byte[1024];
                MemoryStream memory = new MemoryStream();
                this.stream.BeginRead(buffer, 0, buffer.Length, EndReading, 
                    new object[] { buffer, memory, handle, error, forever });
            } catch(Exception ex)
            {
                error?.Invoke(ex);
            }
        }

        private void EndReading(IAsyncResult ar)
        {
            byte[] buffer = (ar.AsyncState as object[])[0] as byte[];
            MemoryStream memory = (ar.AsyncState as object[])[1] as MemoryStream;
            Action<byte[]> handle = (ar.AsyncState as object[])[2] as Action<byte[]>;
            Action<Exception> error = (ar.AsyncState as object[])[3] as Action<Exception>;
            bool forever = (bool)(ar.AsyncState as object[])[4];
            try
            {
                int sz = this.stream.EndRead(ar);
                memory.Write(buffer, 0, sz);
                if (sz > 1000 && this.stream.DataAvailable)
                    this.stream.BeginRead(buffer, 0, buffer.Length, EndReading,
                    new object[] { buffer, memory, handle, error, forever });
                else
                {
                    byte[] data = memory.ToArray();
                    memory.Close();
                    memory.Dispose();

                    handle?.Invoke(data);
                }
            } catch(Exception ex)
            {
                memory.Close();
                memory.Dispose();
                error?.Invoke(ex);
            }
        }

        /// <summary>
        /// Write bytes
        /// </summary>
        /// <param name="bytes">Bytes to write</param>
        /// <param name="handle">Success handler</param>
        /// <param name="error">Error handler</param>
        public void WriteAsync(byte[] bytes, Action handle, Action<Exception> error)
        {
            try
            {
                this.stream.BeginWrite(bytes, 0, bytes.Length, EndWriting,
                    new object[] { handle, error });
            } catch(Exception ex)
            {
                error?.Invoke(ex);
            }
        }

        private void EndWriting(IAsyncResult ar)
        {
            Action handle = (ar.AsyncState as object[])[0] as Action;
            Action<Exception> error = (ar.AsyncState as object[])[1] as Action<Exception>;
            try
            {
                this.stream.EndWrite(ar);
                handle?.Invoke();
            } catch(Exception ex)
            {
                error?.Invoke(ex);
            }
        }

        /// <summary>
        /// Compress byte array
        /// </summary>
        /// <param name="buffer">Bytes to be compressed</param>
        /// <returns>Compressed bytes</returns>
        public byte[] Compress(byte[] buffer)
        {
            MemoryStream comp = new MemoryStream();
            using (DeflateStream def = new DeflateStream(comp, CompressionLevel.Optimal))
                def.Write(buffer, 0, buffer.Length);

            return comp.ToArray();
        }

        /// <summary>
        /// Decompress byte array
        /// </summary>
        /// <param name="buffer">Bytes to be decompressed</param>
        /// <returns>Decompressed bytes</returns>
        public byte[] Decompress(byte[] buffer)
        {
            MemoryStream comp = new MemoryStream(buffer);
            MemoryStream dec = new MemoryStream();
            using (DeflateStream def = new DeflateStream(comp, CompressionMode.Decompress))
                def.CopyTo(dec);

            return dec.ToArray();
        }
    }
}
