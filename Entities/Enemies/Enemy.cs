using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace ForestQuest.Entities.Enemies
{
    public class Enemy
    {
        private Vector2 _feetPos;

        // Movement / detectie
        private float _speed = 1.2f;
        private bool _followingPlayer;
        private float _followDistance = 140f;
        private float _stopFollowDistance = 190f;
        private float _attackRange = 42f;

        private enum Facing { Left, Right }
        private Facing _facing = Facing.Right;
        private double _idleDirTimer;
        private const double IdleDirSwitch = 2.0;

        // State
        private EnemyState _state = EnemyState.Idle;
        private bool _isDead;
        private bool _stateLocked;

        // Health
        private int _maxHealth = 60;
        private int _health;

        // Textures
        private Texture2D _texIdle;
        private Texture2D _texRun;
        private Texture2D _texAttack;
        private Texture2D _texDeath;

        // Anim
        private class AnimFrame { public Rectangle Src; public Vector2 Pivot; }
        private class Animation { public List<AnimFrame> Frames = new(); public double FrameTime; public bool Loop; }
        private Animation _animIdle, _animRun, _animAttack, _animDeath, _currentAnim;
        private int _currentFrameIndex;
        private double _animElapsed;
        private bool _animFinished;

        // Collision
        private int _collisionWidth;
        private int _collisionHeight;
        private float _scale;

        // Attack
        private int _attackImpactFrame = 1;
        private float _hitCooldownTimer;
        private const float HitCooldownDuration = 2f;

        // Meta
        public int LevelVariant { get; }
        public EnemyType Type { get; }
        public int Health => _health;
        public int MaxHealth => _maxHealth;
        public bool IsBoss => Type == EnemyType.Wolf;

        public event Action<int, int>? OnHealthChanged;
        private void RaiseHealthChanged() => OnHealthChanged?.Invoke(_health, _maxHealth);
        private void ChangeHealth(int newValue)
        {
            int clamped = Math.Clamp(newValue, 0, _maxHealth);
            if (clamped == _health) return;
            _health = clamped;
            RaiseHealthChanged();
        }

        // ---- CORRECTE WOLF FRAME COUNTS (idle 8, run 7, attack 7, death 8) ----
        private const int WOLF_IDLE_FRAMES   = 8;
        private const int WOLF_RUN_FRAMES    = 7;
        private const int WOLF_ATTACK_FRAMES = 7;
        private const int WOLF_DEATH_FRAMES  = 8;

        public Enemy(Vector2 startTopLeft, EnemyType type = EnemyType.Cat, int levelVariant = 1)
        {
            _collisionWidth = 1;
            _collisionHeight = 1;
            _feetPos = new Vector2(startTopLeft.X + _collisionWidth * 0.5f, startTopLeft.Y + _collisionHeight);
            LevelVariant = Math.Clamp(levelVariant, 1, 3);
            Type = type;
            _scale = type == EnemyType.Wolf ? 1.5f : 0.5f;
            _health = _maxHealth = type switch
            {
                EnemyType.Cat => 60,
                EnemyType.Dog => 90,
                EnemyType.Wolf => 100,
                _ => 60
            };
        }

        public void LoadContent(ContentManager content)
        {
            string levelDir = $"Enemy/Level{LevelVariant}";
            string baseName = Type switch
            {
                EnemyType.Cat => "cat",
                EnemyType.Dog => "dog",
                EnemyType.Wolf => "wolf",
                _ => "cat"
            };

            _texIdle   = content.Load<Texture2D>($"{levelDir}/{baseName}_idle");
            _texRun    = content.Load<Texture2D>($"{levelDir}/{baseName}_run");
            _texAttack = content.Load<Texture2D>($"{levelDir}/{baseName}_attack");
            _texDeath  = content.Load<Texture2D>($"{levelDir}/{baseName}_death");

            if (Type == EnemyType.Wolf)
            {
                _animIdle   = BuildAnimationSmartWolf(_texIdle,   WOLF_IDLE_FRAMES,   0.14, true);
                _animRun    = BuildAnimationSmartWolf(_texRun,    WOLF_RUN_FRAMES,    0.08, true);
                _animAttack = BuildAnimationSmartWolf(_texAttack, WOLF_ATTACK_FRAMES, 0.10, false);
                _animDeath  = BuildAnimationSmartWolf(_texDeath,  WOLF_DEATH_FRAMES,  0.12, false);
            }
            else
            {
                var (idle, run, attack, death) = Type switch
                {
                    EnemyType.Cat => (4, 6, 4, 4),
                    EnemyType.Dog => (4, 6, 4, 4),
                    _ => (4, 6, 4, 4)
                };
                _animIdle   = BuildAnimationFixedAndTrim(_texIdle,   idle,   0.18, true);
                _animRun    = BuildAnimationFixedAndTrim(_texRun,    run,    0.10, true);
                _animAttack = BuildAnimationFixedAndTrim(_texAttack, attack, 0.12, false);
                _animDeath  = BuildAnimationFixedAndTrim(_texDeath,  death,  0.15, false);
            }

            if (_animAttack.Frames.Count > 0)
                _attackImpactFrame = _animAttack.Frames.Count / 2;

            DeriveCollisionFromIdle();
            SetState(EnemyState.Idle, force: true);
            RaiseHealthChanged();
        }

        // ---------- Wolf specifieke slicing ----------
        private Animation BuildAnimationSmartWolf(Texture2D tex, int expectedFrames, double frameTime, bool loop)
        {
            var anim = new Animation { FrameTime = frameTime, Loop = loop };

            int w = tex.Width;
            int h = tex.Height;
            Color[] pixels = new Color[w * h];
            tex.GetData(pixels);

            int[] opaquePerColumn = new int[w];
            for (int x = 0; x < w; x++)
            {
                int count = 0;
                for (int y = 0; y < h; y++)
                {
                    if (pixels[y * w + x].A > 25)
                        count++;
                }
                opaquePerColumn[x] = count;
            }

            // Een kolom is "leeg" als er vrijwel geen opaque pixels zijn.
            const int EMPTY_THRESHOLD = 1;      // <= 1 opaque pixel => scheiding
            const int MIN_SEG_WIDTH = 8;        // voorkom extreem smalle noise frames
            List<(int start, int end)> segments = new();

            bool inside = false;
            int segStart = 0;

            for (int x = 0; x < w; x++)
            {
                bool isEmpty = opaquePerColumn[x] <= EMPTY_THRESHOLD;
                if (!inside && !isEmpty)
                {
                    inside = true;
                    segStart = x;
                }
                else if (inside && isEmpty)
                {
                    int segEnd = x - 1;
                    if (segEnd - segStart + 1 >= MIN_SEG_WIDTH)
                        segments.Add((segStart, segEnd));
                    inside = false;
                }
            }
            if (inside)
            {
                int segEnd = w - 1;
                if (segEnd - segStart + 1 >= MIN_SEG_WIDTH)
                    segments.Add((segStart, segEnd));
            }

            // Fallback indien geen bruikbare segmenten
            if (segments.Count == 0)
            {
                Debug.WriteLine("[WolfAnim] Geen segmenten gevonden -> uniform fallback.");
                return UniformFallback(tex, expectedFrames, frameTime, loop);
            }

            // Als we méér of minder segmenten hebben dan verwacht -> fallback (uniform + trim)
            if (segments.Count != expectedFrames)
            {
                Debug.WriteLine($"[WolfAnim] Segment mismatch: found {segments.Count}, expected {expectedFrames} -> uniform fallback.");
                return UniformFallback(tex, expectedFrames, frameTime, loop);
            }

            // Trim per segment
            int baseline = 0;
            var temp = new List<AnimFrame>();

            foreach (var (start, end) in segments)
            {
                int minX = end, maxX = start, minY = h - 1, maxY = 0;
                bool any = false;
                for (int y = 0; y < h; y++)
                {
                    for (int x = start; x <= end; x++)
                    {
                        var c = pixels[y * w + x];
                        if (c.A > 25)
                        {
                            any = true;
                            if (x < minX) minX = x;
                            if (x > maxX) maxX = x;
                            if (y < minY) minY = y;
                            if (y > maxY) maxY = y;
                        }
                    }
                }

                Rectangle src;
                if (!any)
                {
                    src = new Rectangle(start, 0, end - start + 1, h);
                }
                else
                {
                    src = new Rectangle(minX, minY, maxX - minX + 1, maxY - minY + 1);
                }

                var frame = new AnimFrame
                {
                    Src = src,
                    Pivot = new Vector2(src.Width / 2f, src.Height)
                };
                temp.Add(frame);
                if (frame.Pivot.Y > baseline) baseline = (int)frame.Pivot.Y;
            }

            // Baseline normaliseren
            foreach (var f in temp)
                f.Pivot = new Vector2(f.Pivot.X, baseline);

            anim.Frames.AddRange(temp);
            return anim;
        }

        private Animation UniformFallback(Texture2D tex, int expectedFrames, double frameTime, bool loop)
        {
            var anim = new Animation { FrameTime = frameTime, Loop = loop };
            int w = tex.Width;
            int h = tex.Height;
            int slice = w / expectedFrames;
            if (slice <= 0) slice = w;

            Color[] pixels = new Color[w * h];
            tex.GetData(pixels);

            int baseline = 0;
            var temp = new List<AnimFrame>();

            for (int i = 0; i < expectedFrames; i++)
            {
                int x0 = i * slice;
                int width = (i == expectedFrames - 1) ? (w - x0) : slice;
                if (x0 >= w) break;
                if (x0 + width > w) width = w - x0;

                Rectangle full = new(x0, 0, width, h);

                // Fine trim binnen slice
                int minX = full.Right, maxX = full.Left - 1, minY = h - 1, maxY = 0;
                bool any = false;
                for (int y = 0; y < h; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        Color c = pixels[y * w + (x0 + x)];
                        if (c.A > 25)
                        {
                            any = true;
                            int gx = x0 + x;
                            if (gx < minX) minX = gx;
                            if (gx > maxX) maxX = gx;
                            if (y < minY) minY = y;
                            if (y > maxY) maxY = y;
                        }
                    }
                }

                Rectangle src = any ? new Rectangle(minX, minY, maxX - minX + 1, maxY - minY + 1) : full;
                var frame = new AnimFrame { Src = src, Pivot = new Vector2(src.Width / 2f, src.Height) };
                temp.Add(frame);
                if (frame.Pivot.Y > baseline) baseline = (int)frame.Pivot.Y;
            }

            foreach (var f in temp)
                f.Pivot = new Vector2(f.Pivot.X, baseline);

            anim.Frames.AddRange(temp);
            return anim;
        }

        // ---------- Kat/Hond vaste slicing + trim ----------
        private Animation BuildAnimationFixedAndTrim(Texture2D tex, int frameCount, double frameTime, bool loop)
        {
            var anim = new Animation { FrameTime = frameTime, Loop = loop };

            if (frameCount <= 0) frameCount = 1;
            int rawFrameWidth = tex.Width / frameCount;
            if (rawFrameWidth * frameCount != tex.Width)
            {
                Debug.WriteLine($"[Enemy] WARNING: width {tex.Width} not divisible by frameCount {frameCount}");
            }
            int rawFrameHeight = tex.Height;

            Color[] pixels = new Color[tex.Width * tex.Height];
            tex.GetData(pixels);

            int baseline = 0;
            var temp = new List<AnimFrame>();

            for (int i = 0; i < frameCount; i++)
            {
                int x0 = i * rawFrameWidth;
                if (x0 >= tex.Width) break;
                int w = Math.Min(rawFrameWidth, tex.Width - x0);
                Rectangle full = new(x0, 0, w, rawFrameHeight);

                int minX = full.Right, maxX = full.Left - 1, minY = full.Bottom - 1, maxY = full.Top;
                bool any = false;
                for (int y = 0; y < full.Height; y++)
                {
                    int gy = y;
                    for (int x = 0; x < full.Width; x++)
                    {
                        int gx = x0 + x;
                        var c = pixels[gy * tex.Width + gx];
                        if (c.A > 15)
                        {
                            any = true;
                            if (gx < minX) minX = gx;
                            if (gx > maxX) maxX = gx;
                            if (gy < minY) minY = gy;
                            if (gy > maxY) maxY = gy;
                        }
                    }
                }

                Rectangle src = any ? new Rectangle(minX, minY, maxX - minX + 1, maxY - minY + 1) : full;
                var frame = new AnimFrame
                {
                    Src = src,
                    Pivot = new Vector2(src.Width / 2f, src.Height)
                };
                temp.Add(frame);
                if (frame.Pivot.Y > baseline) baseline = (int)frame.Pivot.Y;
            }

            foreach (var f in temp)
                f.Pivot = new Vector2(f.Pivot.X, baseline);

            anim.Frames.AddRange(temp);
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
            ApplyDamage(_health);
        }

        public void ApplyDamage(int amount)
        {
            if (_isDead) return;
            ChangeHealth(_health - amount);
            if (_health <= 0)
            {
                _isDead = true;
                SetState(EnemyState.Death, force: true);
            }
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

            Vector2 playerCenter = playerTopLeft;
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
                            feet = _feetPos;
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

        public bool TryDealDamage(Rectangle playerRect, out int damage)
        {
            damage = 0;
            if (_isDead) return false;
            if (_state != EnemyState.Attack) return false;
            if (_hitCooldownTimer > 0) return false;
            if (_currentAnim == null) return false;
            if (_currentFrameIndex != _attackImpactFrame) return false;
            if (!BoundingBox.Intersects(playerRect)) return false;

            damage = Type switch
            {
                EnemyType.Cat => 10,
                EnemyType.Dog => 18,
                EnemyType.Wolf => 30,
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
        public Vector2 Position => new(_feetPos.X - _collisionWidth * 0.5f, _feetPos.Y - _collisionHeight);
        public EnemyState State => _state;
        public bool IsDead => _isDead;
        public int FrameWidth => _collisionWidth;
        public int FrameHeight => _collisionHeight;
    }
}