using System;
using MongoDB.Bson;
using MovieBot.Worker.Constants;
using MovieBot.Worker.Interfaces;
using MovieBot.Worker.Services;

namespace MovieBot.Worker.Models
{
    public class Prompt: DefaultModel
    {
        public PromptData Data { get; set; }
        public ulong MessageId { get; set; }
        public PromptType PromptType { get; set; }
        public ulong CreatorId { get; set; }
        public bool Complete { get; set; } = false;
    }
}