namespace BiddingService.Models
{
    public class Notification
    {
        public int LotId { get; set; }
        public string RecieverMail { get; set; }
        public DateTime TimeStamp { get; set; }
        public string LotName { get; set; }
        public string NewLotPrice { get; set; }
        public string NewBidLink { get; set; }

    }
}
