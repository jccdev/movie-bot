using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Rest;
using Discord.WebSocket;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;
using MovieBot.Worker.DataAccess;
using MovieBot.Worker.Models;

namespace MovieBot.Worker.Services
{
    public class PollService : IPollService
    {
        private readonly IPollDataAccess _dataAccess;
        
        public PollService(IPollDataAccess dataAccess)
        {
            _dataAccess = dataAccess;
        }
        
        public async Task HandleExpired(DiscordSocketClient client)
        {
            var expired =  await _dataAccess.GetExpired();
            foreach (var poll in expired)
            {
                var channel = (RestTextChannel)(await client.Rest.GetChannelAsync(poll.ChannelId));
                var origMsg = (RestUserMessage)(await channel.GetMessageAsync(poll.PollMessageId));
                var builder = new StringBuilder();
                builder.AppendLine("**POLL RESULTS**");
                builder.AppendLine();
                builder.AppendLine($"> {poll.Question}");

                var winnerCount = 0;
                var winners = new List<PollAnswer>();

                foreach (var answer in poll.Answers)
                {
                    var emoji = new Emoji(answer.Reaction);

                    var reactionUsers = await origMsg.GetReactionUsersAsync(emoji, int.MaxValue).FlattenAsync();

                    var userIds = reactionUsers
                        .Where(ru => !ru.IsBot)
                        .Select(ru => ru.Id)
                        .ToList();

                    answer.VoterIds = userIds;

                    builder.AppendLine();
                    builder.AppendLine($"{emoji} - {answer.VoterIds.Count()} vote(s) - `{answer.Answer}`");

                    if (userIds.Count > winnerCount)
                    {
                        winnerCount = userIds.Count;
                        winners.Clear();
                    }

                    if (winnerCount > 0 && userIds.Count == winnerCount)
                    {
                        winners.Add(answer);
                    }
                }
                
                builder.AppendLine();
                builder.AppendLine($"**Winner(s)**");

                if (winnerCount == 0 || winnerCount == poll.Answers.Count())
                {
                    builder.AppendLine("Nothing won, way to be indecisive!");
                }

                foreach (var winner in winners)
                {
                    winner.IsWinner = true;
                    builder.AppendLine($"*{winner.Answer}*");
                }

                await channel.SendMessageAsync(builder.ToString());
                poll.Complete = true;
                await Update(poll);
            }
        }
        
        public async Task Add(Poll poll)
        {
            var date = DateTimeOffset.UtcNow;
            poll.ExpiresAt = date.AddSeconds(poll.Expires);
            poll.CreatedAt = date;
            poll.UpdatedAt = date;
            await _dataAccess.Add(poll);
        }
        
        public async Task Update(Poll poll)
        {
            var date = DateTimeOffset.UtcNow;
            poll.CreatedAt = date;
            poll.UpdatedAt = date;
            await _dataAccess.Update(poll);
        }
    }
}