using System.Threading.Tasks;
using MongoDB.Driver;
using MovieBot.Worker.Models;
using MovieBot.Worker.Services;

namespace MovieBot.Worker.DataAccess
{
    public class PromptDataAccess : IPromptDataAccess
    {
        private const string CollectionName = "prompts";
        private readonly IMovieBotDbFactory _dbFactory;
        private readonly IGenericDataAccess<Prompt> _genericDataAccess;

        public PromptDataAccess(IMovieBotDbFactory dbFactory, IGenericDataAccess<Prompt> genericDataAccess)
        {
            _dbFactory = dbFactory;
            _genericDataAccess = genericDataAccess;
        }
        
        public Task Add(Prompt poll)
        {
            return _genericDataAccess.Add(poll);
        }
        
        public Task Update(Prompt poll)
        {
            return _genericDataAccess.Update(poll);
        }
        
        public async Task<Prompt> GetPending(ulong messageId, ulong creatorId)
        {
            var database = _dbFactory.Get();
            var collection = database.GetCollection<Prompt>(CollectionName);
            var result = await collection
                .Find(x =>
                    x.MessageId == messageId &&
                    x.CreatorId == creatorId &&
                    x.Complete == false)
                .FirstOrDefaultAsync();
            return result;
        } 
    }
}