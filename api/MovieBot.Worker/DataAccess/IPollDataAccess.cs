using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Bson;
using MovieBot.Worker.Models;
using MovieBot.Worker.Services;

namespace MovieBot.Worker.DataAccess
{
    public interface IPollDataAccess
    {
        Task Add(Poll poll);
        Task<IEnumerable<Poll>> GetExpired();
        Task Update(Poll poll);
        Task<Poll> GetPendingPoll(ulong messageId);
        Task<IEnumerable<Poll>> GetOpenPollsForUser(ulong userId);
        Task<Poll> Get(ObjectId id);
    }
}