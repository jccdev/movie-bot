using System;
using System.Threading.Tasks;
using MongoDB.Driver;
using MovieBot.Worker.Models;

namespace MovieBot.Worker.DataAccess
{
    public class RouletteGameDataAccess : IRouletteGameDataAccess
    {
        private const string CollectionName = "roulette-games";
        private readonly IGenericDataAccess<RouletteGame> _genericDataAccess;
        private readonly IMovieBotDbFactory _dbFactory;

        public RouletteGameDataAccess(IGenericDataAccess<RouletteGame> genericDataAccess, IMovieBotDbFactory dbFactory)
        {
            _genericDataAccess = genericDataAccess;
            _dbFactory = dbFactory;
        }

        public async Task<RouletteGame> GetCurrentGame()
        {
            var db = _dbFactory.Get();
            var collection = db.GetCollection<RouletteGame>(CollectionName);
            var game = await collection
                .Find(x =>
                    x.Winner == null &&
                    !x.Deleted)
                .SortByDescending(x => x.UpdatedAt)
                .FirstOrDefaultAsync();
            return game;
        }

        public Task Add(RouletteGame value)
        {
            return _genericDataAccess.Add(value);
        }
        
        public Task Update(RouletteGame value)
        {
            return _genericDataAccess.Update(value);
        }
    }
}