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
        private int _frameWidth;
        private int _frameHeight;
        private int _currentFrame;
        private int _totalFramesPerRow = 4;
        private double _frameTime;
        private double _elapsedTime;
        private int _currentRow;

        private Camera _camera; // Nieuwe camera

        public Vector2 Position => _position; // Voor toegang tot positie
        public int FrameWidth => _frameWidth; // Voor camera-centrering
        public int FrameHeight => _frameHeight; // Voor camera-centrering
        public Matrix CameraTransform => _camera.Transform; // Voor toegang tot camera-transformatie

        public Player(Vector2 startPosition)
        {
            _position = startPosition;
            _frameTime = 0.1;
            _camera = new Camera();
        }

        public void LoadContent(ContentManager content)
        {
            _spritesheet = content.Load<Texture2D>("Player/Lina");
            _frameWidth = _spritesheet.Width / 4;
            _frameHeight = _spritesheet.Height / 4;
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

            // Beperk de speler binnen de mapgrenzen
            float scale = 0.17f;
            int scaledWidth = (int)(_frameWidth * scale);
            int scaledHeight = (int)(_frameHeight * scale);
            int tileSize = 32;
            int mapWidth = backgroundTiles.GetLength(1) * tileSize;
            int mapHeight = backgroundTiles.GetLength(0) * tileSize;

            if (newPosition.X < 0) newPosition.X = 0;
            if (newPosition.Y < 0) newPosition.Y = 0;
            if (newPosition.X + scaledWidth > mapWidth) newPosition.X = mapWidth - scaledWidth;
            if (newPosition.Y + scaledHeight > mapHeight) newPosition.Y = mapHeight - scaledHeight;

            // Controleer collision met huizen (0) en bomen (1)
            if (backgroundTiles != null)
            {
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
                        if (backgroundTiles[y, x] == 0 || backgroundTiles[y, x] == 1)
                        {
                            Rectangle tileRect = new Rectangle(
                                x * tileSize,
                                y * tileSize,
                                tileSize,
                                tileSize
                            );

                            if (playerRect.Intersects(tileRect))
                            {
                                if (movement.X != 0) newPosition.X = _position.X;
                                if (movement.Y != 0) newPosition.Y = _position.Y;
                            }
                        }
                    }
                }
            }

            // Update positie
            _position = newPosition;

            // Update de camera
            _camera.Follow(this, viewport, mapWidth, mapHeight);

            // Animatie logica
            if (movement != Vector2.Zero)
            {
                _elapsedTime += gameTime.ElapsedGameTime.TotalSeconds;

                if (_elapsedTime >= _frameTime)
                {
                    _currentFrame++;
                    _elapsedTime = 0;

                    if (_currentFrame >= _totalFramesPerRow) _currentFrame = 0;
                }
            }
            else
            {
                _currentFrame = 0;
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            Rectangle sourceRectangle = new Rectangle(
                _currentFrame * _frameWidth,
                _currentRow * _frameHeight,
                _frameWidth,
                _frameHeight
            );

            float scale = 0.17f;

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

        private class Camera
        {
            public Vector2 Position { get; private set; }
            public Matrix Transform { get; private set; }

            public void Follow(Player player, Viewport viewport, int mapWidth, int mapHeight)
            {
                // Centreer de camera op de speler
                var playerPosition = player.Position;
                var cameraPosition = new Vector2(
                    playerPosition.X + (player.FrameWidth * 0.17f) / 2 - viewport.Width / 2,
                    playerPosition.Y + (player.FrameHeight * 0.17f) / 2 - viewport.Height / 2
                );

                // Beperk de camera binnen de mapgrenzen
                cameraPosition.X = MathHelper.Clamp(cameraPosition.X, 0, mapWidth - viewport.Width);
                cameraPosition.Y = MathHelper.Clamp(cameraPosition.Y, 0, mapHeight - viewport.Height);

                Position = cameraPosition;

                // Maak de transformatiegematrix
                Transform = Matrix.CreateTranslation(-cameraPosition.X, -cameraPosition.Y, 0);
            }
        }
    }
}