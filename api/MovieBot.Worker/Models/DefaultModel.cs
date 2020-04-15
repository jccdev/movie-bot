using System;
using MongoDB.Bson;
using MovieBot.Worker.Interfaces;

namespace MovieBot.Worker.Models
{
    public class DefaultModel : IDefaultModel
    {
        public ObjectId Id { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
        public bool Deleted { get; set; } = false;
    }
}