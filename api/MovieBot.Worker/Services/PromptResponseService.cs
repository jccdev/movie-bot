using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Rest;
using Discord.WebSocket;
using MongoDB.Bson;
using MongoDB.Driver;
using MovieBot.Worker.DataAccess;
using MovieBot.Worker.Models;

namespace MovieBot.Worker.Services
{
    public class PromptResponseService
    {
        private readonly IPollService _pollService;
        private readonly IPromptDataAccess _promptDataAccess;

        public PromptResponseService(IPromptDataAccess promptDataAccess, IPollService pollService)
        {
            _promptDataAccess = promptDataAccess;
            _pollService = pollService;
        }

        public async Task ProcessPendingPrompts(ulong messageId, SocketReaction reaction, DiscordRestClient client)
        {
            var prompt = await _promptDataAccess.FindPending(messageId, reaction.UserId);
            if (prompt != null)
            {
                switch (prompt.PromptType)
                {
                    case PromptType.Poll:
                        var match = prompt.Data.Poll.ClosePollReactionMap.FirstOrDefault(cpr => cpr.Emoji == reaction.Emote.Name);
                        if (match != default)
                        {
                            await _pollService.ClosePoll(match.Id, client);
                        }
                        break;
                    
                    default:
                        throw new ArgumentException($"{nameof(PromptType)}: {prompt.PromptType} is not mapped.");
                }
            }
        }
    }

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

    public class PromptData
    {
        public PollPromptData Poll { get; set; } = null;
    }

    public class PollPromptData
    {
        public IEnumerable<(string Emoji, ObjectId Id)> ClosePollReactionMap { get; set; } = Enumerable.Empty<(string Emoji, ObjectId Id)>();
    }

    public enum PromptType
    {
        Poll = 1
    }
}