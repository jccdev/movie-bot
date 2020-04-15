using MovieBot.Worker.Interfaces;

namespace MovieBot.Worker.Services.Common
{
    public class DefaultModelWriteActions<T> : IDefaultModelWriteActions<T> where T : IDefaultModel
    {
        private readonly ITrackedWriteActions<T> _trackedWriteActions;
        private readonly ICanDeleteWriteActions<T> _deleteWriteActions;

        public DefaultModelWriteActions(ITrackedWriteActions<T> trackedWriteActions, ICanDeleteWriteActions<T> deleteWriteActions)
        {
            _trackedWriteActions = trackedWriteActions;
            _deleteWriteActions = deleteWriteActions;
        }

        public void SetAddProperties(T model)
        {
            _trackedWriteActions.SetAddProperties(model);
        }

        public void SetUpdateProperties(T model)
        {
            
            _trackedWriteActions.SetUpdateProperties(model);
        }
        
        public void SetDeleteProperties(T model)
        {
            _trackedWriteActions.SetUpdateProperties(model);
            _deleteWriteActions.SetDeleteProperties(model);
        }
        
        public void SetRestoreProperties(T model)
        {
            _trackedWriteActions.SetUpdateProperties(model);
            _deleteWriteActions.SetRestoreProperties(model);
        }
    }
}