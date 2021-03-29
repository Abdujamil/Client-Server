using System;
using System.Net;
using System.Threading;

namespace Server
{
    class Program
    {
        static int port;
        static IPAddress serverip;
        static ServerObject serverObject;
        static Thread listenThread;

        static void Main(string[] args)
        {
            Console.WriteLine("Введите ip-адрес сервера");
            serverip = IPAddress.Parse(Console.ReadLine());
            Console.WriteLine("Введите номер порта для сервера");
            port = int.Parse(Console.ReadLine());
            Console.Clear();
            try
            {
                serverObject = new ServerObject();
                listenThread = new Thread(new ThreadStart(() => serverObject.Listen(serverip, port)));
                listenThread.Start();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                serverObject.Disconnect();
            }
        }
    }
}
