using System.Collections.Concurrent;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

public class RpcClient : IDisposable
{
    private const string QueueName = "rpc_queue";

    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly string _replyQueueName;
    private readonly ConcurrentDictionary<string, TaskCompletionSource<string>> _callbackMapper = new();

    private readonly List<string> _syncCallbackMapper = new();

    public string result = string.Empty;

    public RpcClient()
    {
        var factory = new ConnectionFactory { HostName = "localhost" };

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
        // declare a server-named queue
        _replyQueueName = _channel.QueueDeclare().QueueName;
        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += (model, ea) =>
        {
            //if (!_callbackMapper.TryRemove(ea.BasicProperties.CorrelationId, out var tcs))
            //    return;
            if (!_syncCallbackMapper.Contains(ea.BasicProperties.CorrelationId)) return;
            _syncCallbackMapper.Remove(ea.BasicProperties.CorrelationId);
            var body = ea.Body.ToArray();
            var response = Encoding.UTF8.GetString(body);
            result = response;
            //tcs.TrySetResult(response);
        };

        _channel.BasicConsume(consumer: consumer,
                             queue: _replyQueueName,
                             autoAck: true);
    }

    public async Task<string> CallAsync(string message, CancellationToken cancellationToken = default)
    {
        IBasicProperties props = _channel.CreateBasicProperties();
        var correlationId = Guid.NewGuid().ToString();
        props.CorrelationId = correlationId;
        props.ReplyTo = _replyQueueName;
        var messageBytes = Encoding.UTF8.GetBytes(message);
        var tcs = new TaskCompletionSource<string>();
        _callbackMapper.TryAdd(correlationId, tcs);

        _channel.BasicPublish(exchange: string.Empty,
                             routingKey: QueueName,
                             basicProperties: props,
                             body: messageBytes);

        cancellationToken.Register(() => _callbackMapper.TryRemove(correlationId, out _));
        return await tcs.Task;
    }

    public bool TryCall(string message)
    {
        IBasicProperties props = _channel.CreateBasicProperties();
        var correlationId = Guid.NewGuid().ToString();
        props.CorrelationId = correlationId;
        props.ReplyTo = _replyQueueName;
        var messageBytes = Encoding.UTF8.GetBytes(message);
        _syncCallbackMapper.Add(correlationId);
        _channel.BasicPublish(exchange: string.Empty,
            routingKey: QueueName,
            basicProperties: props,
            body: messageBytes);

        var spin = new SpinWait();

        while (result == string.Empty && spin.Count < 100)
        {
            spin.SpinOnce();
        }



        return true;
    }

    public void Dispose()
    {
        _connection.Close();
    }
}

public class Rpc
{
    public static void Main(string[] args)
    {
        Console.WriteLine("RPC Client");
        string n = args.Length > 0 ? args[0] : "30";
        InvokeAsync(n);

        Console.WriteLine(" Press [enter] to exit.");
        Console.ReadLine();
    }

    private static void InvokeAsync(string n)
    {
        using var rpcClient = new RpcClient();

        Console.WriteLine(" [x] Requesting fib({0})", n);
        //var response = await rpcClient.CallAsync(n);

        rpcClient.TryCall(n);
        
        Console.WriteLine(" [.] Got '{0}'", rpcClient.result);
    }
}