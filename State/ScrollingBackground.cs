using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace ForestQuest.State
{
    public class ScrollingBackground
    {
        private Texture2D _texture;
        private float _scrollSpeed;
        private float _offset;
        private int _screenWidth;
        private int _screenHeight;

        public ScrollingBackground(ContentManager content, GraphicsDevice graphicsDevice, string texturePath, float scrollSpeed)
        {
            _texture = content.Load<Texture2D>(texturePath);
            _scrollSpeed = scrollSpeed;
            _offset = 0f;
            _screenWidth = graphicsDevice.Viewport.Width;
            _screenHeight = graphicsDevice.Viewport.Height;
        }

        public void Update(GameTime gameTime)
        {
            _offset += _scrollSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (_offset > _texture.Width)
                _offset -= _texture.Width;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            int texWidth = _texture.Width;
            int texHeight = _texture.Height;
            float x = -_offset;
            while (x < _screenWidth)
            {
                spriteBatch.Draw(_texture, new Rectangle((int)x, 0, texWidth, _screenHeight), Color.White);
                x += texWidth;
            }
        }
    }
} 