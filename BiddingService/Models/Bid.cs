using System;

namespace BiddingService.Models
{
    public class Bid
    {
        public string LotId {  get; set; }
        public int Amount { get; set; }

        public string BidderId { get; set; }

        public DateTime Timestamp { get; set; }

        public Bid(int amount, string bidderid, DateTime timestamp)
        {
            Amount = amount;
            BidderId = bidderid;
            Timestamp = timestamp;
        }

        public Bid()
        {

        }
    }
}