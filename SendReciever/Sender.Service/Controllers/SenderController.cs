using System.Threading.Tasks;
using System.Web.Http;
using System.Text;
using log4net;
using RabbitMQ.Client;

namespace Sender.Service.Controllers
{
    public class SenderController : ApiController
    {
        private readonly ILog _log;
        public SenderController(ILog log)
        {
            _log = log;
        }

        [Route("Sender/SendTest")]
        [HttpPost]
        public IHttpActionResult SendTest()
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

                    const string message = "Hello World!";
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
    }
}
