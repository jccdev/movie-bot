using MongoDB.Driver;

namespace MovieBot.Worker.DataAccess
{
    public interface IMovieBotDbFactory
    {
        IMongoDatabase Get();
    }
}