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
using MovieBot.Worker.Constants;
using MovieBot.Worker.DataAccess;
using MovieBot.Worker.Models;
using MovieBot.Worker.Services.Common;

namespace MovieBot.Worker.Services
{
    public class PromptService : IPromptService
    {
        private readonly IPollService _pollService;
        private readonly IPromptDataAccess _promptDataAccess;
        private readonly IDefaultModelWriteActions<Prompt> _writeActions;

        public PromptService(IPromptDataAccess promptDataAccess, IPollService pollService, IDefaultModelWriteActions<Prompt> writeActions)
        {
            _promptDataAccess = promptDataAccess;
            _pollService = pollService;
            _writeActions = writeActions;
        }

        public async Task ProcessPendingPrompts(IUserMessage message, SocketReaction reaction, DiscordRestClient client)
        {
            var prompt = await _promptDataAccess.GetPending(message.Id, reaction.UserId);
            if (prompt != null)
            {
                switch (prompt.PromptType)
                {
                    case PromptType.Poll:
                        var match = prompt.Data.Poll.ClosePollReactionMap.FirstOrDefault(cpr => cpr.Emoji == reaction.Emote.Name);
                        if (match != default)
                        {
                            await _pollService.ClosePoll(ObjectId.Parse(match.Id), client);
                            prompt.Complete = true;
                            await Update(prompt);
                        }
                        break;
                    
                    default:
                        throw new ArgumentException($"{nameof(PromptType)}: {prompt.PromptType} is not mapped.");
                }
            }
        }

        public async Task Update(Prompt prompt)
        {
            _writeActions.SetUpdateProperties(prompt);
            await _promptDataAccess.Update(prompt);
        }

        public async Task<Prompt> CreatePollPrompt(ulong userId)
        {
            var pending = await _pollService.GetPending(userId, 10);

            if (pending.Any())
            {
                var prompt = new Prompt()
                {
                    PromptType = PromptType.Poll,
                    Complete = false,
                    Data = new PromptData
                    {
                        Poll = new PollPromptData()
                    },
                    CreatorId = userId,
                };

                var maps = new List<PollPromptReactionMap>();

                for (var i = 0; i < pending.Count(); i++)
                {
                    var poll = pending.ElementAt(i);
                    maps.Add(new PollPromptReactionMap {
                        Emoji = Indicators.All[i],
                        Id = poll.Id.ToString(),
                        Question = poll.Question
                    });
                }

                prompt.Data.Poll.ClosePollReactionMap = maps;

                return prompt;
            }
            return null;
        }
        
        public async Task Add(Prompt prompt)
        {
            _writeActions.SetAddProperties(prompt);
            await _promptDataAccess.Add(prompt);
        }
    }
}