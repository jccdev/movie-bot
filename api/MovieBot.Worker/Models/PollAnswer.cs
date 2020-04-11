using System.Collections.Generic;
using System.Linq;

namespace MovieBot.Worker.Models
{
    public class PollAnswer
    {
        public string Answer { get; set; }
        public string Reaction { get; set; }
        public IEnumerable<ulong> VoterIds { get; set; } = Enumerable.Empty<ulong>();
        public int Count { get; set; }
        public bool IsWinner { get; set; }
    }
}