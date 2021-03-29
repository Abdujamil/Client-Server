using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Server
{
	class ServerObject
	{
		public List<ClientObject> clients;
		private readonly List<Tuple<string, string>> _messages;
		public List<Tuple<string, string>> DupletMessages;

		static TcpListener tcpListener;

		public ServerObject()
		{
			clients = new List<ClientObject>();
			_messages = new List<Tuple<string, string>>();
			DupletMessages = new List<Tuple<string, string>>();
		}

		public void Listen(IPAddress serverIp, int port)
		{
			try
			{
				tcpListener = new TcpListener(serverIp, port);
				tcpListener.Start();
				Console.WriteLine("Ваш сервер создан, ожидается подключение клиентов");
				while (true)
				{
					TcpClient tcpClient = tcpListener.AcceptTcpClient();
					ClientObject clientObject = new ClientObject(tcpClient, this);
					Thread clientThread = new Thread(new ThreadStart(clientObject.Process));
					clientThread.Start();
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
				Disconnect();
			}
		}

		#region client

		public void AddClent(ClientObject clientObject)
		{
			var clientNumber = 0;
			if (clients.Count == 0)
				clientObject.ClientNumber = clientNumber;
			else
			{
				var clientsNumber = clients.OrderBy(c => c.ClientNumber).Select(c => c.ClientNumber).ToArray();
				var clientMaxNumber = clientsNumber.Max();
				if (clientMaxNumber == 0)
				{
					clientObject.ClientNumber = 1;
				}
				else
				{
					for (var i = 0; i <= clientMaxNumber; i++)
					{
						if (clientsNumber[i] == clientNumber)
						{
							clientNumber++;
						}
						else
						{
							break;
						}
					}
					if (clientNumber == clientMaxNumber)
						clientObject.ClientNumber = clientNumber + 1;
					else
					{
						clientObject.ClientNumber = clientNumber;
					}
				}
			}

			clients.Add(clientObject);
		}

		public void RemoveClient(string id)
		{
			ClientObject client = clients.FirstOrDefault(c => c.Id == id);
			if (client != null)
				clients.Remove(client);
		}

		#endregion

		public void AddMessage(string Id, string message)
		{
			_messages.Add(Tuple.Create(Id, message));
		}

		public void AddDupletMessage(string Id, string message)
		{
			DupletMessages.Add(Tuple.Create(Id, message));
		}

		public void RemoveMessage(string Id, string message)
		{
			var messageForRemove = _messages.FirstOrDefault(o => o.Item1 == Id && o.Item2 == message);
			if (messageForRemove == null)
				return;

			_messages.Remove(messageForRemove);
		}
		public bool DupletCheck(string Id, string message)
		{
			if (_messages.Any(m => m.Item2 == message))
			{
				DupletMessages.Add(new Tuple<string, string>(Id, message));
				return true;
			}
			return false;
		}

		public void ClearDuplet(string message)
		{
			DupletMessages = DupletMessages.Where(m => m.Item2 != message).ToList();
		}

		public bool IsServerBusy(string Id, string message)
		{
			return _messages.Any(m => m.Item1 == Id && m.Item2 != message);
		}

		public void Disconnect()
		{
			tcpListener.Stop();

			for (int i = 0; i < clients.Count; i++)
			{
				clients[i].Close();
			}
			Environment.Exit(0);
		}
	}
}
