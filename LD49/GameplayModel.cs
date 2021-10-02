namespace LD49
{
    using Microsoft.Xna.Framework;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class Tile
    {
        public const char EmptyValue = '0';

        public char Type; // 0, I, L, T, X, B
        public byte Rotation; // 0, 1, 2, 3
        public char Occupant; // 0, H
        public bool IsFixed; // 0, 1

        public static Tile FromCode(string code) => new Tile
        {
            Type = code[0],
            Rotation = byte.Parse(code[1].ToString()),
            Occupant = code[2],
            IsFixed = byte.Parse(code[3].ToString()) == 1
        };

        public string ToCode() => $"{Type}{Rotation}{Occupant}{(IsFixed ? 1 : 0)}";

        public static Tile Empty => new Tile
        {
            Type = EmptyValue,
            Rotation = 0,
            Occupant = EmptyValue,
            IsFixed = true
        };
    }

    public class Level
    {
        public int Number;
        public Dictionary<Point, Tile> State;
    }

    public class GameState
    {
        private List<Level> _levels = new List<Level>();
        private Level _activeLevel = null;

        public void Start()
        {
            _levels = _levels.OrderBy(l => l.Number).ToList();
            _activeLevel = _levels.First();
        }

        public Level ActiveLevel => _activeLevel;

        public void LoadLevel(int levelNumber, string[] levelData)
        {
            _levels.RemoveAll(l => l.Number == levelNumber);

            var level = new Level
            {
                Number = levelNumber,
                State = levelData.SelectMany((ld, y) =>
                    ld.Split(",").Select((t, x) =>
                    {
                        var position = new Point(x, y);
                        var tile = Tile.FromCode(t);
                        return (Position: position, Tile: tile);
                    })
                .Where(t => t.Tile.Type != Tile.EmptyValue)
                ).ToDictionary(
                    k => k.Position,
                    v => v.Tile
                )
            };

            _levels.Add(level);

            if (ActiveLevel?.Number == level.Number)
                _activeLevel = level;
        }

        public List<string> ReadLevel(int number)
        {
            var level = _levels.Single(l => l.Number == number);
            var state = level.State;

            var xMin = state.Keys.Select(k => k.X).Min();
            var xMax = state.Keys.Select(k => k.X).Max();
            var yMin = state.Keys.Select(k => k.Y).Min();
            var yMax = state.Keys.Select(k => k.Y).Max();
            
            var result = new List<string>();
            for (var y = yMin; y < yMax + 1; y++)
            {
                var lineValues = new List<string>();
                for (var x = xMin; x < xMax + 1; x++)
                {
                    var point = new Point(x, y);
                    lineValues.Add(state.ContainsKey(point)
                        ? state[point].ToCode()
                        : Tile.Empty.ToCode());
                }
                result.Add(string.Join(",", lineValues));
            }
            return result;
        }
    }
}
