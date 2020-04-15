using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson;
using MovieBot.Worker.Interfaces;

namespace MovieBot.Worker.Models
{
    public class Poll: DefaultModel
    {
        public ulong MessageId { get; set; }
        public ulong? PollMessageId { get; set; }
        public ulong ConfigMessageId { get; set; }
        public ulong ResultMessageId { get; set; }
        public ulong ChannelId { get; set; }
        public string Question { get; set; }
        public IEnumerable<PollAnswer> Answers { get; set; } = Enumerable.Empty<PollAnswer>();
        public ulong CreatorId { get; set; }
        public DateTimeOffset? ExpiresAt { get; set; }
        public bool Complete { get; set; } = false;
    }
}