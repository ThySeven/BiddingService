namespace BiddingService.Models
{
    public class Notification
    {
        public string LotId { get; set; }
        public string ReceiverMail { get; set; }
        public DateTime TimeStamp { get; set; }
        public string LotName { get; set; }
        public int NewLotPrice { get; set; }
        public string NewBidLink { get; set; }

    }
}
