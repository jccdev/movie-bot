using System;
using MovieBot.Worker.Interfaces;

namespace MovieBot.Worker.Services.Common
{
    public class TrackedWriteActions<T> : ITrackedWriteActions<T> where T : ITracked
    {
        public void SetAddProperties(T model)
        {
            var date = DateTimeOffset.UtcNow;
            model.CreatedAt = date;
            model.UpdatedAt = date;
        }

        public void SetUpdateProperties(T model)
        {
            var date = DateTimeOffset.UtcNow;
            model.UpdatedAt = date;
        }
    }
}