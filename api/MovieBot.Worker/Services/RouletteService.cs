using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MovieBot.Worker.DataAccess;
using MovieBot.Worker.Exceptions;
using MovieBot.Worker.Models;
using MovieBot.Worker.Services.Common;

namespace MovieBot.Worker.Services
{
    public class RouletteService : IRouletteService
    {
        private readonly IDefaultModelWriteActions<RouletteGame> _writeActions;
        private readonly IRouletteGameDataAccess _dataAccess;

        public RouletteService(IRouletteGameDataAccess dataAccess, IDefaultModelWriteActions<RouletteGame> writeActions)
        {
            _dataAccess = dataAccess;
            _writeActions = writeActions;
        }

        public async Task AddTitle(string title)
        {
            var game = await _dataAccess.GetCurrentGame();
            if (game == null)
            {
                game = new RouletteGame();
                game.Titles = new[] {title};
                await Add(game);
            }
            else
            {
                if (game.Titles.Count() >= 20)
                {
                    throw new RouletteException("Cannot add title, max limit of 20 has been reached.");
                }

                if (game.Titles.Any(t => string.Compare(t, title, StringComparison.CurrentCultureIgnoreCase) == 0))
                {
                    throw new RouletteException($"Cannot add title, '{title}' is a duplicate.");
                }
                game.Titles = game.Titles.Concat(new []{ title});
                await Update(game);
            }
        }
        
        public async Task<string> Spin()
        {
            var game = await _dataAccess.GetCurrentGame();

            if (game?.Titles == null || !game.Titles.Any())
            {
                throw new RouletteException("No titles have been added.");
            }
            
            var rand = new Random();
            var i = rand.Next(0, game.Titles.Count() - 1);
            var titles = game.Titles.ToList();
            var res = titles.ElementAt(i);
            titles.RemoveAt(i);
            game.Winner = res;
            var newGame = new RouletteGame { Titles = titles };

            await Update(game);
            await Add(newGame);

            return res;
        }
        
        public async Task<IEnumerable<string>> List()
        {
            var game = await _dataAccess.GetCurrentGame();
            if (game != null)
            {
                return game.Titles;
            }
            return Enumerable.Empty<string>();
        }
        
        public async Task Remove(string title)
        {
            var game = await _dataAccess.GetCurrentGame();
            var match = game?.Titles?.FirstOrDefault(t => string.Compare(t, title, StringComparison.CurrentCultureIgnoreCase) == 0);
            if (match == null)
            {
                throw new RouletteException($"Could not remove, '{title}' not found.");
            }
            var titles = game.Titles.ToList();
            titles.Remove(match);
            game.Titles = titles;
            await Update(game);
        }
        
        public async Task Clear()
        {
            var game = await _dataAccess.GetCurrentGame();
            if (game != null)
            {
                game.Titles = Enumerable.Empty<string>();
                await Update(game);
            }
        }

        private async Task Add(RouletteGame game)
        {
            _writeActions.SetAddProperties(game);
            await _dataAccess.Add(game);
        }
        
        private async Task Update(RouletteGame game)
        {
            _writeActions.SetUpdateProperties(game);
            await _dataAccess.Update(game);
        }
    }
}