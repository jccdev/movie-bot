using System.Threading.Tasks;
using MovieBot.Worker.Interfaces;
using MovieBot.Worker.Models;

namespace MovieBot.Worker.DataAccess
{
    public interface IGenericDataAccess<T> where T : IHasId
    {
        Task Add(T value);
        Task Update(T value);
    }
}