using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace WorkerQueues.Consumer.One
{
    public class BusService : IDisposable
    {
        private readonly ConnectionFactory _factory = new ConnectionFactory { HostName = "localhost" };
        private IConnection _connection;
        private IModel _channel;
        private readonly string _logPath;

        public BusService(string logPath)
        {
            _logPath = logPath;
            Init();
        }

        public void Init()
        {
            _connection = _factory.CreateConnection();
            _channel = _connection.CreateModel();
        }

        public void Listen() => StartRead(_channel);

        private void StartRead(IModel channel)
        {
            channel.QueueDeclare(queue: "hello",
                durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            File.AppendAllText(_logPath, " [*] Waiting for messages.");
            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (model, ea) =>
            {
                ManageMessage(ea);
            };
            channel.BasicConsume(queue: "hello",
                autoAck: true,
                consumer: consumer);

            File.AppendAllText(_logPath, "Consumer is running");
            Console.ReadLine();
        }

        private void ManageMessage(BasicDeliverEventArgs message)
        {
            var body = message.Body.ToArray();
            var stringMessage = Encoding.UTF8.GetString(body);
            File.AppendAllText(_logPath, $" [x] Received {stringMessage}");
            var dots = stringMessage.Split('.').Length - 1;
            Thread.Sleep(dots * 1000);
            File.AppendAllText(_logPath, " [x] Done");
        }

        public void Dispose()
        {
            _channel.Close();
            _connection.Close();
            _channel.Dispose();
            _connection.Dispose();
            File.AppendAllText(_logPath, "Queue consumption stopped");
        }
    }
}