using System;
using MongoDB.Bson;
using MovieBot.Worker.Constants;
using MovieBot.Worker.Services;

namespace MovieBot.Worker.Models
{
    public class Prompt: IDefaultModel
    {
        public ObjectId Id { get; set; }
        public PromptData Data { get; set; }
        public ulong MessageId { get; set; }
        public PromptType PromptType { get; set; }
        public ulong CreatorId { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
        public bool Complete { get; set; } = false;
    }
}