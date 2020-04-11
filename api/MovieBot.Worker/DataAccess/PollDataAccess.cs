using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver;
using MovieBot.Worker.Models;
using MovieBot.Worker.Services;

namespace MovieBot.Worker.DataAccess
{
    public class PollDataAccess : IPollDataAccess
    {
        private const string CollectionName = "polls";
        private readonly IMovieBotDbFactory _dbFactory;

        public PollDataAccess(IMovieBotDbFactory dbFactory)
        {
            _dbFactory = dbFactory;
        }

        public async Task Add(Poll poll)
        {
            var database = _dbFactory.Get();
            var collection = database.GetCollection<Poll>(CollectionName);
            await collection.InsertOneAsync(poll);
        }
        
        public async Task Update(Poll poll)
        {
            var database = _dbFactory.Get();
            var collection = database.GetCollection<Poll>(CollectionName);
            await collection.ReplaceOneAsync(x => x.Id == poll.Id, poll);
        }

        public async Task<IEnumerable<Poll>> GetExpired()
        {
            var database = _dbFactory.Get();
            var collection = database.GetCollection<Poll>(CollectionName);
            var results = await collection
                .Find(x =>
                    x.Complete == false &&
                    x.ExpiresAt <= DateTimeOffset.UtcNow)
                .ToListAsync();
            return results;
        }
    }
}