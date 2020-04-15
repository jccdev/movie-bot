using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Rest;
using Discord.WebSocket;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;
using MovieBot.Worker.Constants;
using MovieBot.Worker.DataAccess;
using MovieBot.Worker.Interfaces;
using MovieBot.Worker.Models;
using MovieBot.Worker.Services.Common;

namespace MovieBot.Worker.Services
{
    public class PollService : IPollService
    {
        private readonly IPollDataAccess _dataAccess;
        private readonly IDefaultModelWriteActions<Poll> _writeActions;
        
        public PollService(IPollDataAccess dataAccess, IDefaultModelWriteActions<Poll> writeActions)
        {
            _dataAccess = dataAccess;
            _writeActions = writeActions;
        }

        public async Task HandleExpired(DiscordSocketClient client)
        {
            var expired =  await _dataAccess.GetExpired();
            foreach (var poll in expired)
            {
                await ClosePoll(poll, client.Rest);
            }
        }

        public async Task ClosePoll(ObjectId pollId, DiscordRestClient client)
        {
            var poll = await _dataAccess.Get(pollId);
            await ClosePoll(poll, client);
        }

        private async Task ClosePoll(Poll poll, DiscordRestClient client)
        {
            var channel = (RestTextChannel)(await client.GetChannelAsync(poll.ChannelId));
            var origMsg = (RestUserMessage)(await channel.GetMessageAsync(poll.PollMessageId.Value));
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

        public async Task ProcessPollConfigResponse(IMessageChannel channel, IMessage origMsg, SocketReaction reaction)
        {
            var poll =  await _dataAccess.GetPendingPoll(origMsg.Id);
            if (poll != null && poll.CreatorId == reaction.UserId)
            {
                switch (reaction.Emote.Name)
                {
                    case Indicators.E:
                        await ConfigurePoll(poll, channel);
                        break;
                    case Indicators.A:
                        await ConfigurePoll(poll, channel, 5);
                        break;
                    case Indicators.B:
                        await ConfigurePoll(poll, channel, 15);
                        break;
                    case Indicators.C:
                        await ConfigurePoll(poll, channel, 30);
                        break;
                    case Indicators.D:
                        await ConfigurePoll(poll, channel, 60);
                        break;
                }
            }
        }
        
        public async Task Add(Poll poll)
        {
            _writeActions.SetAddProperties(poll);
            await _dataAccess.Add(poll);
        }
        
        public async Task Update(Poll poll)
        {
            _writeActions.SetUpdateProperties(poll);
            await _dataAccess.Update(poll);
        }

        public async Task<IEnumerable<Poll>> GetPending(ulong userId, int? limit = null)
        {
            var polls = await _dataAccess.GetPendingPollsForUser(userId, limit);
            return polls;
        }
        
        private async Task ConfigurePoll(Poll poll, IMessageChannel channel, int? expireMins = null)
        {
            var reactions = new List<IEmote>();
            var builder = new StringBuilder();
            builder.AppendLine($"**POLL**");
            builder.AppendLine();
            builder.AppendLine($"> {poll.Question}");
            var i = 0;
            foreach (var answer in poll.Answers)
            {
                builder.AppendLine();
                var emoji = Indicators.All[i];
                answer.Reaction = emoji;
                reactions.Add(new Emoji(emoji));
                builder.AppendLine($"{emoji} - *{answer.Answer}*");
                i++;
            }

            if (expireMins != null)
            {
                poll.ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(expireMins.Value);
                if (expireMins.Value >= 60)
                {
                    builder.AppendLine($"Poll ends in {TimeSpan.FromMinutes(expireMins.Value).TotalHours} hour(s).");
                }
                else
                {
                    builder.AppendLine();
                    builder.AppendLine($"Poll ends in {expireMins.Value} minute(s).");
                }
            }
            
            builder.AppendLine();
            builder.AppendLine("Please choose:");
            
            var sentMessage = await channel.SendMessageAsync(builder.ToString());
            await sentMessage.AddReactionsAsync(reactions.ToArray());

            poll.PollMessageId = sentMessage.Id;
            await Update(poll);            
            await channel.DeleteMessageAsync(poll.ConfigMessageId);
        }
    }


}