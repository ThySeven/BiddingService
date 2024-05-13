using BiddingService.Models;
using BiddingService.Repository;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Text;
using System.Threading.Tasks;
using RabbitMQ.Client;

namespace BiddingService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationController : ControllerBase
    {
        
        private readonly ConnectionFactory _factory;

        public NotificationController()
        {
           
            _factory = new ConnectionFactory { HostName = Environment.GetEnvironmentVariable("RabbitMQHostName") };
        }

        [HttpPost]
        public async Task<IActionResult> GetNotification([FromBody] Notification noti)
        {
            try
            {
                /*// Your logic to handle the bid goes here
                Console.WriteLine($"{noti.Id} has placed a bid of {noti.Amount} at {noti.Timestamp}");*/

                // Simulating some asynchronous operation (e.g., database operation)
                await Task.Delay(100);

                // Send the bid object to RabbitMQ
                using (var connection = _factory.CreateConnection())
                using (var channel = connection.CreateModel())
                {
                    var test = Environment.GetEnvironmentVariable("MailQueueName");
                    channel.QueueDeclare(queue: Environment.GetEnvironmentVariable("MailQueueName"),
                                         durable: false,
                                         exclusive: false,
                                         autoDelete: false,
                                         arguments: null);

                    string message = Newtonsoft.Json.JsonConvert.SerializeObject(noti);

                    var body = Encoding.UTF8.GetBytes(message);

                    channel.BasicPublish(exchange: "",
                                         routingKey: Environment.GetEnvironmentVariable("MailQueueName"),
                                         basicProperties: null,
                                         body: body);
                }

                // If there are no exceptions, the bid was successfully placed
                return Ok(noti);
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Error placing bid: {ex.Message}");
                throw; // Re-throw the exception to propagate it
            }
        }
    }
}

