using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson;

namespace MovieBot.Worker.Models
{
    public class PollPromptData
    {
        public IEnumerable<PollPromptReactionMap> ClosePollReactionMap { get; set; } =new List<PollPromptReactionMap>();
    }
}