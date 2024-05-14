using System;
using System.Text;

namespace BiddingService.Models
{
    public class MailNotimix
    {
        private string? receiverMail;
        private string? header;
        private string? content;

        public string? ReceiverMail { get => receiverMail; set => receiverMail = value; }
        public string? Header { get => header; set => header = value; }
        public string? Content { get => content; set => content = value; }

        // Method to generate email content based on a Notification object
        public void GenerateContentFromNotification(Notification notification)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"Lot ID: {notification.LotId}");
            sb.AppendLine($"Lot Name: {notification.LotName}");
            sb.AppendLine($"New Lot Price: {notification.NewLotPrice}");
            sb.AppendLine($"New Bid Link: {notification.NewBidLink}");
            sb.AppendLine($"Timestamp: {notification.TimeStamp}");

            // Set the content of the email to the generated content
            Content = sb.ToString();
        }
    }
}
