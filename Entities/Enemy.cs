using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace ForestQuest.Entities
{
    public class Enemy
    {
        private Texture2D _texture;
        private Vector2 _position;

        // Movement / follow
        private float _speed = 1f;
        private bool _followingPlayer = false;
        private const float _followDistance = 120f;
        private const float _stopFollowDistance = 160f;

        // Animation (single row, 4 frames)
        private readonly int _frameWidth = 78; // 315 / 4 ≈ 78.75
        private readonly int _frameHeight = 80;
        private int _currentFrame;
        private int _totalFrames = 4;
        private double _frameTime = 0.15;
        private double _elapsedTime;

        // Direction (only Left / Right)
        private enum Direction { Left, Right }
        private Direction _direction = Direction.Right;
        private double _directionTimer = 0;
        private const double _directionSwitchTime = 2.0;

        // Scale (smaller enemies)
        private const float _scale = 0.6f;

        // Random (if needed for future extension)
        private static readonly Random _rand = new Random();

        public Enemy(Vector2 startPosition)
        {
            _position = startPosition;
        }

        public void LoadContent(ContentManager content)
        {
            _texture = content.Load<Texture2D>("Enemy/squirrel");
        }

        // Backwards compatible update (no tile collision)
        public void Update(GameTime gameTime, Vector2 playerPosition)
            => Update(gameTime, playerPosition, null);

        // Update with tile collision (pass backgroundTiles from GameState)
        public void Update(GameTime gameTime, Vector2 playerPosition, int[,] backgroundTiles)
        {
            UpdateAnimation(gameTime);
            UpdateIdleDirection(gameTime);

            float distanceToPlayer = Vector2.Distance(_position, playerPosition);

            if (_followingPlayer)
            {
                if (distanceToPlayer > _stopFollowDistance)
                    _followingPlayer = false;
            }
            else
            {
                if (distanceToPlayer < _followDistance && IsFacingPlayer(playerPosition))
                    _followingPlayer = true;
            }

            if (_followingPlayer)
            {
                Vector2 toPlayer = playerPosition - _position;
                if (toPlayer.LengthSquared() > 1f)
                {
                    toPlayer.Normalize();

                    // Override facing while following
                    if (toPlayer.X > 0.1f) _direction = Direction.Right;
                    else if (toPlayer.X < -0.1f) _direction = Direction.Left;

                    TryMove(toPlayer * _speed, backgroundTiles);
                }
            }
        }

        private void UpdateAnimation(GameTime gameTime)
        {
            _elapsedTime += gameTime.ElapsedGameTime.TotalSeconds;
            if (_elapsedTime >= _frameTime)
            {
                _currentFrame = (_currentFrame + 1) % _totalFrames;
                _elapsedTime = 0;
            }
        }

        private void UpdateIdleDirection(GameTime gameTime)
        {
            if (_followingPlayer) return; // while following direction is controlled by movement
            _directionTimer += gameTime.ElapsedGameTime.TotalSeconds;
            if (_directionTimer >= _directionSwitchTime)
            {
                _direction = _direction == Direction.Right ? Direction.Left : Direction.Right;
                _directionTimer = 0;
            }
        }

        private bool IsFacingPlayer(Vector2 playerPos)
        {
            if (_direction == Direction.Right && playerPos.X > _position.X) return true;
            if (_direction == Direction.Left && playerPos.X < _position.X) return true;
            return false;
        }

        private void TryMove(Vector2 delta, int[,] tiles)
        {
            if (tiles == null)
            {
                _position += delta;
                return;
            }

            // Separate axis movement for simple collision resolution
            Vector2 newPosX = _position + new Vector2(delta.X, 0);
            if (!Collides(newPosX, tiles))
                _position = new Vector2(newPosX.X, _position.Y);

            Vector2 newPosY = _position + new Vector2(0, delta.Y);
            if (!Collides(newPosY, tiles))
                _position = new Vector2(_position.X, newPosY.Y);
        }

        private bool Collides(Vector2 testPos, int[,] tiles)
        {
            int tileSize = 32;
            int mapH = tiles.GetLength(0);
            int mapW = tiles.GetLength(1);

            int w = (int)(_frameWidth * _scale);
            int h = (int)(_frameHeight * _scale);

            // Bounding rectangle for this test position
            Rectangle rect = new Rectangle((int)testPos.X, (int)testPos.Y, w, h);

            // Map bounds
            if (rect.Left < 0 || rect.Top < 0 || rect.Right > mapW * tileSize || rect.Bottom > mapH * tileSize)
                return true;

            // Tile range
            int startX = rect.Left / tileSize;
            int endX = (rect.Right - 1) / tileSize;
            int startY = rect.Top / tileSize;
            int endY = (rect.Bottom - 1) / tileSize;

            for (int y = startY; y <= endY; y++)
            {
                for (int x = startX; x <= endX; x++)
                {
                    int tile = tiles[y, x];
                    // Walkable only if grass (2)
                    if (tile != 2)
                        return true;
                }
            }
            return false;
        }

        public void Draw(SpriteBatch spriteBatch, Vector2 playerPosition)
        {
            SpriteEffects fx = _direction == Direction.Right ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            Rectangle source = new Rectangle(_currentFrame * _frameWidth, 0, _frameWidth, _frameHeight);

            spriteBatch.Draw(
                _texture,
                _position,
                source,
                Color.White,
                0f,
                Vector2.Zero,
                _scale,
                fx,
                0f
            );
        }

        // Public info
        public Vector2 Position => _position;
        public int FrameWidth => (int)(_frameWidth * _scale);
        public int FrameHeight => (int)(_frameHeight * _scale);
    }
}