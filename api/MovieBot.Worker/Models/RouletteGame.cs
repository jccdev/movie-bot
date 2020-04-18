using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson;
using MovieBot.Worker.Interfaces;

namespace MovieBot.Worker.Models
{
    public class RouletteGame: DefaultModel
    {
        public IEnumerable<string> Titles { get; set; } = Enumerable.Empty<string>();
        public string Winner { get; set; }
        public bool InProgress { get; set; } = false;
    }
}