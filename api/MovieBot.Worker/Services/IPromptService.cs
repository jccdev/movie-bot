using System.Threading.Tasks;
using Discord;
using Discord.Rest;
using Discord.WebSocket;
using MovieBot.Worker.Models;

namespace MovieBot.Worker.Services
{
    public interface IPromptService
    {
        Task ProcessPendingPrompts(IUserMessage message, SocketReaction reaction, DiscordRestClient client);
        Task<Prompt> CreatePollPrompt(ulong userId);
        Task Add(Prompt prompt);
        Task Update(Prompt prompt);
    }
}