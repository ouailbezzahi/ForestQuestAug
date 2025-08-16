using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace ForestQuest.Items.Coin
{
    public class AnimatedCoin
    {
        private Texture2D _spriteSheet;
        private Vector2 _position;
        private int _frameWidth = 120;
        private int _frameHeight = 296;
        private int _frameCount = 7;
        private int _currentFrame = 0;
        private float _frameTime = 0.1f; // tijd per frame
        private double _elapsedTime = 0;
        private float _scale = 0.15f; // mooie grootte voor coin

        public Vector2 Position => _position;
        public Rectangle BoundingBox => new Rectangle((int)_position.X, (int)_position.Y, _frameWidth, _frameHeight);

        public AnimatedCoin(ContentManager content, Vector2 position)
        {
            _spriteSheet = content.Load<Texture2D>("Items/coin");
            _position = position;
        }

        public void Update(GameTime gameTime)
        {
            _elapsedTime += gameTime.ElapsedGameTime.TotalSeconds;
            if (_elapsedTime >= _frameTime)
            {
                _currentFrame++;
                _elapsedTime = 0;
                if (_currentFrame >= _frameCount) _currentFrame = 0;
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            Rectangle sourceRect = new Rectangle(_currentFrame * _frameWidth, 0, _frameWidth, _frameHeight);
            Vector2 origin = Vector2.Zero;
            spriteBatch.Draw(_spriteSheet, _position, sourceRect, Color.White, 0f, origin, _scale, SpriteEffects.None, 0f);
        }
    }
} 