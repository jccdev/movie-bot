using System.Threading.Tasks;
using Discord.WebSocket;
using MovieBot.Worker.Models;

namespace MovieBot.Worker.Services
{
    public interface IPollService
    {
        Task HandleExpired(DiscordSocketClient client);
        Task Add(Poll poll);
        Task Update(Poll poll);
    }
}