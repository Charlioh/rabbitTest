using System;
using System.Diagnostics;
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
        private EventLog _eventLog;

        public BusService(EventLog eventLog)
        {
            Init(eventLog);
        }

        public void Init(EventLog eventLog)
        {
            _eventLog = eventLog;
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

            _eventLog.WriteEntry(" [*] Waiting for messages.");
            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (model, ea) =>
            {
                ManageMessage(ea);
            };
            channel.BasicConsume(queue: "hello",
                autoAck: true,
                consumer: consumer);

            _eventLog.WriteEntry("Consumer is running");
            Console.ReadLine();
        }

        private void ManageMessage(BasicDeliverEventArgs message)
        {
            var body = message.Body.ToArray();
            var stringMessage = Encoding.UTF8.GetString(body);
            _eventLog.WriteEntry($" [x] Received {stringMessage}");
            var dots = stringMessage.Split('.').Length - 1;
            Thread.Sleep(dots * 1000);
            _eventLog.WriteEntry(" [x] Done");
        }

        public void Dispose()
        {
            _channel.Close();
            _connection.Close();
            _channel.Dispose();
            _connection.Dispose();
            _eventLog.WriteEntry("Queue consumption stopped");
        }
    }
}