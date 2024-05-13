using BiddingService.Models;
using Microsoft.Extensions.Hosting;
using MongoDB.Driver;
using System;

namespace BiddingService.Repository 
{
    public class BidHistoryDB
    {
        private readonly IMongoCollection<Bid> _bidHistoryCollection;
        private string _connectionString = Environment.GetEnvironmentVariable("MongoDBConnectionString");

        public BidHistoryDB()
        {
            var client = new MongoClient(_connectionString);
            var database = client.GetDatabase("AuctionCoreServices");
            _bidHistoryCollection = database.GetCollection<Bid>("BidHistory");
            foreach(var bid in _bidHistoryCollection)
            {
                Console.WriteLine(bid);
            }
        }

    }
}
