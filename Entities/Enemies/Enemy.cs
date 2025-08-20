using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace ForestQuest.Entities.Enemies
{
    public class Enemy
    {
        // Wereldpositie: voeten (bottom center)
        private Vector2 _feetPos;

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
        private bool _stateLocked;

        // (Optioneel) Health
        private int _maxHealth = 60;
        private int _health;

        // Textures
        private Texture2D _texIdle;
        private Texture2D _texRun;
        private Texture2D _texAttack;
        private Texture2D _texDeath;

        // Animation data (zelfde structuur als Player)
        private class AnimFrame { public Rectangle Src; public Vector2 Pivot; }
        private class Animation { public List<AnimFrame> Frames = new(); public double FrameTime; public bool Loop; }

        private Animation _animIdle;
        private Animation _animRun;
        private Animation _animAttack;
        private Animation _animDeath;
        private Animation _currentAnim;

        private int _currentFrameIndex;
        private double _animElapsed;
        private bool _animFinished;

        // Collision (afgeleid)
        private int _collisionWidth;
        private int _collisionHeight;

        private readonly float _scale = 0.5f;

        // Attack timing
        private int _attackImpactFrame = 1; // wordt gezet na BuildAnimation
        private float _hitCooldownTimer;
        private const float HitCooldownDuration = 2f;

        public int Level { get; }

        public Enemy(Vector2 startTopLeft, int level = 1)
        {
            // Tijdelijk minimale waardes tot na load
            _collisionWidth = 1;
            _collisionHeight = 1;
            _feetPos = new Vector2(startTopLeft.X + _collisionWidth * 0.5f, startTopLeft.Y + _collisionHeight);
            Level = Math.Clamp(level, 1, 3);
            _health = _maxHealth;
        }

        public void LoadContent(ContentManager content)
        {
            _texIdle   = content.Load<Texture2D>("Enemy/Level1/cat_idle");
            _texRun    = content.Load<Texture2D>("Enemy/Level1/cat_run");
            _texAttack = content.Load<Texture2D>("Enemy/Level1/cat_attack");
            _texDeath  = content.Load<Texture2D>("Enemy/Level1/cat_death");

            _animIdle   = BuildAnimation(_texIdle,   4, 0.18, true);
            _animRun    = BuildAnimation(_texRun,    6, 0.10, true);
            _animAttack = BuildAnimation(_texAttack, 4, 0.12, false);
            _animDeath  = BuildAnimation(_texDeath,  4, 0.15, false);

            // Impact frame = midden van attack anim
            if (_animAttack.Frames.Count > 0)
                _attackImpactFrame = _animAttack.Frames.Count / 2;

            DeriveCollisionFromIdle();
            SetState(EnemyState.Idle, force: true);
        }

        private Animation BuildAnimation(Texture2D tex, int frameCount, double frameTime, bool loop)
        {
            var anim = new Animation { FrameTime = frameTime, Loop = loop };
            int rawFrameWidth = tex.Width / frameCount;
            int rawFrameHeight = tex.Height;

            // Pixel data voor trimming
            Color[] pixels = new Color[tex.Width * tex.Height];
            tex.GetData(pixels);

            for (int i = 0; i < frameCount; i++)
            {
                int frameX = i * rawFrameWidth;
                Rectangle full = new(frameX, 0, rawFrameWidth, rawFrameHeight);

                int minX = full.Right, maxX = full.Left - 1, minY = full.Bottom, maxY = full.Top - 1;
                bool any = false;
                for (int y = 0; y < full.Height; y++)
                {
                    int yGlobal = full.Y + y;
                    for (int x = 0; x < full.Width; x++)
                    {
                        int xGlobal = frameX + x;
                        var c = pixels[yGlobal * tex.Width + xGlobal];
                        if (c.A > 15)
                        {
                            any = true;
                            if (xGlobal < minX) minX = xGlobal;
                            if (xGlobal > maxX) maxX = xGlobal;
                            if (yGlobal < minY) minY = yGlobal;
                            if (yGlobal > maxY) maxY = yGlobal;
                        }
                    }
                }

                Rectangle src;
                Vector2 pivot;
                if (!any)
                {
                    src = full;
                    pivot = new Vector2(full.Width / 2f, full.Height);
                }
                else
                {
                    src = new Rectangle(minX, minY, (maxX - minX + 1), (maxY - minY + 1));
                    pivot = new Vector2(src.Width / 2f, src.Height);
                }

                anim.Frames.Add(new AnimFrame { Src = src, Pivot = pivot });
            }
            return anim;
        }

        private void DeriveCollisionFromIdle()
        {
            int maxW = 0, maxH = 0;
            foreach (var f in _animIdle.Frames)
            {
                if (f.Src.Width > maxW) maxW = f.Src.Width;
                if (f.Src.Height > maxH) maxH = f.Src.Height;
            }
            _collisionWidth = (int)(maxW * _scale);
            _collisionHeight = (int)(maxH * _scale);

            if (_feetPos == Vector2.Zero)
                _feetPos = new Vector2(_collisionWidth * 0.5f, _collisionHeight);
        }

        private void SetState(EnemyState newState, bool force = false)
        {
            if (!force && newState == _state) return;
            if (_isDead && newState != EnemyState.Death) return;

            _state = newState;
            _currentFrameIndex = 0;
            _animElapsed = 0;
            _animFinished = false;

            switch (_state)
            {
                case EnemyState.Idle:   _currentAnim = _animIdle;   _stateLocked = false; break;
                case EnemyState.Run:    _currentAnim = _animRun;    _stateLocked = false; break;
                case EnemyState.Attack: _currentAnim = _animAttack; _stateLocked = true;  break;
                case EnemyState.Death:  _currentAnim = _animDeath;  _stateLocked = true;  break;
                default:                _currentAnim = _animIdle;   _stateLocked = false; break;
            }
        }

        public void Kill()
        {
            if (_isDead) return;
            _isDead = true;
            SetState(EnemyState.Death, force: true);
        }

        public void Update(GameTime gameTime, Vector2 playerTopLeft) => Update(gameTime, playerTopLeft, null);

        public void Update(GameTime gameTime, Vector2 playerTopLeft, int[,] tiles)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (_hitCooldownTimer > 0) _hitCooldownTimer -= dt;

            if (_isDead)
            {
                UpdateAnimation(dt);
                return;
            }

            // Bepaal speler-center (top-left + half collision)
            // Player expose top-left; we reconstrueren center voor afstand
            Vector2 playerCenter = playerTopLeft + new Vector2(0, 0); // vereenvoudigd (fine voor follow)
            Vector2 myCenter = new(_feetPos.X, _feetPos.Y - _collisionHeight * 0.5f);
            float dist = Vector2.Distance(myCenter, playerCenter);

            if (!_stateLocked)
            {
                if (_followingPlayer)
                {
                    if (dist > _stopFollowDistance) _followingPlayer = false;
                }
                else if (dist < _followDistance) _followingPlayer = true;

                if (_followingPlayer && dist <= _attackRange)
                    SetState(EnemyState.Attack);
                else if (_followingPlayer && dist > _attackRange)
                    SetState(EnemyState.Run);
                else if (!_followingPlayer)
                    SetState(EnemyState.Idle);
            }

            if (_state == EnemyState.Attack && _animFinished)
            {
                if (dist <= _attackRange)
                    SetState(EnemyState.Attack, force: true);
                else
                    SetState(_followingPlayer ? EnemyState.Run : EnemyState.Idle, force: true);
            }

            if (_state == EnemyState.Run)
            {
                Vector2 toPlayer = (playerCenter - myCenter);
                if (toPlayer.LengthSquared() > 1f)
                {
                    toPlayer.Normalize();
                    if (toPlayer.X > 0.1f) _facing = Facing.Right;
                    else if (toPlayer.X < -0.1f) _facing = Facing.Left;
                    TryMoveFeet(toPlayer * _speed, tiles);
                }
            }
            else if (_state == EnemyState.Idle && !_stateLocked)
            {
                _idleDirTimer += dt;
                if (_idleDirTimer >= IdleDirSwitch)
                {
                    _facing = _facing == Facing.Right ? Facing.Left : Facing.Right;
                    _idleDirTimer = 0;
                }
            }
            else if (_state == EnemyState.Attack)
            {
                if (playerCenter.X > myCenter.X) _facing = Facing.Right;
                else if (playerCenter.X < myCenter.X) _facing = Facing.Left;
            }

            UpdateAnimation(dt);
        }

        private void TryMoveFeet(Vector2 delta, int[,] tiles)
        {
            if (tiles == null)
            {
                _feetPos += delta;
                return;
            }

            Vector2 newFeet = _feetPos + delta;
            CollideFeet(ref newFeet, tiles);
            _feetPos = newFeet;
        }

        private void CollideFeet(ref Vector2 feet, int[,] tiles)
        {
            int tileSize = 32;
            int mapWidth = tiles.GetLength(1) * tileSize;
            int mapHeight = tiles.GetLength(0) * tileSize;

            // World bounds
            if (feet.X - _collisionWidth * 0.5f < 0) feet.X = _collisionWidth * 0.5f;
            if (feet.X + _collisionWidth * 0.5f > mapWidth) feet.X = mapWidth - _collisionWidth * 0.5f;
            if (feet.Y > mapHeight) feet.Y = mapHeight;
            if (feet.Y - _collisionHeight < 0) feet.Y = _collisionHeight;

            Rectangle box = FeetRect(feet);

            for (int y = 0; y < tiles.GetLength(0); y++)
            {
                for (int x = 0; x < tiles.GetLength(1); x++)
                {
                    if (tiles[y, x] != 2)
                    {
                        Rectangle tileRect = new(x * tileSize, y * tileSize, tileSize, tileSize);
                        if (box.Intersects(tileRect))
                        {
                            feet = _feetPos; // rollback
                            return;
                        }
                    }
                }
            }
        }

        private Rectangle FeetRect(Vector2 feet)
        {
            int left = (int)(feet.X - _collisionWidth * 0.5f);
            int top = (int)(feet.Y - _collisionHeight);
            return new Rectangle(left, top, _collisionWidth, _collisionHeight);
        }

        private void UpdateAnimation(double dt)
        {
            if (_currentAnim == null || _currentAnim.Frames.Count == 0) return;
            if (_animFinished) return;

            _animElapsed += dt;
            while (_animElapsed >= _currentAnim.FrameTime)
            {
                _animElapsed -= _currentAnim.FrameTime;
                _currentFrameIndex++;
                if (_currentFrameIndex >= _currentAnim.Frames.Count)
                {
                    if (_currentAnim.Loop)
                        _currentFrameIndex = 0;
                    else
                    {
                        _currentFrameIndex = _currentAnim.Frames.Count - 1;
                        _animFinished = true;
                    }
                }
            }
        }

        // Player damage (impact frame)
        public bool TryDealDamage(Rectangle playerRect, out int damage)
        {
            damage = 0;
            if (_isDead) return false;
            if (_state != EnemyState.Attack) return false;
            if (_hitCooldownTimer > 0) return false;
            if (_currentAnim == null) return false;
            if (_currentFrameIndex != _attackImpactFrame) return false;
            if (!BoundingBox.Intersects(playerRect)) return false;

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

        public void Draw(SpriteBatch spriteBatch, Vector2 playerTopLeft)
        {
            if (_currentAnim == null || _currentAnim.Frames.Count == 0) return;
            var frame = _currentAnim.Frames[_currentFrameIndex];

            Vector2 drawPos = _feetPos - (frame.Pivot * _scale);
            SpriteEffects fx = _facing == Facing.Right ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            Texture2D sheet = _texIdle;
            if (_currentAnim == _animRun) sheet = _texRun;
            else if (_currentAnim == _animAttack) sheet = _texAttack;
            else if (_currentAnim == _animDeath) sheet = _texDeath;

            spriteBatch.Draw(sheet, drawPos, frame.Src, Color.White, 0f, Vector2.Zero, _scale, fx, 0f);
        }

        public Rectangle BoundingBox => FeetRect(_feetPos);

        // Expose top-left style Position (compat met bestaande code)
        public Vector2 Position => new(_feetPos.X - _collisionWidth * 0.5f, _feetPos.Y - _collisionHeight);
        public EnemyState State => _state;
        public bool IsDead => _isDead;
        public int FrameWidth => _collisionWidth;
        public int FrameHeight => _collisionHeight;
    }
}