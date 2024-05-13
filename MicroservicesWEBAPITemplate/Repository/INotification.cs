using BiddingService.Models;
using System.Threading.Tasks;

namespace BiddingService.Repository
{
    public interface INotification
    {
        public Task GetNotification(Notification noti);
    }
}
