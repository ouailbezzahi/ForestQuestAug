using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace ForestQuest.World.Background
{
    // Simple horizontally scrolling background.
    // Behavior matches prior MenuState usage: scrolls at a fixed speed and tiles the texture across the screen.
    public class ScrollingBackground
    {
        private Texture2D _texture;
        private float _scrollSpeed;
        private float _offset;
        private int _screenWidth;
        private int _screenHeight;

        private readonly GraphicsDevice _graphicsDevice;

        public ScrollingBackground(ContentManager content, GraphicsDevice graphicsDevice, string texturePath, float scrollSpeed)
        {
            _graphicsDevice = graphicsDevice ?? throw new ArgumentNullException(nameof(graphicsDevice));
            if (content is null) throw new ArgumentNullException(nameof(content));
            if (string.IsNullOrWhiteSpace(texturePath)) throw new ArgumentException("Must provide a valid content path.", nameof(texturePath));

            _texture = content.Load<Texture2D>(texturePath);
            _scrollSpeed = scrollSpeed;
            _offset = 0f;

            // Initialize with current viewport; will refresh each Draw in case of resize.
            _screenWidth = _graphicsDevice.Viewport.Width;
            _screenHeight = _graphicsDevice.Viewport.Height;
        }

        public void Update(GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Advance offset and wrap to texture width to avoid float drift.
            _offset += _scrollSpeed * dt;
            if (_texture != null && _texture.Width > 0)
            {
                // Keep offset in [0, texture.Width)
                _offset %= _texture.Width;
                if (_offset < 0) _offset += _texture.Width;
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (_texture == null) return;

            // Refresh screen size to handle window resizes.
            _screenWidth = _graphicsDevice.Viewport.Width;
            _screenHeight = _graphicsDevice.Viewport.Height;

            // Tile the texture horizontally across the screen height, stretched to screen height (as used previously).
            int texWidth = _texture.Width;
            float x = -_offset;

            while (x < _screenWidth)
            {
                spriteBatch.Draw(_texture, new Rectangle((int)x, 0, texWidth, _screenHeight), Color.White);
                x += texWidth;
            }
        }
    }
}