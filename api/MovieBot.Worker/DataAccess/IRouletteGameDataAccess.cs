using System.Threading.Tasks;
using MovieBot.Worker.Models;

namespace MovieBot.Worker.DataAccess
{
    public interface IRouletteGameDataAccess
    {
        Task Add(RouletteGame value);
        Task Update(RouletteGame value);
        Task<RouletteGame> GetCurrentGame();
    }
}