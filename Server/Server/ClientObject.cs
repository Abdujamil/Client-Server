using System;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Server
{
    class ClientObject
    {
        public string Id { get; private set; }

        public int ClientNumber { get; set; }

        public NetworkStream Stream { get; private set; }

        private readonly TcpClient _tcpClient;
        private readonly ServerObject _serverObject;

        public ClientObject(TcpClient tcpClient, ServerObject serverObject)
        {
            Id = Guid.NewGuid().ToString();
            _tcpClient = tcpClient;
            _serverObject = serverObject;
            serverObject.AddClent(this);
        }

        public void Process()
        {
            try
            {
                Stream = _tcpClient.GetStream();
                Console.WriteLine($"Клиент с номером потока {ClientNumber} подключился");

                while (true)
                {
                    try
                    {
                        if (ClientNumber <= 4)
                        {
                            var message = GetMessage();
                            if (message == null)
                                continue;

                            if (!_serverObject.IsServerBusy(Id, message))
                            {
                                var processingMessage = message;
                                ProcessingMessageAsync(processingMessage);
                            }
                            else
                            {
                                var attention = "Сервер не закончил обработку предыдущего запроса!";
                                SendMessage(attention);
                            }
                        }
                        else
                        {
                            GetMessage();
                            string builder = "Превышен лимит потоков, попробуйте позже";
                            SendMessage(builder);
                        }
                    }
                    catch
                    {
                        string message = String.Format($"Клиент с номером потока {ClientNumber} отключился");
                        Console.WriteLine(message);
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {
                _serverObject.RemoveClient(Id);
                Close();
            }
        }

        private async void ProcessingMessageAsync(string message)
        {
            if (!_serverObject.DupletCheck(Id, message))
            {
                await Task.Run(() =>
                {
                    _serverObject.AddMessage(Id, message);
                    Thread.Sleep(RequestProcessingTime);

                    var messageWithValue = "Число " + message.ToString() + " обработано!";
                    SendMessage(messageWithValue);
                    Console.WriteLine($"Номер клиента {ClientNumber}, " + DateTime.Now.ToShortTimeString() + ": " + messageWithValue);

                    var listId = _serverObject.DupletMessages.Select(m => m.Item1);
                    var clientsId = listId.Distinct();
                    foreach (var id in clientsId)
                    {
                        var client = _serverObject.clients.FirstOrDefault(c => c.Id == id);
                        client.SendMessage(messageWithValue);
                        Console.WriteLine($"Номер клиента {client.ClientNumber}, " + DateTime.Now.ToShortTimeString() + ": " + messageWithValue);
                    }
                    _serverObject.RemoveMessage(Id, message);
                    _serverObject.ClearDuplet(message);
                });
            }
            else
            {
                _serverObject.AddDupletMessage(Id, message);
                var attention = "Такой запрос уже обрабатывается!";
                SendMessage(attention);
            }
        }

        private string GetMessage()
        {
            if (!Stream.CanRead)
                return null;

            StringBuilder stringBuilder = new StringBuilder();
            do
            {
                byte[] data = new byte[256];
                var bytes = Stream.Read(data, 0, data.Length);
                stringBuilder.Append(Encoding.Unicode.GetString(data, 0, bytes));
            }
            while (Stream.DataAvailable);

            return stringBuilder.ToString();
        }
        private void SendMessage(string message)
        {
            if (!Stream.CanWrite)
                return;

            var data = Encoding.Unicode.GetBytes(message);
            Stream.Write(data, 0, data.Length);
        }

        protected internal void Close()
        {
            if (Stream != null)
                Stream.Close();
            if (_tcpClient != null)
                _tcpClient.Close();
        }

        private const int RequestProcessingTime = 7000;
    }
}
