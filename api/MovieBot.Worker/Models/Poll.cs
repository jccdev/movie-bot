using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson;

namespace MovieBot.Worker.Models
{
    public class Poll
    {
        public ObjectId Id { get; set; }
        public ulong MessageId { get; set; }
        public ulong PollMessageId { get; set; }
        public ulong ResultMessageId { get; set; }
        public ulong ChannelId { get; set; }
        public string Question { get; set; }
        public IEnumerable<PollAnswer> Answers { get; set; } = Enumerable.Empty<PollAnswer>();
        public ulong CreatorId { get; set; }
        public ulong BotId { get; set; }
        public int Expires { get; set; } = 600;
        public DateTimeOffset ExpiresAt { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
        public bool Complete { get; set; } = false;
    }
}