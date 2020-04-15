using System;

namespace MovieBot.Worker.Interfaces
{
    public interface ITracked
    {
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
    }
}