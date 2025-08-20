using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace ForestQuest.Entities.Enemies
{
    public class Enemy
    {
        private Vector2 _position;

        // Movement / detectie
        private float _speed = 1.2f;
        private bool _followingPlayer;
        private float _followDistance = 140f;
        private float _stopFollowDistance = 190f;
        private float _attackRange = 42f;

        // Facing
        private enum Facing { Left, Right }
        private Facing _facing = Facing.Right;
        private double _idleDirTimer;
        private const double IdleDirSwitch = 2.0;

        // State
        private EnemyState _state = EnemyState.Idle;
        private bool _isDead;

        // Textures per anim
        private Texture2D _texIdle;
        private Texture2D _texRun;
        private Texture2D _texAttack;
        private Texture2D _texDeath;

        // Huidige animatie data
        private Texture2D _currentTex;
        private int _currentFrame;
        private int _frameCount;
        private double _frameTime;
        private double _elapsed;
        private bool _loop;
        private bool _animFinished;

        // Config
        private readonly AnimConfig _cfgIdle = new(4, 0.18, true);
        private readonly AnimConfig _cfgRun = new(6, 0.10, true);
        private readonly AnimConfig _cfgAttack = new(4, 0.12, false); // één cyclus
        private readonly AnimConfig _cfgDeath = new(4, 0.15, false);

        private float _scale = 0.5f;

        private record struct AnimConfig(int Frames, double FrameTime, bool Loop);

        // Nieuw: level & hit-cooldown
        public int Level { get; }
        private float _hitCooldownTimer; // seconden tot volgende damage mogelijk
        private const float HitCooldownDuration = 2f; // afstemmen op attack anim

        public Enemy(Vector2 startPosition, int level = 1)
        {
            _position = startPosition;
            Level = Math.Clamp(level, 1, 3);
        }

        public void LoadContent(ContentManager content)
        {
            _texIdle = content.Load<Texture2D>("Enemy/Level1/cat_idle");
            _texRun = content.Load<Texture2D>("Enemy/Level1/cat_run");
            _texAttack = content.Load<Texture2D>("Enemy/Level1/cat_attack");
            _texDeath = content.Load<Texture2D>("Enemy/Level1/cat_death");

            SetState(EnemyState.Idle, force: true);
        }

        private void SetState(EnemyState newState, bool force = false)
        {
            if (!force && newState == _state && _currentTex != null) return;

            _state = newState;
            _currentFrame = 0;
            _elapsed = 0;
            _animFinished = false;

            AnimConfig cfg;
            switch (_state)
            {
                case EnemyState.Idle: _currentTex = _texIdle; cfg = _cfgIdle; break;
                case EnemyState.Run: _currentTex = _texRun; cfg = _cfgRun; break;
                case EnemyState.Attack: _currentTex = _texAttack; cfg = _cfgAttack; break;
                case EnemyState.Death: _currentTex = _texDeath; cfg = _cfgDeath; break;
                default: _currentTex = _texIdle; cfg = _cfgIdle; break;
            }

            _frameCount = Math.Max(1, cfg.Frames);
            _frameTime = cfg.FrameTime <= 0 ? 0.1 : cfg.FrameTime;
            _loop = cfg.Loop;
        }

        public void Kill()
        {
            if (_isDead) return;
            _isDead = true;
            SetState(EnemyState.Death, force: true);
        }

        public void Update(GameTime gameTime, Vector2 playerPos) => Update(gameTime, playerPos, null);

        public void Update(GameTime gameTime, Vector2 playerPos, int[,] tiles)
        {
            if (_currentTex == null) return;

            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (_hitCooldownTimer > 0) _hitCooldownTimer -= dt;

            if (_isDead)
            {
                UpdateAnimation(gameTime);
                return;
            }

            float dist = Vector2.Distance(_position, playerPos);

            if (_followingPlayer)
            {
                if (dist > _stopFollowDistance) _followingPlayer = false;
            }
            else if (dist < _followDistance) _followingPlayer = true;

            if (_followingPlayer && dist <= _attackRange && !_animFinished)
                SetState(EnemyState.Attack);
            else if (_followingPlayer && dist > _attackRange)
                SetState(EnemyState.Run);
            else if (!_followingPlayer)
                SetState(EnemyState.Idle);

            if (_state == EnemyState.Attack && _animFinished)
            {
                if (dist <= _attackRange)
                    SetState(EnemyState.Attack, force: true); // eventueel later met echte cooldown
                else
                    SetState(_followingPlayer ? EnemyState.Run : EnemyState.Idle, force: true);
            }

            if (_state == EnemyState.Run)
            {
                Vector2 toPlayer = playerPos - _position;
                if (toPlayer.LengthSquared() > 1f)
                {
                    toPlayer.Normalize();
                    if (toPlayer.X > 0.1f) _facing = Facing.Right;
                    else if (toPlayer.X < -0.1f) _facing = Facing.Left;
                    TryMove(toPlayer * _speed, tiles);
                }
            }
            else if (_state == EnemyState.Idle)
            {
                _idleDirTimer += gameTime.ElapsedGameTime.TotalSeconds;
                if (_idleDirTimer >= IdleDirSwitch)
                {
                    _facing = _facing == Facing.Right ? Facing.Left : Facing.Right;
                    _idleDirTimer = 0;
                }
            }
            else if (_state == EnemyState.Attack)
            {
                if (playerPos.X > _position.X) _facing = Facing.Right;
                else if (playerPos.X < _position.X) _facing = Facing.Left;
            }

            UpdateAnimation(gameTime);
        }

        // Nieuw: proberen damage toe te brengen (wordt aangeroepen vanuit GameState)
        public bool TryDealDamage(Rectangle playerRect, out int damage)
        {
            damage = 0;
            if (_isDead) return false;
            if (_state != EnemyState.Attack) return false;
            if (_hitCooldownTimer > 0) return false;

            // Simpele impact frame: midden van animatie
            int impactFrame = _frameCount > 0 ? _frameCount / 2 : 0;
            if (_currentFrame != impactFrame) return false;

            Rectangle enemyRect = BoundingBox;
            if (!enemyRect.Intersects(playerRect)) return false;

            damage = Level switch
            {
                1 => 10,
                2 => 20,
                3 => 30,
                _ => 10
            };

            _hitCooldownTimer = HitCooldownDuration;
            return true;
        }

        private void TryMove(Vector2 delta, int[,] tiles)
        {
            if (tiles == null)
            {
                _position += delta;
                return;
            }

            Vector2 newPosX = _position + new Vector2(delta.X, 0);
            if (!Collides(newPosX, tiles))
                _position.X = newPosX.X;

            Vector2 newPosY = _position + new Vector2(0, delta.Y);
            if (!Collides(newPosY, tiles))
                _position.Y = newPosY.Y;
        }

        private bool Collides(Vector2 testPos, int[,] tiles)
        {
            if (tiles == null) return false;
            int tileSize = 32;
            int mapH = tiles.GetLength(0);
            int mapW = tiles.GetLength(1);

            Rectangle rect = new((int)testPos.X, (int)testPos.Y, FrameWidth, FrameHeight);

            if (rect.Left < 0 || rect.Top < 0 || rect.Right > mapW * tileSize || rect.Bottom > mapH * tileSize)
                return true;

            int startX = rect.Left / tileSize;
            int endX = (rect.Right - 1) / tileSize;
            int startY = rect.Top / tileSize;
            int endY = (rect.Bottom - 1) / tileSize;

            for (int y = startY; y <= endY; y++)
                for (int x = startX; x <= endX; x++)
                    if (tiles[y, x] != 2) return true;

            return false;
        }

        private void UpdateAnimation(GameTime gameTime)
        {
            if (_animFinished) return;

            _elapsed += gameTime.ElapsedGameTime.TotalSeconds;
            while (_elapsed >= _frameTime)
            {
                _elapsed -= _frameTime;
                _currentFrame++;
                if (_currentFrame >= _frameCount)
                {
                    if (_loop)
                        _currentFrame = 0;
                    else
                    {
                        _currentFrame = _frameCount - 1;
                        _animFinished = true;
                    }
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch, Vector2 playerPos)
        {
            if (_currentTex == null || _frameCount <= 0) return;

            int fw = _currentTex.Width / _frameCount;
            int fh = _currentTex.Height;
            var src = new Rectangle(_currentFrame * fw, 0, fw, fh);
            SpriteEffects fx = _facing == Facing.Right ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            spriteBatch.Draw(
                _currentTex,
                _position,
                src,
                Color.White,
                0f,
                Vector2.Zero,
                _scale,
                fx,
                0f
            );
        }

        public Rectangle BoundingBox =>
            new Rectangle((int)_position.X, (int)_position.Y, FrameWidth, FrameHeight);

        public Vector2 Position => _position;
        public EnemyState State => _state;
        public bool IsDead => _isDead;
        public int FrameWidth => (_currentTex == null || _frameCount == 0) ? 0 : (int)((_currentTex.Width / _frameCount) * _scale);
        public int FrameHeight => _currentTex == null ? 0 : (int)(_currentTex.Height * _scale);
    }
}