using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ForestQuest.World.Background
{
    // Adapter to the existing ScrollingBackground without changing it.
    // It hides reflection behind an interface so callers depend on an abstraction.
    public sealed class ScrollingBackgroundAdapter : IScrollingBackground
    {
        private readonly ScrollingBackground _impl;
        private readonly FieldInfo _textureField;
        private readonly FieldInfo _offsetField;

        public ScrollingBackgroundAdapter(ScrollingBackground impl)
        {
            _impl = impl;
            _textureField = typeof(ScrollingBackground).GetField("_texture", BindingFlags.NonPublic | BindingFlags.Instance)!;
            _offsetField = typeof(ScrollingBackground).GetField("_offset", BindingFlags.NonPublic | BindingFlags.Instance)!;
        }

        public void Update(GameTime gameTime) => _impl.Update(gameTime);

        public Texture2D Texture => (Texture2D)_textureField.GetValue(_impl)!;

        public float Offset => (float)_offsetField.GetValue(_impl)!;

        public int TextureWidth => Texture.Width;
    }
}