using System;

namespace BiddingService.Models
{
    public class Bid
    {
        public string LotId {  get; set; }
        public int Amount { get; set; }
<<<<<<< HEAD
        public string BidderID { get; set; }
=======
        public string BidderId { get; set; }
>>>>>>> ee9ac72632222586a79bd3648b4b64db1fd9b936
        public DateTime Timestamp { get; set; }

        public Bid(int amount, string bidderid, DateTime timestamp)
        {
            Amount = amount;
<<<<<<< HEAD
            BidderID = bidderid;
=======
            BidderId = bidder;
>>>>>>> ee9ac72632222586a79bd3648b4b64db1fd9b936
            Timestamp = timestamp;
        }

        public Bid()
        {

        }
    }
}