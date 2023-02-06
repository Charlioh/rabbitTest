using System;
using System.Diagnostics;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Consum.Service
{
    public static class BusService
    {
        private static readonly ConnectionFactory _factory;
        private static readonly IConnection _connection;
        private static readonly IModel _channel;

        static BusService()
        {
            _factory = new ConnectionFactory { HostName = "localhost" };
            _connection = _factory.CreateConnection();
            _channel = _connection.CreateModel();
        }

        public static void Listen(EventLog eventLog) => StartRead(eventLog, _channel);

        private static void StartRead(EventLog log, IModel channel)
        {
            channel.QueueDeclare(queue: "hello",
                durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            log.WriteEntry(" [*] Waiting for messages.");
            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (model, ea) =>
            {
                ManageMessage(ea, log);
            };
            channel.BasicConsume(queue: "hello",
                autoAck: true,
                consumer: consumer);

            log.WriteEntry("Consumer is running");
            Console.ReadLine();
        }

        private static void ManageMessage(BasicDeliverEventArgs message, EventLog log)
        {
            var body = message.Body.ToArray();
            var stringMessage = Encoding.UTF8.GetString(body);
            log.WriteEntry($" [x] Received {stringMessage}");
        }

        public static void Dispose()
        {
            _channel.Close();
            _connection.Close();
            _channel.Dispose();
            _connection.Dispose();
        }
    }
}