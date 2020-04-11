using System;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace MovieBot.Worker.DataAccess
{
    public class MovieBotDbFactory: IMovieBotDbFactory
    {
        private const string MovieBotDbName = "movie-bot";
        private const string ConnStrEnvVar = "MOVIE_BOT_CONN_STR";
        private readonly IMongoClient _client;

        public MovieBotDbFactory()
        {
            var connString = Environment.GetEnvironmentVariable(ConnStrEnvVar);
            if (connString == null)
            {
                var error = $"Environment Variable '{ConnStrEnvVar}' is not set.";
                throw new Exception(error);
            }
            
            _client = new MongoClient(connString);
        }
        
        public IMongoDatabase Get()
        {
            var database = _client.GetDatabase(MovieBotDbName);
            return database;
        }
    }
}