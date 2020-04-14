using System;
using System.Threading.Tasks;
using MongoDB.Driver;
using MovieBot.Worker.Models;
using MovieBot.Worker.Services;

namespace MovieBot.Worker.DataAccess
{
    public class GenericDataAccess<T> : IGenericDataAccess<T> where T : IDefaultModel
    {
        private readonly IMovieBotDbFactory _dbFactory;

        public GenericDataAccess(IMovieBotDbFactory dbFactory)
        {
            _dbFactory = dbFactory;
        }

        public async Task Add(T value)
        {
            var database = _dbFactory.Get();
            var collection = database.GetCollection<T>(CollectionMap(value));
            await collection.InsertOneAsync(value);
        }
        
        public async Task Update(T value)
        {
            var database = _dbFactory.Get();
            var collection = database.GetCollection<T>(CollectionMap(value));
            await collection.ReplaceOneAsync(x => x.Id == value.Id, value);
        }

        private string CollectionMap(T value)
        {
            switch (value)
            {
                case Poll p:
                    return "polls";
                case Prompt prompt:
                    return "prompts";
                default:
                    throw new Exception($"Collection for type {nameof(T)}.");
            }
        }
    }
}