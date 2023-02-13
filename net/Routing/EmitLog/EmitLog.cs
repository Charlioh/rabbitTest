using System.Text;
using RabbitMQ.Client;

var factory = new ConnectionFactory { HostName = "localhost" };
using var connection = factory.CreateConnection();
using var channel = connection.CreateModel();
channel.ExchangeDeclare("direct_logs", ExchangeType.Direct);

var message = GetMessage(args);
var body = Encoding.UTF8.GetBytes(message);
channel.BasicPublish(exchange: "direct_logs",
    routingKey: severity,
    basicProperties: null,
    body: body);

Console.WriteLine(" [x] Sent {0}", message);

Console.WriteLine(" Press [enter] to exit.");
Console.ReadLine();

static string GetMessage(string[] args) => (args.Length > 0 ? string.Join(" ", args) : "Hello World!");