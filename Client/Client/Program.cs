using System;
using System.Net;

namespace Client
{
	public class Programm
	{
		static int Port;
		static IPAddress ServerIp;

		static void Main(string[] args)
		{
			Console.WriteLine("Введите ip-адрес сервера");
			ServerIp = IPAddress.Parse(Console.ReadLine());
			Console.WriteLine("Введите порт сервера");
			Port = int.Parse(Console.ReadLine());
			Console.Clear();
			try
			{
				ClientObject Client = new ClientObject();
				Client.Connect(ServerIp, Port);
				Client.Start();
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
			}

		}
	}
}