using System.Threading.Tasks;
using Discord;

namespace MovieBot.Worker.Services
{
    public interface IBotCommandsService
    {
        Task ProcessCommands(IMessage message);
    }
}