namespace LD49
{
    using Microsoft.Xna.Framework;
    using System.Collections.Generic;
    using System.IO;
    using System.Text.Json;
    using System.Threading.Tasks;

    public class Tile
    {
        public char Type; // I, L, T, X
        public int Rotation; // 0, 1, 2, 3
    }

    public class Room
    {
        private Dictionary<Point, Tile> Tiles = new Dictionary<Point, Tile>();
    }

    public class Level
    {
        private Dictionary<Point, Room> Rooms = new Dictionary<Point, Room>();
    }

    public class Gameplay
    {
        public Level CurrentLevel { get; private set; }

        private async Task<Level> LoadLevel(string path)
        {
            using var file = File.OpenRead(path);
            return await JsonSerializer.DeserializeAsync<Level>(file);
        }
    }
}
