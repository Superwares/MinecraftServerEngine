
using Common;
using Containers;

namespace MinecraftServerEngine.Protocols
{
    internal sealed class ClientListener : System.IDisposable
    {
        private enum VisitorLevel : int
        {
            Handshake,
            Request,
            Response,
            Ping,
            Pong,
            StartLogin,
        }

        private static readonly System.TimeSpan PendingTimeout = System.TimeSpan.FromSeconds(1);

        private bool _disposed = false;

        private readonly ConnectionListener _ConnectionListener;


        private readonly System.Net.Sockets.Socket _ListenerSocket;  // Disposable

        private readonly Queue<MinecraftClient> _Visitors;  // Disposable
        private readonly Queue<VisitorLevel> _VisitorLevels;  // Disposable


        internal ClientListener(ConnectionListener connListener, ushort port)
        {
            _ConnectionListener = connListener;

            _ListenerSocket = SocketMethods.Establish(port);
            _Visitors = new();
            _VisitorLevels = new();

            SocketMethods.SetBlocking(_ListenerSocket, true);
        }

        ~ClientListener()
        {
            System.Diagnostics.Debug.Assert(false);

            //Dispose(false);
        }

        private int HandleVisitors()
        {
            System.Diagnostics.Debug.Assert(_disposed == false);

            int count = _Visitors.Length;
            System.Diagnostics.Debug.Assert(count == _VisitorLevels.Length);
            if (count == 0) return 0;

            bool close, success;

            for (; count > 0; --count)
            {
                close = success = false;

                MinecraftClient client = _Visitors.Dequeue();
                VisitorLevel level = _VisitorLevels.Dequeue();

                /*Console.WriteLine($"count: {count}, level: {level}");*/

                using MinecraftProtocolDataStream buffer = new();

                try
                {
                    if (level == VisitorLevel.Handshake)
                    {
                        /*Console.WriteLine("Handshake!");*/
                        client.Recv(buffer);

                        int packetId = buffer.ReadInt(true);
                        if (ServerboundHandshakingPacket.SetProtocolPacketId != packetId)
                        {
                            throw new UnexpectedPacketException();
                        }

                        SetProtocolPacket packet = SetProtocolPacket.Read(buffer);

                        if (!buffer.Empty)
                        {
                            throw new BufferOverflowException();
                        }

                        switch (packet.NextState)
                        {
                            default:
                                throw new InvalidEncodingException();
                            case Packet.States.Status:
                                level = VisitorLevel.Request;
                                break;
                            case Packet.States.Login:
                                level = VisitorLevel.StartLogin;
                                break;
                        }

                    }

                    if (level == VisitorLevel.Request)  // Request
                    {
                        /*Console.WriteLine("Request!");*/
                        client.Recv(buffer);

                        int packetId = buffer.ReadInt(true);
                        if (ServerboundStatusPacket.RequestPacketId != packetId)
                        {
                            throw new UnexpectedPacketException();
                        }

                        RequestPacket requestPacket = RequestPacket.Read(buffer);

                        if (!buffer.Empty)
                        {
                            throw new BufferOverflowException();
                        }

                        // TODO
                        ResponsePacket responsePacket = new(100, 10, "Hello, World!");
                        responsePacket.Write(buffer);

                        level = VisitorLevel.Response;
                        client.Send(buffer);

                        level = VisitorLevel.Ping;
                    }

                    if (level == VisitorLevel.Response)
                    {
                        client.Send(buffer);

                        level = VisitorLevel.Ping;
                    }

                    if (level == VisitorLevel.Ping)  // Ping
                    {
                        /*Console.WriteLine("Ping!");*/
                        client.Recv(buffer);

                        int packetId = buffer.ReadInt(true);
                        if (ServerboundStatusPacket.PingPacketId != packetId)
                        {
                            throw new UnexpectedPacketException();
                        }

                        PingPacket inPacket = PingPacket.Read(buffer);

                        if (!buffer.Empty)
                        {
                            throw new BufferOverflowException();
                        }

                        PongPacket outPacket = new(inPacket.Payload);
                        outPacket.Write(buffer);

                        level = VisitorLevel.Pong;
                        client.Send(buffer);
                    }

                    if (level == VisitorLevel.Pong)
                    {
                        client.Send(buffer);
                    }

                    if (level == VisitorLevel.StartLogin)  // Start Login
                    {
                        client.Recv(buffer);

                        int packetId = buffer.ReadInt(true);
                        if (ServerboundLoginPacket.StartLoginPacketId != packetId)
                        {
                            throw new UnexpectedPacketException();
                        }

                        StartLoginPacket inPacket = StartLoginPacket.Read(buffer);

                        if (!buffer.Empty)
                        {
                            throw new BufferOverflowException();
                        }

                        // TODO: Check username is empty or invalid.

                        System.Guid userId = System.Guid.Empty;
                        System.Diagnostics.Debug.Assert(inPacket.Username != null);
                        System.Diagnostics.Debug.Assert(string.IsNullOrEmpty(inPacket.Username) == false);
                        string username = inPacket.Username;

                        try
                        {
                            // TODO: Refactoring

                            // TODO: Use own http client in common library.
                            using System.Net.Http.HttpClient httpClient = new();
                            string url = string.Format(
                                "https://api.mojang.com/users/profiles/minecraft/{0}",
                                username);

                            using System.Net.Http.HttpRequestMessage request = new(System.Net.Http.HttpMethod.Get, url);

                            // TODO: handle HttpRequestException
                            using System.Net.Http.HttpResponseMessage response = httpClient.Send(request);

                            using System.IO.Stream stream = response.Content.ReadAsStream();
                            using System.IO.StreamReader reader = new(stream);
                            string str = reader.ReadToEnd();

                            System.Text.Json.JsonElement jsonResponse = System.Text.Json.JsonDocument.Parse(str).RootElement;

                            if (
                                jsonResponse.TryGetProperty("id", out System.Text.Json.JsonElement idElement) == true &&
                                jsonResponse.TryGetProperty("name", out System.Text.Json.JsonElement nameElement) == true
                                )
                            {
                                userId = System.Guid.Parse(idElement.GetString());
                                username = nameElement.GetString();
                            }
                            else
                            {
                                throw new System.InvalidOperationException("Invalid JSON response: missing 'id' or 'name' property.");
                            }

                            //System.Collections.Generic.Dictionary<string, string> dictionary = System.Text.Json.JsonSerializer.Deserialize<System.Collections.Generic.Dictionary<string, string>>(str);
                            //System.Diagnostics.Debug.Assert(dictionary != null);

                            //userId = System.Guid.Parse(dictionary["id"]);

                            //System.Diagnostics.Debug.Assert(string.Equals(dictionary["name"], username) == true);
                            //username = dictionary["name"];  // TODO: check username is valid

                            // TODO: Handle to throw exception
                            /*System.Diagnostics.Debug.Assert(inPacket.Username == username);*/
                        }
                        catch (System.Exception e)
                        {
                            MyConsole.Warn(e.Message);

                            userId = System.Guid.NewGuid();
                        }

                        //System.Diagnostics.Debug.Assert(userId != System.Guid.Empty);
                        //System.Diagnostics.Debug.Assert(username != null);
                        //System.Diagnostics.Debug.Assert(string.IsNullOrEmpty(username) == false);
                        //LoginSuccessPacket outPacket1 = new(userId, username);
                        //outPacket1.Write(buffer);

                        //client.Send(buffer);

                        UserProperty[] properties = null;

                        try
                        {
                            // TODO: Refactoring

                            using System.Net.Http.HttpClient httpClient = new();
                            string url = string.Format(
                                "https://sessionserver.mojang.com/session/minecraft/profile/{0}?unsigned=false",
                                userId.ToString());

                            using System.Net.Http.HttpRequestMessage request = new(System.Net.Http.HttpMethod.Get, url);

                            // TODO: handle HttpRequestException
                            using System.Net.Http.HttpResponseMessage response = httpClient.Send(request);

                            using System.IO.Stream stream = response.Content.ReadAsStream();
                            using System.IO.StreamReader reader = new(stream);
                            string str = reader.ReadToEnd();
                            //var jsonResponse = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(str);

                            //var propertiesArray = jsonResponse.GetProperty("properties").EnumerateArray();
                            //properties = System.Linq.Enumerable.ToArray(
                            //    System.Linq.Enumerable.Select(propertiesArray,
                            //    prop => new UserProperty(
                            //        prop.GetProperty("name").GetString(),
                            //        prop.GetProperty("value").GetString(),
                            //        prop.GetProperty("signature").GetString()
                            //    )));

                            System.Text.Json.JsonElement jsonResponse = System.Text.Json.JsonDocument.Parse(str).RootElement;

                            if (
                                jsonResponse.TryGetProperty("id", out System.Text.Json.JsonElement idElement) == true &&
                                jsonResponse.TryGetProperty("name", out System.Text.Json.JsonElement nameElement) == true
                                )
                            {
                                //userId = System.Guid.Parse(idElement.GetString());
                                //username = nameElement.GetString();
                            }
                            else
                            {
                                throw new System.InvalidOperationException("Invalid JSON response: missing 'id' or 'name' property.");
                            }

                            System.Text.Json.JsonElement propertiesElement;
                            if (
                                jsonResponse.TryGetProperty("properties", out propertiesElement) == true && 
                                propertiesElement.ValueKind == System.Text.Json.JsonValueKind.Array == true
                                )
                            {
                                System.Text.Json.JsonElement.ArrayEnumerator propertiesArray = propertiesElement.EnumerateArray();

                                properties = System.Linq.Enumerable.ToArray(
                                    System.Linq.Enumerable.Select(propertiesArray,
                                    prop => new UserProperty(
                                        prop.GetProperty("name").GetString(),
                                        prop.GetProperty("value").GetString(),
                                        prop.GetProperty("signature").GetString()
                                    )));
                            }
                            else
                            {
                                properties = System.Array.Empty<UserProperty>();
                            }
                            

                        }
                        catch (System.Exception e)
                        {
                            MyConsole.Warn(e.Message);
                        }

                        User user = new(client, userId, username, properties);

                        // TODO: Must dealloc id when connection is disposed.
                        _ConnectionListener.AddUser(user);

                        success = true;
                    }


                    System.Diagnostics.Debug.Assert(buffer.Size == 0);

                    if (success == false)
                    {
                        close = true;
                    }

                }
                catch (TryAgainException)
                {
                    System.Diagnostics.Debug.Assert(success == false);
                    System.Diagnostics.Debug.Assert(close == false);

                }
                catch (UnexpectedClientBehaviorExecption)
                {
                    System.Diagnostics.Debug.Assert(success == false);
                    System.Diagnostics.Debug.Assert(close == false);

                    close = true;

                    buffer.Flush();

                    if (level >= VisitorLevel.StartLogin)  // >= Start Login level
                    {
                        System.Diagnostics.Debug.Assert(false);
                        // TODO: Handle send Disconnect packet with reason.
                    }

                }
                catch (DisconnectedClientException)
                {
                    System.Diagnostics.Debug.Assert(success == false);
                    System.Diagnostics.Debug.Assert(close == false);
                    close = true;

                    /*Console.Write("~");*/

                    buffer.Flush();

                    /*Console.Write($"EndofFileException");*/
                }

                System.Diagnostics.Debug.Assert(buffer.Empty);

                if (success == false)
                {
                    if (close == false)
                    {
                        _Visitors.Enqueue(client);
                        _VisitorLevels.Enqueue(level);
                    }
                    else
                    {
                        client.Dispose();
                    }

                    continue;
                }

                System.Diagnostics.Debug.Assert(close == false);

            }

            return _Visitors.Length;
        }

        internal void StartRoutine()
        {
            System.Diagnostics.Debug.Assert(_disposed == false);

            /*Console.Print(">");*/

            try
            {
                if (!SocketMethods.IsBlocking(_ListenerSocket) &&
                    HandleVisitors() == 0)
                {
                    SocketMethods.SetBlocking(_ListenerSocket, true);
                }

                if (SocketMethods.Poll(_ListenerSocket, PendingTimeout))
                {
                    MinecraftClient client = MinecraftClient.Accept(_ListenerSocket);
                    _Visitors.Enqueue(client);
                    _VisitorLevels.Enqueue(VisitorLevel.Handshake);

                    SocketMethods.SetBlocking(_ListenerSocket, false);
                }
            }
            catch (TryAgainException)
            {
                /*Console.WriteLine("TryAgainException!");*/
            }


        }

        internal void Flush()
        {
            System.Diagnostics.Debug.Assert(_disposed == false);

            _Visitors.Flush();
            _VisitorLevels.Flush();

            // TODO: Handle client, send Disconnet Packet if the client's step is after StartLogin;

        }

        public void Dispose()
        {
            // Assertions.
            System.Diagnostics.Debug.Assert(_disposed == false);

            System.Diagnostics.Debug.Assert(_Visitors.Empty);
            System.Diagnostics.Debug.Assert(_VisitorLevels.Empty);

            // Release resources.
            _ListenerSocket.Dispose();
            _Visitors.Dispose();
            _VisitorLevels.Dispose();

            // Finish.
            System.GC.SuppressFinalize(this);
            _disposed = true;
        }

    }


}
