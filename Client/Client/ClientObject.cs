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
        private static string[] randomRequests = new[] { "Рандомный запрос!", "Покажи мое число!?", "Запрашиваю число от 10 до 20!", "Делаю запрос на число от 10 до 20" };

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
				var message = randomRequests[rnd.Next(2)];
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
            catch(Exception ex)
			{
				Console.WriteLine("На данный момент сервер отключен - {0}" , ex);
			}
        }
    }
}
