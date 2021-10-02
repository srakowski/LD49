namespace LD49
{
    using Microsoft.Xna.Framework.Graphics;
    using System.Collections.Generic;

    public record ContentLibrary(
        Dictionary<string, Texture2D> Textures,
        Dictionary<string, SpriteFont> SpriteFonts
    );
}
