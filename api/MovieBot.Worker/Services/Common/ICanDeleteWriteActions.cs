using MovieBot.Worker.Interfaces;

namespace MovieBot.Worker.Services.Common
{
    public interface ICanDeleteWriteActions<T> where T : ICanDelete
    {
        void SetDeleteProperties(T model);
        void SetRestoreProperties(T model);
    }
}