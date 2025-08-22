using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ForestQuest.World.Background
{
    // Abstraction for a scrolling background the menu can render without knowing internals.
    public interface IScrollingBackground
    {
        void Update(GameTime gameTime);
        Texture2D Texture { get; }     // full-width tile
        float Offset { get; }          // horizontal scroll offset in pixels
        int TextureWidth { get; }
    }
}