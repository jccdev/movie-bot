using MovieBot.Worker.Interfaces;

namespace MovieBot.Worker.Services.Common
{
    public interface ITrackedWriteActions<T> where T : ITracked
    {
        void SetAddProperties(T model);
        void SetUpdateProperties(T model);
    }
}