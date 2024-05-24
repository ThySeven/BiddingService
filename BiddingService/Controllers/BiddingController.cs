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
    public class BiddingController : ControllerBase
    {
        private readonly IBiddingRepo _bidService;
        private readonly ConnectionFactory _factory;

        public BiddingController(IBiddingRepo bidService)
        {
            _bidService = bidService;
            var rabbitMQHostName = Environment.GetEnvironmentVariable("RabbitMQHostName") ?? "localhost"; // Provide a default hostname
            _factory = new ConnectionFactory { HostName = rabbitMQHostName };
        }

        [HttpPost]
        public async Task<IActionResult> PlaceBid([FromBody] Bid bid)
        {
            try
            {
                // Your logic to handle the bid goes here

                Console.WriteLine($"{bid.BidderId} has placed a bid of {bid.Amount} at {bid.Timestamp}");


                // Simulating some asynchronous operation (e.g., database operation)
                await Task.Delay(100);

                // Send the bid object to RabbitMQ
                using (var connection = _factory.CreateConnection())
                using (var channel = connection.CreateModel())
                {
                    var test = Environment.GetEnvironmentVariable("BiddingQueueName");
                    channel.QueueDeclare(queue: Environment.GetEnvironmentVariable("BiddingQueueName"),
                                         durable: false,
                                         exclusive: false,
                                         autoDelete: false,
                                         arguments: null);

                    string message = Newtonsoft.Json.JsonConvert.SerializeObject(bid);

                    var body = Encoding.UTF8.GetBytes(message);

                    channel.BasicPublish(exchange: "",
                                         routingKey: Environment.GetEnvironmentVariable("BiddingQueueName"),
                                         basicProperties: null,
                                         body: body);
                }

                // If there are no exceptions, the bid was successfully placed
                return Ok(bid);
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Error placing bid: {ex.Message}");
                throw; // Re-throw the exception to propagate it
            }

           
        }

        [HttpGet("lot/{id}")]

        public async Task<IActionResult> BidAgain(string id)
        {


            return Ok($"dette er en dummy side for lot:{id}");


        }
    }

}

