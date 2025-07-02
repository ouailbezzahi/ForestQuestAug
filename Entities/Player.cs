using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace ForestQuest.Entities
{
    public class Player
    {
        private Texture2D _spritesheet;
        private Vector2 _position;
        private float _speed = 2f;

        // Animatie
        private int _frameWidth; // Breedte van één frame
        private int _frameHeight; // Hoogte van één frame
        private int _currentFrame; // Huidige frame (0-3 per rij)
        private int _totalFramesPerRow = 4; // Frames per rij
        private double _frameTime; // Tijd per frame
        private double _elapsedTime; // Verstreken tijd
        private int _currentRow; // Huidige rij (richting)

        public Player(Vector2 startPosition)
        {
            _position = startPosition;
            _frameTime = 0.1; // 100ms per frame
        }

        public void LoadContent(ContentManager content)
        {
            _spritesheet = content.Load<Texture2D>("Player/Lina");
            _frameWidth = _spritesheet.Width / 4; // 4 kolommen
            _frameHeight = _spritesheet.Height / 4; // 4 rijen
        }

        public void Update(KeyboardState keyboardState, GameTime gameTime)
        {
            Vector2 movement = Vector2.Zero;

            // Beweging logica
            if (keyboardState.IsKeyDown(Keys.Z))
            {
                movement.Y -= _speed;
                _currentRow = 1;
            }
            if (keyboardState.IsKeyDown(Keys.S))
            {
                movement.Y += _speed;
                _currentRow = 0;
            }
            if (keyboardState.IsKeyDown(Keys.Q))
            {
                movement.X -= _speed;
                _currentRow = 2;
            }
            if (keyboardState.IsKeyDown(Keys.D))
            {
                movement.X += _speed;
                _currentRow = 3;
            }

            _position += movement;

            // Animatie logica
            if (movement != Vector2.Zero)
            {
                _elapsedTime += gameTime.ElapsedGameTime.TotalSeconds;

                if (_elapsedTime >= _frameTime)
                {
                    _currentFrame++;
                    _elapsedTime = 0;

                    // Zorg ervoor dat de frames cyclisch worden afgespeeld
                    if (_currentFrame >= _totalFramesPerRow) _currentFrame = 0;
                }
            }
            else
            {
                _currentFrame = 0; // Terug naar het eerste frame als de speler stilstaat
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            // Bereken de bronrechthoek voor het huidige frame
            Rectangle sourceRectangle = new Rectangle(_currentFrame * _frameWidth, _currentRow * _frameHeight, _frameWidth, _frameHeight);

            spriteBatch.Draw(_spritesheet, _position, sourceRectangle, Color.White);
        }
    }
}
