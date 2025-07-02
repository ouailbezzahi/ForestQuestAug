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
        private float _speed = 6f;

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

        public void Update(KeyboardState keyboardState, GameTime gameTime, Viewport viewport, int[,] backgroundTiles)
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

            // Bereken nieuwe positie
            Vector2 newPosition = _position + movement;

            // Controleer op schermgrenzen (met schaal 0.2f in acht genomen)
            float scale = 0.2f;
            int scaledWidth = (int)(_frameWidth * scale);
            int scaledHeight = (int)(_frameHeight * scale);
            int screenWidth = viewport.Width;
            int screenHeight = viewport.Height;

            if (newPosition.X < 0) newPosition.X = 0;
            if (newPosition.Y < 0) newPosition.Y = 0;
            if (newPosition.X + scaledWidth > screenWidth) newPosition.X = screenWidth - scaledWidth;
            if (newPosition.Y + scaledHeight > screenHeight) newPosition.Y = screenHeight - scaledHeight;

            // Controleer collision met huizen (0) en bomen (1)
            if (backgroundTiles != null)
            {
                int tileSize = 32;
                int playerTileX = (int)(newPosition.X / tileSize);
                int playerTileY = (int)(newPosition.Y / tileSize);

                Rectangle playerRect = new Rectangle(
                    (int)newPosition.X,
                    (int)newPosition.Y,
                    scaledWidth,
                    scaledHeight
                );

                for (int y = 0; y < backgroundTiles.GetLength(0); y++)
                {
                    for (int x = 0; x < backgroundTiles.GetLength(1); x++)
                    {
                        if (backgroundTiles[y, x] == 0 || backgroundTiles[y, x] == 1) // Huis of boom
                        {
                            Rectangle tileRect = new Rectangle(x * tileSize, y * tileSize, tileSize, tileSize);
                            if (playerRect.Intersects(tileRect))
                            {
                                // Corrigeer positie om collision te voorkomen
                                if (movement.X != 0) newPosition.X = _position.X;
                                if (movement.Y != 0) newPosition.Y = _position.Y;
                            }
                        }
                    }
                }
            }

            // Update positie
            _position = newPosition;

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
            Rectangle sourceRectangle = new Rectangle(
                _currentFrame * _frameWidth,
                _currentRow * _frameHeight,
                _frameWidth,
                _frameHeight
            );

            // Pas een schaal toe om de speler kleiner te maken
            float scale = 0.2f;

            spriteBatch.Draw(
                _spritesheet,
                _position,
                sourceRectangle,
                Color.White,
                0f,
                Vector2.Zero,
                scale,
                SpriteEffects.None,
                0f
            );
        }
    }
}