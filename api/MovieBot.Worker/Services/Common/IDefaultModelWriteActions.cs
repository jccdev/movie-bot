using MovieBot.Worker.Interfaces;

namespace MovieBot.Worker.Services.Common
{
    public interface IDefaultModelWriteActions<T> where T : IDefaultModel
    {
        void SetAddProperties(T model);
        void SetUpdateProperties(T model);
        void SetDeleteProperties(T model);
        void SetRestoreProperties(T model);
    }
}