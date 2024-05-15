using System;

namespace BiddingService.Models
{
    public class Bid
    {
        public string LotId {  get; set; }
        public int Amount { get; set; }
        public string BidderId { get; set; }
        public DateTime Timestamp { get; set; }

        public Bid(int amount, string bidder, DateTime timestamp)
        {
            Amount = amount;
            BidderId = bidder;
            Timestamp = timestamp;
        }

        public Bid()
        {

        }
    }
}