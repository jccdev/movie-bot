using System.Threading.Tasks;
using MovieBot.Worker.Models;

namespace MovieBot.Worker.DataAccess
{
    public interface IGenericDataAccess<T> where T : IDefaultModel
    {
        Task Add(T value);
        Task Update(T value);
    }
}