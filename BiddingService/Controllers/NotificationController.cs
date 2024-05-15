using BiddingService.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Text;
using System.Threading.Tasks;
using RabbitMQ.Client;
using BiddingService.Models;
using BiddingService.Service.LotService.Services;
using System.Net.Mail;

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
        public async Task<IActionResult> GetNotification([FromBody] Notification receivedNotification)
        {
            AuctionCoreLogger.Logger.Info($"Recieved notification ment for: {receivedNotification.RecieverMail}");
            try
            {
                // Simulating some asynchronous operation (e.g., database operation)

                // Send the notification object to RabbitMQ asynchronously
                var connection = _factory.CreateConnection();
                var channel = connection.CreateModel();

                channel.QueueDeclare(queue: Environment.GetEnvironmentVariable("RabbitMQQueueName"),
                                             durable: false,
                                             exclusive: false,
                                             autoDelete: false,
                                             arguments: null);

                 // Create a MailModel object
                 MailModel mail = new MailModel();
                 mail.ReceiverMail = receivedNotification.RecieverMail;
                 mail.Header = $"Du er blevet overbudt på lot nummer {receivedNotification.LotName}";
                 mail.Content = @$"<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Overbid Notification</title>
</head>
<body>
    <div style=""font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;"">
        <h2 style=""color: #333;"">Overbid Notification</h2>
        <p>Dear User,</p>
        <p>We regret to inform you that you have been overbid in an auction.</p>
        <p>Details of the lot:</p>
        <ul>
            <li><strong>Lot ID:</strong> {receivedNotification.LotId}</li>
            <li><strong>Lot Name:</strong> {receivedNotification.LotName}</li>
            <li><strong>New Lot Price:</strong> {receivedNotification.NewLotPrice}</li>
            <li><strong>Auction End Time:</strong> {receivedNotification.TimeStamp}</li>
        </ul>
        <p>To place a higher bid, please click the following link:</p>
        <a href=""{receivedNotification.NewBidLink}"" style=""display: inline-block; background-color: #007bff; color: #fff; text-decoration: none; padding: 10px 20px; border-radius: 5px;"">Place Bid</a>
        <p>If you have any questions, feel free to contact us.</p>
        <p>Best regards,<br>Your Auction Team</p>
 <div class="""" signature"""">
        <p>Authorized Signature</p>
        <img src=""https://scontent.fcph5-1.fna.fbcdn.net/v/t39.30808-6/441856076_122119247390268251_6872370368606673835_n.jpg?_nc_cat=105&ccb=1-7&_nc_sid=5f2048&_nc_ohc=bTJRI1S50akQ7kNvgGyZA_x&_nc_ht=scontent.fcph5-1.fna&oh=00_AYAYd5Li28UIRQrB7u2kzFOQZz3zK_lyzkVtVz-1tusTUA&oe=6648FCD0"" alt="""" Signature"""">
        <p>Kell Olsen, CEO</p>
    </div>
</body>
</html>
";

                  // Serialize the notification object to JSON
                  string message = Newtonsoft.Json.JsonConvert.SerializeObject(mail);
                  
                  var body = Encoding.UTF8.GetBytes(message);
                  
                  // Publish the message to RabbitMQ asynchronously
                  channel.BasicPublish(exchange: "",
                                       routingKey: Environment.GetEnvironmentVariable("RabbitMQQueueName"),
                                       basicProperties: null,
                                       body: body);

                AuctionCoreLogger.Logger.Info("Notifikation sendt til mailqueue");

                // If there are no exceptions, the notification was successfully sent
                return Ok(receivedNotification);
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Error sending notification: {ex.Message}");
                throw; // Re-throw the exception to propagate it
            }
        }
    }
}
