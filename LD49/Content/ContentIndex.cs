using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using System.Collections.Generic;

namespace LD49
{
	public static class Effects
	{
		public const string dummy = "Effect/dummy";
		public static Dictionary<string, Effect> LoadEffects(this ContentManager content)
		{
			return new Dictionary<string, Effect>
			{
				["Effect/dummy"] = content.Load<Effect>("Effect/dummy"),
			};
		}
	}
	public static class Songs
	{
		public const string dummy = "Song/dummy";
		public static Dictionary<string, Song> LoadSongs(this ContentManager content)
		{
			return new Dictionary<string, Song>
			{
				["Song/dummy"] = content.Load<Song>("Song/dummy"),
			};
		}
	}
	public static class SoundEffects
	{
		public const string dummy = "SoundEffect/dummy";
		public static Dictionary<string, SoundEffect> LoadSoundEffects(this ContentManager content)
		{
			return new Dictionary<string, SoundEffect>
			{
				["SoundEffect/dummy"] = content.Load<SoundEffect>("SoundEffect/dummy"),
			};
		}
	}
	public static class SpriteFonts
	{
		public const string dummy = "SpriteFont/dummy";
		public static Dictionary<string, SpriteFont> LoadSpriteFonts(this ContentManager content)
		{
			return new Dictionary<string, SpriteFont>
			{
				["SpriteFont/dummy"] = content.Load<SpriteFont>("SpriteFont/dummy"),
			};
		}
	}
	public static class Texture2Ds
	{
		public const string tiles = "Texture2D/tiles";
		public static Dictionary<string, Texture2D> LoadTexture2Ds(this ContentManager content)
		{
			return new Dictionary<string, Texture2D>
			{
				["Texture2D/tiles"] = content.Load<Texture2D>("Texture2D/tiles"),
			};
		}
	}
}
