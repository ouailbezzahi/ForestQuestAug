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
        private float _speed = 1f;

        // Animatie
        private int _frameWidth = 78; // 315 / 4 = 78.75, afronden naar 78
        private int _frameHeight = 80; // sprite is 80px hoog
        private int _currentFrame;
        private int _totalFrames = 4;
        private double _frameTime = 0.15;
        private double _elapsedTime;

        // Richting
        private enum Direction { Left, Right }
        private Direction _direction = Direction.Right;
        private double _directionTimer = 0;
        private const double _directionSwitchTime = 2.0;

        // Volg logica
        private bool _followingPlayer = false;
        private const float _followDistance = 120f;
        private const float _stopFollowDistance = 160f;

        public Enemy(Vector2 startPosition)
        {
            _position = startPosition;
            _currentFrame = 0;
            _elapsedTime = 0;
        }

        public void LoadContent(ContentManager content)
        {
            _texture = content.Load<Texture2D>("Enemy/squirrel");
            // _frameWidth en _frameHeight zijn nu hardcoded op basis van sprite info
        }

        public void Update(GameTime gameTime, Vector2 playerPosition)
        {
            // Animatie
            _elapsedTime += gameTime.ElapsedGameTime.TotalSeconds;
            if (_elapsedTime >= _frameTime)
            {
                _currentFrame = (_currentFrame + 1) % _totalFrames;
                _elapsedTime = 0;
            }

            // Richting wisselen elke 2 seconden
            _directionTimer += gameTime.ElapsedGameTime.TotalSeconds;
            if (_directionTimer >= _directionSwitchTime)
            {
                _direction = _direction == Direction.Right ? Direction.Left : Direction.Right;
                _directionTimer = 0;
            }

            float distanceToPlayer = Vector2.Distance(_position, playerPosition);

            // Volg logica
            if (_followingPlayer)
            {
                if (distanceToPlayer > _stopFollowDistance)
                {
                    _followingPlayer = false;
                }
            }
            else
            {
                // Alleen starten met volgen als speler dichtbij is én enemy kijkt naar speler
                if (distanceToPlayer < _followDistance && IsFacingPlayer(playerPosition))
                {
                    _followingPlayer = true;
                }
            }

            // Bewegen
            if (_followingPlayer)
            {
                Vector2 directionToPlayer = playerPosition - _position;
                if (directionToPlayer.Length() > 1f)
                {
                    directionToPlayer.Normalize();
                    Vector2 newPos = _position + directionToPlayer * _speed;

                    // Map grenzen
                    int tileSize = 32;
                    int mapWidth = 31 * tileSize;
                    int mapHeight = 31 * tileSize;
                    newPos.X = Math.Clamp(newPos.X, 0, mapWidth - _frameWidth);
                    newPos.Y = Math.Clamp(newPos.Y, 0, mapHeight - _frameHeight);

                    _position = newPos;
                }
            }
        }

        // Enemy kijkt naar speler als speler links/rechts van enemy is en richting klopt
        private bool IsFacingPlayer(Vector2 playerPosition)
        {
            if (_direction == Direction.Right && playerPosition.X > _position.X)
                return true;
            if (_direction == Direction.Left && playerPosition.X < _position.X)
                return true;
            return false;
        }

        public void Draw(SpriteBatch spriteBatch, Vector2 playerPosition)
        {
            int row = 0; // Altijd rij 0, want je sprite heeft maar 1 rij
            SpriteEffects effects = SpriteEffects.None;
            if (_direction == Direction.Right)
                effects = SpriteEffects.FlipHorizontally;

            Rectangle sourceRect = new Rectangle(_currentFrame * _frameWidth, row * _frameHeight, _frameWidth, _frameHeight);
            float scale = 0.6f; // Maak de enemy 60% van origineel
            spriteBatch.Draw(_texture, _position, sourceRect, Color.White, 0f, Vector2.Zero, scale, effects, 0f);
        }

        public Vector2 Position => _position;
        public int FrameWidth => _frameWidth;
        public int FrameHeight => _frameHeight;
    }
}