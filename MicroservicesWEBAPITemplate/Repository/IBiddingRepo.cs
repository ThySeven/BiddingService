using BiddingService.Models;
using System.Threading.Tasks;

namespace BiddingService.Repository
{
    public interface IBiddingRepo
    {
       public Task PlaceBid(Bid bid);
    }
}
