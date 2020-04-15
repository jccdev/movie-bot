using MongoDB.Bson;

namespace MovieBot.Worker.Interfaces
{
    public interface IHasId
    {
        ObjectId Id { get; set; }
    }
}