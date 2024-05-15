using System;

namespace BiddingService.Models
{
    public class Bid
    {
        public string LotId {  get; set; }
        public int Amount { get; set; }
        public string BidderID { get; set; }
        public DateTime Timestamp { get; set; }

        public Bid(int amount, string bidderid, DateTime timestamp)
        {
            Amount = amount;
            BidderID = bidderid;
            Timestamp = timestamp;
        }

        public Bid()
        {

        }
    }
}