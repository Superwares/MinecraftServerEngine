using System.Diagnostics;
using System.Net.Sockets;
using System.Net;

namespace Network
{
    public class Listener
    {
        private readonly int _Port;

        public Listener(int port)
        {
            Debug.Assert(port > 0);
            _Port = port;

            var ipEndPoint = new IPEndPoint(IPAddress.Any, port);
            TcpListener listener = new(ipEndPoint);

            try
            {
                listener.Start();

                TcpClient handler = listener.AcceptTcpClient();
                NetworkStream stream = handler.GetStream();

                /*var message = $"📅 {DateTime.Now} 🕛";
                var dateTimeBytes = Encoding.UTF8.GetBytes(message);
                stream.Write(dateTimeBytes);

                Console.WriteLine($"Sent message: \"{message}\"");*/
                // Sample output:
                //     Sent message: "📅 8/22/2022 9:07:17 AM 🕛"
            }
            finally
            {
                listener.Stop();

            }
        }

        public void Init()
        {

        }


        public class McpClient
        {
            public McpClient()
            {

            }
        }
    }

    
}
