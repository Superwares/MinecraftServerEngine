

namespace MinecraftServerEngine.Protocols
{
    internal static class SocketMethods
    {
        internal static int GetLocalPort(System.Net.Sockets.Socket socket)
        {
            System.Diagnostics.Debug.Assert(socket != null);

            System.Net.IPEndPoint localEndPoint = (System.Net.IPEndPoint)socket.LocalEndPoint;
            System.Diagnostics.Debug.Assert(localEndPoint != null);
            return localEndPoint.Port;
        }

        internal static System.Net.Sockets.Socket Establish(ushort port)
        {
            System.Net.Sockets.Socket socket = new(
                System.Net.Sockets.SocketType.Stream,
                System.Net.Sockets.ProtocolType.Tcp);

            System.Net.IPEndPoint localEndPoint = new(System.Net.IPAddress.Any, port);
            socket.Bind(localEndPoint);
            socket.Listen();

            return socket;
        }

        /// <exception cref="TryAgainException"></exception>
        internal static System.Net.Sockets.Socket Accept(System.Net.Sockets.Socket socket)
        {
            System.Diagnostics.Debug.Assert(socket != null);

            try
            {
                System.Net.Sockets.Socket newSocket = socket.Accept();

                return newSocket;
            }
            catch (System.Net.Sockets.SocketException e)
            {
                if (e.SocketErrorCode == System.Net.Sockets.SocketError.WouldBlock)
                {
                    throw new TryAgainException();
                }

                throw;
            }

            /*System.Diagnostics.Debug.Assert(false);*/
        }

        internal static bool Poll(System.Net.Sockets.Socket socket, System.TimeSpan span)
        {
            System.Diagnostics.Debug.Assert(socket != null);

            // TODO: check the socket is binding and listening.

            if (IsBlocking(socket) &&
                socket.Poll(span, System.Net.Sockets.SelectMode.SelectRead) == false)
            {
                return false;
            }

            return true;
        }

        internal static void SetBlocking(
            System.Net.Sockets.Socket socket, bool f)
        {
            System.Diagnostics.Debug.Assert(socket != null);

            socket.Blocking = f;
        }

        internal static bool IsBlocking(
            System.Net.Sockets.Socket socket)
        {
            System.Diagnostics.Debug.Assert(socket != null);

            return socket.Blocking;
        }

        /// <exception cref="DisconnectedClientException"></exception>
        /// <exception cref="TryAgainException"></exception>
        internal static int RecvBytes(
            System.Net.Sockets.Socket socket,
            byte[] data, int offset, int size)
        {
            System.Diagnostics.Debug.Assert(socket != null);

            if (data == null || data.Length == 0)
            {
                System.Diagnostics.Debug.Assert(offset == 0);
                System.Diagnostics.Debug.Assert(size == 0);
                return 0;
            }

            System.Diagnostics.Debug.Assert(offset <= data.Length);
            System.Diagnostics.Debug.Assert(offset >= 0);
            System.Diagnostics.Debug.Assert(offset + size <= data.Length);
            System.Diagnostics.Debug.Assert(size >= 0);
            if (size == 0)
            {
                return 0;
            }

            System.Diagnostics.Debug.Assert(data != null);
            System.Diagnostics.Debug.Assert(data.Length > 0);

            System.Diagnostics.Debug.Assert(offset <= data.Length);
            System.Diagnostics.Debug.Assert(offset >= 0);

            System.Diagnostics.Debug.Assert(offset + size <= data.Length);
            System.Diagnostics.Debug.Assert(size >= 0);

            try
            {
                int n = socket.Receive(data, offset, size, System.Net.Sockets.SocketFlags.None);
                if (n == 0)
                {
                    throw new DisconnectedClientException();
                }


                System.Diagnostics.Debug.Assert(n <= size);
                return n;
            }
            catch (System.Net.Sockets.SocketException e)
            when (e.SocketErrorCode == System.Net.Sockets.SocketError.WouldBlock)
            {
                //throw new TryAgainException();
                System.Diagnostics.Debug.Assert(data != null);
                //System.Diagnostics.Debug.Assert(size == 1);
                return 0;
            }
            catch (System.Net.Sockets.SocketException e)
            when (e.SocketErrorCode == System.Net.Sockets.SocketError.ConnectionAborted)
            {
                throw new DisconnectedClientException();
            }
            catch (System.Net.Sockets.SocketException e)
            when (e.SocketErrorCode == System.Net.Sockets.SocketError.ConnectionReset)
            {
                throw new DisconnectedClientException();
            }
            catch (System.Net.Sockets.SocketException)
            {
                throw;
            }

        }

        /// <exception cref="DisconnectedClientException"></exception>
        /// <exception cref="TryAgainException"></exception>
        //internal static byte RecvByte(System.Net.Sockets.Socket socket)
        //{
        //    System.Diagnostics.Debug.Assert(socket != null);

        //    byte[] buffer = new byte[1];

        //    int n = RecvBytes(socket, buffer, 0, 1);
        //    System.Diagnostics.Debug.Assert(n == 1);

        //    return buffer[0];
        //}

        /// <exception cref="DisconnectedClientException"></exception>
        /// <exception cref="TryAgainException"></exception>
        internal static int SendBytes(
            System.Net.Sockets.Socket socket,
            byte[] data, int offset, int size)
        {
            System.Diagnostics.Debug.Assert(socket != null);

            if (data == null || data.Length == 0)
            {
                System.Diagnostics.Debug.Assert(offset == 0);
                System.Diagnostics.Debug.Assert(size == 0);
                return 0;
            }

            System.Diagnostics.Debug.Assert(data != null);

            System.Diagnostics.Debug.Assert(offset <= data.Length);
            System.Diagnostics.Debug.Assert(offset >= 0);

            System.Diagnostics.Debug.Assert(offset + size == data.Length);
            System.Diagnostics.Debug.Assert(size >= 0);

            try
            {

                // For debug, For debug, randomly adjust the size
                //if ((new System.Random()).Next(0, 10) == 0)
                //{
                //    const int _offset = 1;
                //    int minValue = System.Math.Max(0, size - _offset);

                //    size = (new System.Random()).Next(minValue, size + 1);
                //}

                System.Diagnostics.Debug.Assert(data != null);
                int n = socket.Send(data, offset, size, System.Net.Sockets.SocketFlags.None);

                System.Diagnostics.Debug.Assert(n >= 0);
                System.Diagnostics.Debug.Assert(n <= size);
                return n;
            }
            catch (System.Net.Sockets.SocketException e)
            when (e.SocketErrorCode == System.Net.Sockets.SocketError.WouldBlock)
            {
                //throw new TryAgainException();
                System.Diagnostics.Debug.Assert(data != null);
                //System.Diagnostics.Debug.Assert(data.Length == 1);
                return 0;
            }
            catch (System.Net.Sockets.SocketException e)
            when (e.SocketErrorCode == System.Net.Sockets.SocketError.ConnectionAborted)
            {
                throw new DisconnectedClientException();
            }
            catch (System.Net.Sockets.SocketException e)
            when (e.SocketErrorCode == System.Net.Sockets.SocketError.ConnectionReset)
            {
                throw new DisconnectedClientException();
            }
            catch (System.Net.Sockets.SocketException)
            {
                throw;
            }

        }

        /// <exception cref="DisconnectedClientException"></exception>
        /// <exception cref="TryAgainException"></exception>
        //internal static int SendByte(System.Net.Sockets.Socket socket, byte v)
        //{
        //    System.Diagnostics.Debug.Assert(socket != null);

        //    return SendBytes(socket, [v]);
        //}

    }

}
