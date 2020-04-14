using System.Threading.Tasks;
using MovieBot.Worker.Services;

namespace MovieBot.Worker.DataAccess
{
    public interface IPromptDataAccess
    {
        Task<Prompt> FindPending(ulong messageId, ulong creatorId);
        Task Add(Prompt poll);
        Task Update(Prompt poll);
    }
}