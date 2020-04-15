using System.Collections.Generic;
using System.Threading.Tasks;

namespace MovieBot.Worker.Services
{
    public interface IRouletteService
    {
        Task AddTitle(string title);
        Task<string> Spin();
        Task<IEnumerable<string>> List();
        Task Remove(string title);
        Task Clear();
    }
}