using System.Threading.Tasks;
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
        Task ProcessPollConfigResponse(DiscordRestClient client, ulong messageId);
        Task HandlePrompt(Prompt prompt, SocketReaction reaction);
        Task ClosePoll(ObjectId pollId, DiscordRestClient client);
    }
}