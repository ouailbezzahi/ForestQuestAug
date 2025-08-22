using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace ForestQuest.Entities.Player
{
    public class Player
    {
        private Vector2 _feetPos;
        private float _baseSpeed = 3f;
        private float _speed;
        // Acceleration: start slower and ramp to _baseSpeed
        private float _minSpeedFactor = 0.25f;   // 25% of base when starting to move
        private float _accelDuration = 0.35f;    // seconds to reach full speed
        private float _accelTimer = 0f;

        private readonly float _scale = 0.8f;

        private PlayerState _state = PlayerState.Idle;
        private bool _stateLocked;

        private int _maxHealth = 100;
        private int _health;
        public int Health => _health;
        public bool IsDead => _state == PlayerState.Death;

        private enum Facing { Left, Right }
        private Facing _facing = Facing.Right;

        private Texture2D _texIdle;
        private Texture2D _texRun;
        private Texture2D _texAttack;
        private Texture2D _texHurt;
        private Texture2D _texDeath;

        private SoundEffect _sfxHurt;
        private SoundEffect _sfxDeath;

        private class AnimFrame { public Rectangle Src; public Vector2 Pivot; }
        private class Animation { public System.Collections.Generic.List<AnimFrame> Frames = new(); public double FrameTime; public bool Loop; }

        private Animation _animIdle;
        private Animation _animRun;
        private Animation _animAttack;
        private Animation _animHurt;
        private Animation _animDeath;
        private Animation _currentAnim;

        private int _currentFrameIndex;
        private double _animElapsed;
        private bool _animFinished;

        private int _collisionWidth;
        private int _collisionHeight;

        private double _attackCooldown = 0.5;
        private double _attackLockTime = 0.40;
        private double _attackCooldownTimer;
        private double _attackLockTimer;
        private double _hurtLockDuration = 0.35;
        private double _hurtLockTimer;

        private int _attackActiveStart = 1;
        private int _attackActiveEnd = 2;

        public bool IsAttackActive =>
            _state == PlayerState.Attack &&
            !_animFinished &&
            _currentFrameIndex >= _attackActiveStart &&
            _currentFrameIndex <= _attackActiveEnd;

        private readonly Camera _camera = new();

        public Vector2 Position => new(_feetPos.X - _collisionWidth * 0.5f, _feetPos.Y - _collisionHeight);
        public Vector2 Center => new(_feetPos.X, _feetPos.Y - _collisionHeight * 0.5f);
        public Matrix CameraTransform => _camera.Transform;
        public int FrameWidth => _collisionWidth;
        public int FrameHeight => _collisionHeight;

        public event Action<int, int>? OnHealthChanged;

        private readonly PlayerControls _controls;
        public PlayerControls Controls => _controls;

        public Player(Vector2 startTopLeft, PlayerControls controls, int levelVariant = 1)
        {
            _controls = controls ?? PlayerControls.Default1;
            _collisionWidth = 1;
            _collisionHeight = 1;
            _feetPos = new Vector2(startTopLeft.X + _collisionWidth * 0.5f, startTopLeft.Y + _collisionHeight);
            _speed = _baseSpeed;
            _health = _maxHealth;
        }

        public void LoadContent(ContentManager content)
        {
            _texIdle = content.Load<Texture2D>($"Player/Level1/hero_level1_idle");
            _texRun = content.Load<Texture2D>($"Player/Level1/hero_level1_run");
            _texAttack = content.Load<Texture2D>($"Player/Level1/hero_level1_attack");
            _texHurt = content.Load<Texture2D>($"Player/Level1/hero_level1_hurt");
            _texDeath = content.Load<Texture2D>($"Player/Level1/hero_level1_death");

            _sfxHurt = SafeLoadSfx(content, "Audio/hero_hurt");
            _sfxDeath = SafeLoadSfx(content, "Audio/hero_death");

            _animIdle = BuildAnimation(_texIdle, 4, 0.15, true);
            _animRun = BuildAnimation(_texRun, 6, 0.09, true);
            _animAttack = BuildAnimation(_texAttack, 4, 0.15, false);
            _animHurt = BuildAnimation(_texHurt, 2, 0.18, false);
            _animDeath = BuildAnimation(_texDeath, 4, 0.25, false);

            DeriveCollisionBoxFromIdle();
            SetState(PlayerState.Idle, force: true);
            RaiseHealthChanged();
        }

        private SoundEffect SafeLoadSfx(ContentManager content, string asset)
        {
            try { return content.Load<SoundEffect>(asset); } catch { return null; }
        }

        private Animation BuildAnimation(Texture2D tex, int frameCount, double frameTime, bool loop)
        {
            var anim = new Animation { FrameTime = frameTime, Loop = loop };
            int rawFrameWidth = tex.Width / frameCount;
            int rawFrameHeight = tex.Height;
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
                        Color c = pixels[yGlobal * tex.Width + xGlobal];
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
                    float pivotX = (src.Width / 2f);
                    float pivotY = src.Height;
                    pivot = new Vector2(pivotX, pivotY);
                }

                anim.Frames.Add(new AnimFrame { Src = src, Pivot = pivot });
            }

            return anim;
        }

        private void DeriveCollisionBoxFromIdle()
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

        private void SetState(PlayerState newState, bool force = false)
        {
            if (!force && newState == _state) return;
            if (IsDead && newState != PlayerState.Death) return;

            _state = newState;
            _currentFrameIndex = 0;
            _animElapsed = 0;
            _animFinished = false;

            switch (_state)
            {
                case PlayerState.Idle: _currentAnim = _animIdle; _stateLocked = false; break;
                case PlayerState.Run: _currentAnim = _animRun; _stateLocked = false; break;
                case PlayerState.Attack: _currentAnim = _animAttack; _stateLocked = true; _attackLockTimer = _attackLockTime; break;
                case PlayerState.Hurt: _currentAnim = _animHurt; _stateLocked = true; _hurtLockTimer = _hurtLockDuration; break;
                case PlayerState.Death: _currentAnim = _animDeath; _stateLocked = true; break;
                default: _currentAnim = _animIdle; _stateLocked = false; break;
            }
        }

        public void StartAttack()
        {
            if (IsDead) return;
            if (_attackCooldownTimer > 0) return;
            if (_state == PlayerState.Attack || _state == PlayerState.Hurt) return;
            _attackCooldownTimer = _attackCooldown;
            SetState(PlayerState.Attack, force: true);
        }

        private void RaiseHealthChanged() => OnHealthChanged?.Invoke(_health, _maxHealth);

        private void ChangeHealth(int newValue)
        {
            int clamped = Math.Clamp(newValue, 0, _maxHealth);
            if (clamped == _health) return;
            _health = clamped;
            RaiseHealthChanged();
        }

        public void ApplyDamage(int amount)
        {
            if (IsDead) return;
            if (_state == PlayerState.Hurt && !_animFinished) return;

            ChangeHealth(_health - amount);

            if (_health <= 0)
            {
                SetState(PlayerState.Death, force: true);
                _sfxDeath?.Play();
            }
            else
            {
                SetState(PlayerState.Hurt, force: true);
                _sfxHurt?.Play();
            }
        }

        public void Heal(int amount)
        {
            if (IsDead) return;
            ChangeHealth(_health + amount);
        }

        public void Update(KeyboardState kb, GameTime gt, Viewport vp, int[,] tiles)
        {
            double dt = gt.ElapsedGameTime.TotalSeconds;

            if (_attackCooldownTimer > 0) _attackCooldownTimer -= dt;
            if (_attackLockTimer > 0) _attackLockTimer -= dt;
            if (_hurtLockTimer > 0) _hurtLockTimer -= dt;

            if (IsDead)
            {
                UpdateAnimation(dt);
                FollowCamera(vp, tiles);
                return;
            }

            if (kb.IsKeyDown(_controls.Attack))
                StartAttack();

            // Acceleration update happens BEFORE using _speed to build delta
            bool hasMoveInput = !_stateLocked &&
                (kb.IsKeyDown(_controls.Up) || kb.IsKeyDown(_controls.Down) ||
                 kb.IsKeyDown(_controls.Left) || kb.IsKeyDown(_controls.Right));

            if (hasMoveInput)
            {
                _accelTimer = Math.Min(_accelTimer + (float)dt, _accelDuration);
            }
            else
            {
                _accelTimer = 0f;
            }
            float t = _accelDuration <= 0f ? 1f : Math.Clamp(_accelTimer / _accelDuration, 0f, 1f);
            float minSpeed = _baseSpeed * _minSpeedFactor;
            _speed = MathHelper.SmoothStep(minSpeed, _baseSpeed, t);

            Vector2 delta = Vector2.Zero;
            if (!_stateLocked)
            {
                if (kb.IsKeyDown(_controls.Up)) delta.Y -= _speed;
                if (kb.IsKeyDown(_controls.Down)) delta.Y += _speed;
                if (kb.IsKeyDown(_controls.Left)) { delta.X -= _speed; _facing = Facing.Left; }
                if (kb.IsKeyDown(_controls.Right)) { delta.X += _speed; _facing = Facing.Right; }
            }

            if (!_stateLocked)
            {
                if (delta != Vector2.Zero) SetState(PlayerState.Run);
                else SetState(PlayerState.Idle);
            }
            else
            {
                if (_state == PlayerState.Attack && (_animFinished || _attackLockTimer <= 0))
                {
                    _stateLocked = false;
                    SetState(delta != Vector2.Zero ? PlayerState.Run : PlayerState.Idle, force: true);
                }
                else if (_state == PlayerState.Hurt && (_animFinished || _hurtLockTimer <= 0))
                {
                    _stateLocked = false;
                    SetState(delta != Vector2.Zero ? PlayerState.Run : PlayerState.Idle, force: true);
                }
            }

            if (delta != Vector2.Zero)
            {
                Vector2 newFeet = _feetPos + delta;
                CollideFeet(ref newFeet, tiles);
                _feetPos = newFeet;
            }

            UpdateAnimation(dt);
            FollowCamera(vp, tiles);
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

        private void CollideFeet(ref Vector2 feet, int[,] tiles)
        {
            Rectangle box = FeetCollisionRect(feet);

            int tileSize = 32;
            int mapWidth = tiles.GetLength(1) * tileSize;
            int mapHeight = tiles.GetLength(0) * tileSize;

            if (feet.X - _collisionWidth * 0.5f < 0) feet.X = _collisionWidth * 0.5f;
            if (feet.X + _collisionWidth * 0.5f > mapWidth) feet.X = mapWidth - _collisionWidth * 0.5f;
            if (feet.Y > mapHeight) feet.Y = mapHeight;
            if (feet.Y - _collisionHeight < 0) feet.Y = _collisionHeight;

            box = FeetCollisionRect(feet);

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

        private Rectangle FeetCollisionRect(Vector2 feet)
        {
            int left = (int)(feet.X - _collisionWidth * 0.5f);
            int top = (int)(feet.Y - _collisionHeight);
            return new Rectangle(left, top, _collisionWidth, _collisionHeight);
        }

        public Rectangle GetAttackHitbox()
        {
            var box = FeetCollisionRect(_feetPos);
            int reach = (int)(box.Width * 0.6f);
            if (_facing == Facing.Right)
                return new Rectangle(box.Right, box.Top + box.Height / 4, reach, box.Height / 2);
            else
                return new Rectangle(box.Left - reach, box.Top + box.Height / 4, reach, box.Height / 2);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (_currentAnim == null || _currentAnim.Frames.Count == 0) return;
            var frame = _currentAnim.Frames[_currentFrameIndex];

            Vector2 drawPos = _feetPos - (frame.Pivot * _scale);
            SpriteEffects fx = _facing == Facing.Right ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            Texture2D sheet = _texIdle;
            if (_currentAnim == _animRun) sheet = _texRun;
            else if (_currentAnim == _animAttack) sheet = _texAttack;
            else if (_currentAnim == _animHurt) sheet = _texHurt;
            else if (_currentAnim == _animDeath) sheet = _texDeath;

            spriteBatch.Draw(sheet, drawPos, frame.Src, Color.White, 0f, Vector2.Zero, _scale, fx, 0f);
        }

        private void FollowCamera(Viewport viewport, int[,] tiles)
        {
            int tileSize = 32;
            int mapWidth = tiles.GetLength(1) * tileSize;
            int mapHeight = tiles.GetLength(0) * tileSize;
            _camera.Follow(this, viewport, mapWidth, mapHeight);
        }

        private class Camera
        {
            public Vector2 Position { get; private set; }
            public Matrix Transform { get; private set; }

            public void Follow(Player player, Viewport vp, int mapWidth, int mapHeight)
            {
                Vector2 center = new(player._feetPos.X, player._feetPos.Y - player._collisionHeight * 0.5f);
                Vector2 cam = new(center.X - vp.Width / 2f, center.Y - vp.Height / 2f);
                cam.X = MathHelper.Clamp(cam.X, 0, Math.Max(0, mapWidth - vp.Width));
                cam.Y = MathHelper.Clamp(cam.Y, 0, Math.Max(0, mapHeight - vp.Height));
                Position = cam;
                Transform = Matrix.CreateTranslation(-cam.X, -cam.Y, 0);
            }
        }

        public void ResolveCollisionWith(Rectangle obstacle, int[,] tiles)
        {
            // Current player collision rectangle
            Rectangle me = FeetCollisionRect(_feetPos);
            Rectangle inter = Rectangle.Intersect(me, obstacle);
            if (inter.Width <= 0 || inter.Height <= 0)
                return;

            // Move out along the smallest overlap axis
            Vector2 adjust = Vector2.Zero;
            if (inter.Width < inter.Height)
            {
                // Resolve horizontally
                int myCenterX = me.Center.X;
                int obCenterX = obstacle.Center.X;
                adjust.X = (myCenterX < obCenterX) ? -inter.Width : inter.Width;
            }
            else
            {
                // Resolve vertically
                int myCenterY = me.Center.Y;
                int obCenterY = obstacle.Center.Y;
                adjust.Y = (myCenterY < obCenterY) ? -inter.Height : inter.Height;
            }

            Vector2 newFeet = new(_feetPos.X + adjust.X, _feetPos.Y + adjust.Y);

            // Validate against tiles and map bounds
            CollideFeet(ref newFeet, tiles);
            _feetPos = newFeet;
        }
    }
}