using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Client
{
    class ClientObject
    {
        private const int TimeBetweenRequests = 1600;

        private TcpClient Client;
        private NetworkStream Stream;
        private Random rnd = new Random();

        public void Connect(IPAddress serverIp, int port)
        {
            Client = new TcpClient(serverIp.ToString(), port);
            Stream = Client.GetStream();
        }
        public void Start()
        {

            Thread SendThread = new Thread(new ThreadStart(SendMessage));
            SendThread.Start();
            Thread GetThread = new Thread(new ThreadStart(GetMessageFromServer));
            GetThread.Start();
        }
        public void SendMessage()
        {
            while (true)
            {
                var message = (rnd.Next(10) + 10).ToString();
                byte[] data = Encoding.Unicode.GetBytes(message);
                Stream.Write(data);
                Console.WriteLine("Клиент: " + message);
                Thread.Sleep(TimeBetweenRequests);
            }
        }
        public void GetMessageFromServer()
        {
            StringBuilder builder = new StringBuilder();
            byte[] data = new byte[256];
            try
            {
                do
                {
                    builder.Clear();
                    var bytes = Stream.Read(data, 0, data.Length);
                    builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
                    Console.WriteLine("Сервер: " + builder.ToString());
                }
                while (true);
            }
            catch (Exception ex)
            {
                Console.WriteLine("На данный момент сервер отключен - {0}", ex);
            }
        }
    }
}
