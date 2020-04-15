using MovieBot.Worker.Interfaces;

namespace MovieBot.Worker.Services.Common
{
    public class CanDeleteWriteActions<T> : ICanDeleteWriteActions<T> where T : ICanDelete
    {
        public void SetDeleteProperties(T model)
        {
            model.Deleted = true;
        }
        
        public void SetRestoreProperties(T model)
        {
            model.Deleted = true;
        }
    }
}