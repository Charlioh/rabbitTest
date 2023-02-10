using System.Text;
using System.Web.Http;
using log4net;
using RabbitMQ.Client;

namespace Sender.Controllers
{
    public class SenderController : ApiController
    {
        private readonly ILog _log;
        public SenderController(ILog log)
        {
            _log = log;
        }

        [Route("Sender/SendTest")]
        [HttpGet]
        public IHttpActionResult SendTest([FromUri]string text1, [FromUri]string text2)
        {
            var factory = new ConnectionFactory { HostName = "localhost" };
            using (var connection = factory.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    channel.QueueDeclare(queue: "hello",
                        durable: false,
                        exclusive: false,
                        autoDelete: false,
                        arguments: null);

                    var message = GetMessage(new []{text1, text2});
                    var body = Encoding.UTF8.GetBytes(message);

                    channel.BasicPublish(exchange: string.Empty,
                        routingKey: "hello",
                        basicProperties: null,
                        body: body);

                    _log.Info($" [x] Sent {message}");
                }
            }

            return Ok();
        }

        private static string GetMessage(string[] texts) => texts.Length > 0 ? string.Join(" ",texts) : "Hello World!";
    }
}