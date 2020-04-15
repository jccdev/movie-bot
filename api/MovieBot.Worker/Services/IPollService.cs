using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Discord.Rest;
using Discord.WebSocket;
using MongoDB.Bson;
using MovieBot.Worker.Models;

namespace MovieBot.Worker.Services
{
    public interface IPollService
    {
        Task HandleExpired(DiscordSocketClient client);
        Task Add(Poll poll);
        Task Update(Poll poll);
        Task ProcessPollConfigResponse(IMessageChannel channel, IMessage origMsg, SocketReaction reaction);
        Task ClosePoll(ObjectId pollId, DiscordRestClient client);
        Task<IEnumerable<Poll>> GetPending(ulong userId, int? limit = null);
    }
}