
namespace MinecraftServerEngine.Protocols
{
    internal sealed class MinecraftClient : System.IDisposable
    {
        private bool _disposed = false;

        private const int MaxTyAgainHitCount = 99;
        private int _tryAgainHitCount = 0;

        private const byte SegmentBits = 0x7F;
        private const byte ContinueBit = 0x80;

        private int _sizeRecv = 0, _offsetRecv = 0;
        private byte[] _dataRecv = null;

        private int _presizeSend = 0, _sizeSend = 0, _offsetSend = 0;
        private byte[] _dataSend = null;

        private readonly System.Net.Sockets.Socket Socket;

        internal int LocalPort => SocketMethods.GetLocalPort(Socket);

        /// <exception cref="TryAgainException"></exception>
        internal static MinecraftClient Accept(System.Net.Sockets.Socket socket)
        {
            //TODO: Check the socket is Binding and listening correctly.

            System.Net.Sockets.Socket newSocket = SocketMethods.Accept(socket);
            SocketMethods.SetBlocking(newSocket, false);

            /*Console.WriteLine($"socket: {socket.LocalEndPoint}");*/


            return new(newSocket);
        }

        private MinecraftClient(System.Net.Sockets.Socket socket)
        {
            System.Diagnostics.Debug.Assert(SocketMethods.IsBlocking(socket) == false);
            Socket = socket;
        }

        ~MinecraftClient()
        {
            System.Diagnostics.Debug.Assert(false);

            //Dispose(false);
        }



        /// <exception cref="UnexpectedClientBehaviorExecption"></exception>
        /// <exception cref="DisconnectedClientException"></exception>
        /// <exception cref="TryAgainException"></exception>
        private int RecvSize()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(SocketMethods.IsBlocking(Socket) == false);

            System.Diagnostics.Debug.Assert(_sizeRecv >= 0);
            System.Diagnostics.Debug.Assert(_offsetRecv >= 0);

            byte v;
            int n;
            byte[] data = new byte[1];

            while (true)
            {
                n = SocketMethods.RecvBytes(Socket, data, 0, 1);
                System.Diagnostics.Debug.Assert(n <= 1);

                if (n == 0)
                {
                    throw new TryAgainException();
                }

                v = data[0];  // TODO: Refactoring: only using byte v and its pointer to pass to the RecvBytes by array...

                _sizeRecv |= (v & SegmentBits) << _offsetRecv;
                if ((v & ContinueBit) == 0)
                {
                    break;
                }

                _offsetRecv += 7;

                if (_offsetRecv >= 32)
                {
                    throw new InvalidEncodingException();
                }

                System.Diagnostics.Debug.Assert(_offsetRecv > 0);
            }


            return _sizeRecv;
        }

        /// <exception cref="DisconnectedClientException"></exception>
        /// <exception cref="TryAgainException"></exception>
        private void SendSize(int size)
        {
            System.Diagnostics.Debug.Assert(_disposed == false);

            _presizeSend = size;



            byte value;
            int n;

            while (true)
            {

                if ((_presizeSend & ~SegmentBits) == 0)
                {
                    value = (byte)(_presizeSend & SegmentBits);
                    n = SocketMethods.SendBytes(Socket, [value], 0, 1);

                    System.Diagnostics.Debug.Assert(n <= 1);
                    if (n == 0)
                    {
                        throw new TryAgainException();
                    }

                    break;
                }

                value = (byte)(_presizeSend & SegmentBits | ContinueBit);
                System.Diagnostics.Debug.Assert(((uint)255 ^ (byte)0b_11111111U) == 0);  // TODO: ?

                n = SocketMethods.SendBytes(Socket, [value], 0, 1);

                System.Diagnostics.Debug.Assert(n <= 1);
                if (n == 0)
                {
                    throw new TryAgainException();
                }

                _presizeSend >>= 7;
            }

            System.Diagnostics.Debug.Assert(_presizeSend >= 0);
            _presizeSend = 0;
        }

        /// <exception cref="UnexpectedClientBehaviorExecption"></exception>
        /// <exception cref="DisconnectedClientException"></exception>
        /// <exception cref="TryAgainException"></exception>
        internal void Recv(MinecraftProtocolDataStream buffer)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(SocketMethods.IsBlocking(Socket) == false);

            try
            {
                if (_dataRecv == null)
                {
                    _sizeRecv = RecvSize();
                    _offsetRecv = 0;

                    if (_sizeRecv == 0)
                    {
                        return;
                    }

                    System.Diagnostics.Debug.Assert(_dataRecv == null);
                    System.Diagnostics.Debug.Assert(_sizeRecv > 0);

                    // TODO: Pooling: Instead of dynamically allocating new memory, pre-allocate a fixed amount and reuse it.
                    _dataRecv = new byte[_sizeRecv];

                }

                int n = SocketMethods.RecvBytes(Socket, _dataRecv, _offsetRecv, _sizeRecv);
                System.Diagnostics.Debug.Assert(n <= _sizeRecv);

                if (n < _sizeRecv)
                {
                    _sizeRecv -= n;
                    _offsetRecv += n;

                    throw new TryAgainException();
                }

                buffer.WriteData(_dataRecv);

                _sizeRecv = 0;
                _offsetRecv = 0;
                _dataRecv = null;

                _tryAgainHitCount = 0;
            }
            catch (TryAgainException)
            {
                //MyConsole.Debug($"Recv::_tryAgainHitCount: {_tryAgainHitCount}");
                if (MaxTyAgainHitCount < ++_tryAgainHitCount)
                {
                    throw new DataRecvOrSendTimeoutException();
                }

                throw;
            }

        }

        /// <exception cref="DisconnectedClientException"></exception>
        /// <exception cref="TryAgainException"></exception>
        internal void Send(MinecraftProtocolDataStream buffer)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            int n;

            try
            {
                if (_dataSend != null)
                {
                    if (_dataSend.Length > 0)
                    {
                        System.Diagnostics.Debug.Assert(_presizeSend >= 0);
                        if (_presizeSend > 0)
                        {
                            SendSize(_presizeSend);

                            System.Diagnostics.Debug.Assert(_presizeSend == 0);
                            System.Diagnostics.Debug.Assert(_sizeSend == _dataSend.Length);
                            System.Diagnostics.Debug.Assert(_offsetSend == 0);
                        }

                        n = SocketMethods.SendBytes(Socket, _dataSend, _offsetSend, _sizeSend);

                        if (n < _sizeSend)
                        {
                            _sizeSend -= n;
                            _offsetSend += n;

                            throw new TryAgainException();
                        }
                    }
                    else
                    {
                        System.Diagnostics.Debug.Assert(_presizeSend == 0);
                        System.Diagnostics.Debug.Assert(_sizeSend == 0);
                        System.Diagnostics.Debug.Assert(_offsetSend == 0);
                        _dataSend = null;
                    }




                }

                _dataSend = buffer.ReadData();

                System.Diagnostics.Debug.Assert(_dataSend != null);
                if (_dataSend.Length == 0)
                {
                    _presizeSend = 0;
                    _sizeSend = 0;
                    _offsetSend = 0;
                    _dataSend = null;

                    return;
                }

                _sizeSend = _dataSend.Length;
                _offsetSend = 0;

                SendSize(_dataSend.Length);

                System.Diagnostics.Debug.Assert(_presizeSend == 0);

                n = SocketMethods.SendBytes(Socket, _dataSend, 0, _sizeSend);

                System.Diagnostics.Debug.Assert(n <= _sizeSend);
                if (n < _sizeSend)
                {
                    System.Diagnostics.Debug.Assert(_presizeSend == 0);
                    _sizeSend -= n;
                    _offsetSend += n;

                    throw new TryAgainException();
                }

                _presizeSend = 0;
                _sizeSend = 0;
                _offsetSend = 0;
                _dataSend = null;

                _tryAgainHitCount = 0;
            }
            catch (TryAgainException)
            {
                //MyConsole.Debug($"Send::_tryAgainHitCount: {_tryAgainHitCount}");
                if (MaxTyAgainHitCount < ++_tryAgainHitCount)
                {
                    throw new DataRecvOrSendTimeoutException();
                }

                throw;
            }
        }

        public void Dispose()
        {
            // Assertions.
            System.Diagnostics.Debug.Assert(!_disposed);

            // Release resources.
            Socket.Dispose();
            _dataRecv = null;

            // Finish.
            System.GC.SuppressFinalize(this);
            _disposed = true;
        }

    }

}
