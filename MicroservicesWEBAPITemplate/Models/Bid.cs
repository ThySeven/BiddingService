using System;

namespace BiddingService.Models
{
    public class Bid
    {
        public int LotId {  get; set; }
        public int Amount { get; set; }
        public string Bidder { get; set; }
        public DateTime Timestamp { get; set; }

        public Bid(int amount, string bidder, DateTime timestamp)
        {
            Amount = amount;
            Bidder = bidder;
            Timestamp = timestamp;
        }

        public Bid()
        {

        }
    }
}