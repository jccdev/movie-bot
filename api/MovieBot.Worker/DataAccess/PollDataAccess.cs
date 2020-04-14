using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using MovieBot.Worker.Models;

namespace MovieBot.Worker.DataAccess
{
    public class PollDataAccess : IPollDataAccess
    {
        private const string CollectionName = "polls";
        private readonly IMovieBotDbFactory _dbFactory;
        private readonly IGenericDataAccess<Poll> _genericDataAccess;

        public PollDataAccess(IMovieBotDbFactory dbFactory, IGenericDataAccess<Poll> genericDataAccess)
        {
            _dbFactory = dbFactory;
            _genericDataAccess = genericDataAccess;
        }

        public Task<Poll> Get(ObjectId id)
        {
            var database = _dbFactory.Get();
            var collection = database.GetCollection<Poll>(CollectionName);
            return collection.Find(x => x.Id == id).FirstOrDefaultAsync();
        }

        public Task Add(Poll poll)
        {
            return _genericDataAccess.Add(poll);
        }
        
        public Task Update(Poll poll)
        {
            return _genericDataAccess.Update(poll);
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
        
        public async Task<Poll> GetPendingPoll(ulong messageId)
        {
            var database = _dbFactory.Get();
            var collection = database.GetCollection<Poll>(CollectionName);
            var result = await collection
                .Find(x => x.ConfigMessageId == messageId)
                .FirstOrDefaultAsync();
            return result;
        }

        public async Task<IEnumerable<Poll>> GetOpenPollsForUser(ulong userId)
        {
            var database = _dbFactory.Get();
            var collection = database.GetCollection<Poll>(CollectionName);
            
            var results = await collection
                .Find(x => x.CreatorId == userId && x.Complete == false)
                .ToListAsync();
            return results;
        }
    }
}