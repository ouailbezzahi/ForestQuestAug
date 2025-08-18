using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace ForestQuest.Items.Coin
{
    // Simple sprite-sheet animation styled like Player / Enemy:
    // - Evenly sliced horizontal frames
    // - Fixed frame time loop (always animates)
    // - Scaled draw + simple BoundingBox
    public class AnimatedCoin
    {
        private Texture2D _spriteSheet;
        private Vector2 _position;

        private int _frameWidth;
        private int _frameHeight;
        private int _currentFrame;
        private int _frameCount = 7;

        private double _frameTime = 0.10;
        private double _elapsedTime;

        private float _scale = 0.15f;

        public Vector2 Position => _position;
        public int DrawWidth => (int)(_frameWidth * _scale);
        public int DrawHeight => (int)(_frameHeight * _scale);
        public Rectangle BoundingBox => new Rectangle((int)_position.X, (int)_position.Y, DrawWidth, DrawHeight);

        public AnimatedCoin(ContentManager content, Vector2 position)
        {
            _position = position;
            LoadContent(content);
        }

        private void LoadContent(ContentManager content)
        {
            _spriteSheet = content.Load<Texture2D>("Items/coin");
            _frameWidth = _spriteSheet.Width / _frameCount;
            _frameHeight = _spriteSheet.Height;
        }

        public void Update(GameTime gameTime)
        {
            _elapsedTime += gameTime.ElapsedGameTime.TotalSeconds;
            if (_elapsedTime >= _frameTime)
            {
                _elapsedTime = 0;
                _currentFrame++;
                if (_currentFrame >= _frameCount)
                    _currentFrame = 0;
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            Rectangle sourceRect = new Rectangle(_currentFrame * _frameWidth, 0, _frameWidth, _frameHeight);

            spriteBatch.Draw(
                _spriteSheet,
                _position,
                sourceRect,
                Color.White,
                0f,
                Vector2.Zero,
                _scale,
                SpriteEffects.None,
                0f
            );
        }
    }
}