using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace Server
{
    class ClientObject
    {
        private const int RequestProcessingTime = 7000;
        public string Id { get; private set; }

        public NetworkStream Stream { get; private set; }

        private readonly TcpClient _tcpClient;
        private readonly ServerObject _serverObject;
        private readonly List<Tuple<string, string>> _dubletMessages;

        public ClientObject(TcpClient tcpClient, ServerObject serverObject)
        {
            Id = Guid.NewGuid().ToString();
            _tcpClient = tcpClient;
            _serverObject = serverObject;
            serverObject.AddClent(this);
            _dubletMessages = new List<Tuple<string, string>>();
        }

        public void Process()
        {
            try
            {
                Stream = _tcpClient.GetStream();
                Console.WriteLine($"Клиент с номером потока {_serverObject.GetClientNumber(Id)} подключился");

                while (true)
                {
                    try
                    {
                        if (_serverObject.GetClientNumber(Id) <= 4)
                        {
                            var message = GetMessage();
                            if (message == null)
                                continue;

                            if (!_serverObject.DupletCheck(Id, message))
                            {
                                if (!_serverObject.IsServerBusy(Id, message))
                                {
                                    _serverObject.AddMessage(Id, message);

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
                                _dubletMessages.Add(Tuple.Create(Id, message));
                                var attention = "Такой запрос уже обрабатывается!";
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
                        string message = String.Format($"Клиент с номером потока {_serverObject.GetClientNumber(Id)} отключился");
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
            await Task.Run(() =>
            {
                Thread.Sleep(RequestProcessingTime);
                var messageWithValue = "Число " + message.ToString() + " обработано!";
                Console.WriteLine($"Номер клиента {_serverObject.GetClientNumber(Id)}, " + DateTime.Now.ToShortTimeString() + ": " + messageWithValue);
                SendMessage(messageWithValue);
                _serverObject.RemoveMessage(Id, message);
                _dubletMessages.Clear();
            });
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
    }
}