using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;
using System.Collections.Generic;
using System.Linq;
using ForestQuest.Items.Coin;
using ForestQuest.UI;
using ForestQuest.Entities.Enemies;
using ForestQuest.Entities.Player;
using System;

namespace ForestQuest.State
{
    public class GameState : State
    {
        private readonly int _level;

        // Tiles
        private Texture2D _houseTile;
        private Texture2D _treeTile;
        private Texture2D _grassTile;

        private Texture2D _borderLeftUp;
        private Texture2D _borderLeftDown;
        private Texture2D _borderRightUp;
        private Texture2D _borderRightDown;
        private Texture2D _borderVertical;
        private Texture2D _borderHorizontal;

        // Players & audio
        private Player _player;
        private Player _player2;
        private Song _backgroundMusic;
        private SoundEffect _playerMoveSound;
        private SoundEffectInstance _playerMoveSoundInstance;
        private float _footstepTimer = 0f;
        private SoundEffect? _coinPickupSound;

        // Menus & UI
        private bool _isMultiplayer;
        private bool _isPaused = false;
        private PauseMenu _pauseMenu;
        private OptionsMenu _optionsMenu;
        private bool _showingOptions = false;

        private int _coinCount = 0;
        private CoinManager _coinManager;
        private CoinCounter _coinCounter;
        private HealthBar _healthBar;
        private HealthBar _healthBar2;
        private DialogBox? _dialogBox;

        // Enemies
        private List<Enemy> _enemies;
        private EnemyCounter _enemyCounter;
        private int _totalEnemies;
        private int _enemiesKilled;
        public int TotalEnemies => _totalEnemies;
        public int EnemiesKilled => _enemiesKilled;

        // Boss
        private Enemy? _bossEnemy;
        private BossHealthBar? _bossHealthBar;
        private const int PLAYER_WOLF_ATTACK_DAMAGE = 20;

        // Track enemies hit per player's attack
        private readonly HashSet<Enemy> _hitEnemiesP1 = new();
        private readonly HashSet<Enemy> _hitEnemiesP2 = new();
        private bool _prevAttackActiveP1;
        private bool _prevAttackActiveP2;

        // State
        private bool _gameOverTriggered;
        private bool _victoryTriggered;

        private int _totalCoins;

        // UI helpers
        private SpriteFont _uiFont;
        private Texture2D _pixel;

        private int[,] _backgroundTiles = new int[,]
        {
            { 3,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,5 },
            { 7,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,7 },
            { 7,2,2,2,1,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,1,2,7 },
            { 7,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,7 },
            { 7,2,2,2,2,2,2,2,2,2,2,2,2,2,2,0,2,2,2,2,2,2,2,2,2,2,2,2,2,2,7 },
            { 7,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,7 },
            { 7,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,7 },
            { 7,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,7 },
            { 7,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,7 },
            { 7,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,7 },
            { 7,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,7 },
            { 7,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,7 },
            { 7,2,2,2,2,2,2,1,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,7 },
            { 7,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,7 },
            { 7,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,7 },
            { 7,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,7 },
            { 7,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,1,2,2,2,2,2,2,2,2,2,2,7 },
            { 7,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,7 },
            { 7,2,2,2,1,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,1,2,7 },
            { 7,2,2,1,1,2,2,2,2,2,1,1,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,7 },
            { 7,2,2,2,2,2,2,2,2,2,2,1,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,7 },
            { 7,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,7 },
            { 7,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,7 },
            { 7,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,7 },
            { 7,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,7 },
            { 7,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,7 },
            { 7,2,1,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,7 },
            { 7,2,1,1,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,7 },
            { 7,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,0,2,2,2,2,7 },
            { 7,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,7 },
            { 7,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,7 },
            { 4,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,6 },
        };

        public GameState(Game1 game, ContentManager content, GraphicsDevice graphicsDevice, bool isMultiplayer, int level = 3)
            : base(game, content, graphicsDevice)
        {
            _isMultiplayer = isMultiplayer;
            _level = level;
        }

        public override void LoadContent()
        {
            _houseTile = _content.Load<Texture2D>("Background/House/Slice 10");
            _treeTile = _content.Load<Texture2D>("Background/Tree/Slice 12");
            _grassTile = _content.Load<Texture2D>("Background/Grass/Slice 21");

            _borderLeftUp = _content.Load<Texture2D>("Background/Wooden Border/Slice 3");
            _borderLeftDown = _content.Load<Texture2D>("Background/Wooden Border/Slice 5");
            _borderRightUp = _content.Load<Texture2D>("Background/Wooden Border/Slice 1");
            _borderRightDown = _content.Load<Texture2D>("Background/Wooden Border/Slice 7");
            _borderVertical = _content.Load<Texture2D>("Background/Wooden Border/Slice 4");
            _borderHorizontal = _content.Load<Texture2D>("Background/Wooden Border/Slice 2");

            _backgroundMusic = _content.Load<Song>("Audio/background_music");
            _playerMoveSound = _content.Load<SoundEffect>("Audio/footsteps");
            _playerMoveSoundInstance = _playerMoveSound.CreateInstance();
            _playerMoveSoundInstance.IsLooped = false;

            try { _coinPickupSound = _content.Load<SoundEffect>("Audio/get_coin"); } catch { _coinPickupSound = null; }

            MediaPlayer.IsRepeating = true;
            MediaPlayer.Play(_backgroundMusic);

            _player = new Player(new Vector2(15 * 32, 15 * 32), PlayerControls.Default1, _level);
            _player.LoadContent(_content);

            if (_isMultiplayer)
            {
                // Spawn P2 slightly to the right
                _player2 = new Player(new Vector2(17 * 32, 15 * 32), PlayerControls.Default2, _level);
                _player2.LoadContent(_content);
            }

            _healthBar = new HealthBar(_content, 100);
            _healthBar.SetHealth(_player.Health);
            _player.OnHealthChanged += (cur, max) => _healthBar.SetHealth(cur);

            if (_isMultiplayer)
            {
                _healthBar2 = new HealthBar(_content, 100);
                _healthBar2.SetHealth(_player2.Health);
                _player2.OnHealthChanged += (cur, max) => _healthBar2.SetHealth(cur);
            }

            _pauseMenu = new PauseMenu(_content, _graphicsDevice);
            _optionsMenu = new OptionsMenu(_content, _graphicsDevice);

            int tileSize = 32;
            int mapWidth = _backgroundTiles.GetLength(1) * tileSize;
            int mapHeight = _backgroundTiles.GetLength(0) * tileSize;

            int coinSpawn = _level switch
            {
                1 => 10,
                2 => 15,
                3 => 12,
                _ => 10
            };
            _coinManager = new CoinManager(_content, mapWidth / tileSize, mapHeight / tileSize, coinSpawn);
            _coinCounter = new CoinCounter(_content);
            _totalCoins = _coinManager.Coins.Count;

            _dialogBox = _level == 1
                ? new DialogBox(_content, "Welkom in Forest Quest! ... (Level 1)")
                : null;

            _enemies = new List<Enemy>();

            if (_level == 1)
            {
                var catSpawns = new[]
                {
                    new Vector2(100,100),
                    new Vector2(300,100),
                    new Vector2(500,100),
                    new Vector2(100,300),
                    new Vector2(300,300)
                };
                foreach (var pos in catSpawns)
                {
                    var e = new Enemy(pos, EnemyType.Cat, 1);
                    e.LoadContent(_content);
                    _enemies.Add(e);
                }
            }
            else if (_level == 2)
            {
                var dogSpawns = new[]
                {
                    new Vector2(120,120),
                    new Vector2(480,140),
                    new Vector2(720,180),
                    new Vector2(200,420),
                    new Vector2(520,440),
                    new Vector2(840,300),
                    new Vector2(300,600),
                    new Vector2(660,620)
                };
                foreach (var pos in dogSpawns)
                {
                    var e = new Enemy(pos, EnemyType.Dog, 2);
                    e.LoadContent(_content);
                    _enemies.Add(e);
                }
            }
            else if (_level == 3)
            {
                var wolf = new Enemy(new Vector2(600, 400), EnemyType.Wolf, 3);
                wolf.LoadContent(_content);
                _enemies.Add(wolf);
                // (Eventueel extra dogs toevoegen)
            }

            _totalEnemies = _enemies.Count;
            _enemiesKilled = 0;

            _enemyCounter = new EnemyCounter(_content);
            _enemyCounter.Set(_enemiesKilled, _totalEnemies);

            _bossEnemy = _enemies.FirstOrDefault(e => e.Type == EnemyType.Wolf);
            if (_bossEnemy != null)
            {
                _bossHealthBar = new BossHealthBar(_content, _graphicsDevice, _bossEnemy.MaxHealth)
                {
                    Area = new Rectangle(250, 20, 260, 22)
                };
                _bossEnemy.OnHealthChanged += (cur, max) => _bossHealthBar.Set(cur, max);
            }

            // UI helpers
            _uiFont = _content.Load<SpriteFont>("Fonts/Font");
            _pixel = new Texture2D(_graphicsDevice, 1, 1);
            _pixel.SetData(new[] { Color.White });
        }

        private void StopFootsteps()
        {
            if (_playerMoveSoundInstance is { State: SoundState.Playing })
                _playerMoveSoundInstance.Stop();
        }

        public override void Update(GameTime gameTime)
        {
            if (_dialogBox is { IsVisible: true })
            {
                _dialogBox.Update();
                return;
            }

            var kb = Keyboard.GetState();

            if (!_isPaused && (kb.IsKeyDown(Keys.Escape) || kb.IsKeyDown(Keys.P)))
            {
                _isPaused = true;
                return;
            }
            else if (_isPaused && !_showingOptions && (kb.IsKeyDown(Keys.Escape) || kb.IsKeyDown(Keys.P)))
            {
                _isPaused = false;
                return;
            }

            if (_isPaused)
            {
                if (_showingOptions)
                {
                    MediaPlayer.Volume = _optionsMenu.SoundValue / 100f;
                    _playerMoveSoundInstance.Volume = _optionsMenu.SFXValue / 100f;
                    bool closeOptions = _optionsMenu.Update(_graphicsDevice);
                    if (closeOptions) _showingOptions = false;
                    return;
                }
                int option = _pauseMenu.Update();
                if (option == 0) _isPaused = false;
                else if (option == 1)
                {
                    MediaPlayer.Stop();
                    StopFootsteps();
                    _game.ChangeState(new MenuState(_game, _content, _graphicsDevice));
                    return;
                }
                else if (option == 2) _showingOptions = true;
                else if (option == 3) _game.Exit();
                return;
            }

            // Update players (camera follow in Player uses full viewport; Draw will compute per-viewport cameras)
            _player.Update(kb, gameTime, _graphicsDevice.Viewport, _backgroundTiles);
            if (_isMultiplayer)
                _player2.Update(kb, gameTime, _graphicsDevice.Viewport, _backgroundTiles);

            // Enemies chase the nearest player
            foreach (var enemy in _enemies)
            {
                Vector2 target = _player.Position;
                if (_isMultiplayer)
                {
                    float d1 = Vector2.Distance(enemy.Position, _player.Center);
                    float d2 = Vector2.Distance(enemy.Position, _player2.Center);
                    target = d1 <= d2 ? _player.Position : _player2.Position;
                }
                enemy.Update(gameTime, target, _backgroundTiles);
            }

            // Reset per-player hit sets when their attacks finish
            if (!_player.IsAttackActive && _prevAttackActiveP1)
                _hitEnemiesP1.Clear();
            if (_isMultiplayer && !_player2.IsAttackActive && _prevAttackActiveP2)
                _hitEnemiesP2.Clear();

            // Resolve attacks for both players
            if (_player.IsAttackActive)
            {
                Rectangle attackHit = _player.GetAttackHitbox();
                foreach (var enemy in _enemies)
                {
                    if (!enemy.IsDead &&
                        attackHit.Intersects(enemy.BoundingBox) &&
                        !_hitEnemiesP1.Contains(enemy))
                    {
                        if (enemy.Type == EnemyType.Wolf)
                            enemy.ApplyDamage(PLAYER_WOLF_ATTACK_DAMAGE);
                        else
                            enemy.Kill();

                        _hitEnemiesP1.Add(enemy);

                        if (enemy.IsDead)
                        {
                            _enemiesKilled++;
                            _enemyCounter.Set(_enemiesKilled, _totalEnemies);
                        }
                    }
                }
            }

            if (_isMultiplayer && _player2.IsAttackActive)
            {
                Rectangle attackHit = _player2.GetAttackHitbox();
                foreach (var enemy in _enemies)
                {
                    if (!enemy.IsDead &&
                        attackHit.Intersects(enemy.BoundingBox) &&
                        !_hitEnemiesP2.Contains(enemy))
                    {
                        if (enemy.Type == EnemyType.Wolf)
                            enemy.ApplyDamage(PLAYER_WOLF_ATTACK_DAMAGE);
                        else
                            enemy.Kill();

                        _hitEnemiesP2.Add(enemy);

                        if (enemy.IsDead)
                        {
                            _enemiesKilled++;
                            _enemyCounter.Set(_enemiesKilled, _totalEnemies);
                        }
                    }
                }
            }

            _prevAttackActiveP1 = _player.IsAttackActive;
            if (_isMultiplayer) _prevAttackActiveP2 = _player2.IsAttackActive;

            // Enemy damage to players
            Rectangle p1Rect = new((int)_player.Position.X, (int)_player.Position.Y, _player.FrameWidth, _player.FrameHeight);
            foreach (var enemy in _enemies)
                if (enemy.TryDealDamage(p1Rect, out int dmg1))
                    _player.ApplyDamage(dmg1);

            if (_isMultiplayer)
            {
                Rectangle p2Rect = new((int)_player2.Position.X, (int)_player2.Position.Y, _player2.FrameWidth, _player2.FrameHeight);
                foreach (var enemy in _enemies)
                    if (enemy.TryDealDamage(p2Rect, out int dmg2))
                        _player2.ApplyDamage(dmg2);
            }

            // Coins: both players can pick up using their own Interact key
            _coinManager.Update(gameTime);
            for (int i = _coinManager.Coins.Count - 1; i >= 0; i--)
            {
                var coin = _coinManager.Coins[i];

                bool picked = false;
                Rectangle rect1 = new((int)_player.Position.X, (int)_player.Position.Y, _player.FrameWidth, _player.FrameHeight);
                if (coin.BoundingBox.Intersects(rect1) && kb.IsKeyDown(_player.Controls.Interact))
                {
                    picked = true;
                }

                if (_isMultiplayer && !picked)
                {
                    Rectangle rect2 = new((int)_player2.Position.X, (int)_player2.Position.Y, _player2.FrameWidth, _player2.FrameHeight);
                    if (coin.BoundingBox.Intersects(rect2) && kb.IsKeyDown(_player2.Controls.Interact))
                    {
                        picked = true;
                    }
                }

                if (picked)
                {
                    _coinManager.Coins.RemoveAt(i);
                    _coinCounter.AddCoins(1);
                    _coinCount++;
                    float sfxVolume = (_optionsMenu != null) ? _optionsMenu.SFXValue / 100f : 1f;
                    _coinPickupSound?.Play(sfxVolume, 0f, 0f);
                }
            }

            // Footsteps for either player moving
            if (!_gameOverTriggered && !_victoryTriggered && !_player.IsDead && (!_isMultiplayer || !_player2.IsDead))
            {
                const float footstepInterval = 0.3f;
                _footstepTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;

                bool moving1 = kb.IsKeyDown(_player.Controls.Up) || kb.IsKeyDown(_player.Controls.Left) ||
                               kb.IsKeyDown(_player.Controls.Down) || kb.IsKeyDown(_player.Controls.Right);
                bool moving2 = _isMultiplayer && (kb.IsKeyDown(_player2.Controls.Up) || kb.IsKeyDown(_player2.Controls.Left) ||
                                                  kb.IsKeyDown(_player2.Controls.Down) || kb.IsKeyDown(_player2.Controls.Right));
                bool anyMoving = moving1 || moving2;

                if (anyMoving && _footstepTimer >= footstepInterval)
                {
                    if (_playerMoveSoundInstance.State != SoundState.Playing)
                        _playerMoveSoundInstance.Play();
                    _footstepTimer = 0f;
                }
                else if (!anyMoving && _playerMoveSoundInstance.State == SoundState.Playing)
                {
                    _playerMoveSoundInstance.Stop();
                }
            }
            else
            {
                StopFootsteps();
            }

            if (!_victoryTriggered &&
                (!_player.IsDead || (_isMultiplayer && !_player2.IsDead)) &&
                _enemiesKilled == _totalEnemies &&
                _coinManager.Coins.Count == 0)
            {
                _victoryTriggered = true;
                MediaPlayer.Stop();
                StopFootsteps();
                _game.ChangeState(new VictoryState(
                    _game,
                    _content,
                    _graphicsDevice,
                    currentLevel: _level,
                    isMultiplayer: _isMultiplayer,
                    coinsCollected: _coinCount,
                    totalCoins: _totalCoins,
                    enemiesKilled: _enemiesKilled,
                    totalEnemies: _totalEnemies));
                return;
            }

            // Game over: single player -> when P1 dies; multiplayer -> when both die
            bool gameOver = !_isMultiplayer ? _player.IsDead : (_player.IsDead && _player2.IsDead);
            if (!_gameOverTriggered && gameOver && !_victoryTriggered)
            {
                _gameOverTriggered = true;
                MediaPlayer.Stop();
                StopFootsteps();
                _game.ChangeState(new GameOverState(
                    _game,
                    _content,
                    _graphicsDevice,
                    coinsCollected: _coinCount,
                    enemiesKilled: _enemiesKilled,
                    totalEnemies: _totalEnemies));
            }
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            _graphicsDevice.Clear(Color.CornflowerBlue);

            // Save original viewport
            var originalViewport = _graphicsDevice.Viewport;

            int tileSize = 32;
            float grassScale = 2f;

            void DrawWorld(SpriteBatch sb)
            {
                for (int y = 0; y < _backgroundTiles.GetLength(0); y++)
                    for (int x = 0; x < _backgroundTiles.GetLength(1); x++)
                        if (_backgroundTiles[y, x] == 2)
                            sb.Draw(_grassTile, new Vector2(x * tileSize, y * tileSize),
                                null, Color.White, 0f, Vector2.Zero, new Vector2(grassScale), SpriteEffects.None, 0f);

                for (int y = 0; y < _backgroundTiles.GetLength(0); y++)
                {
                    for (int x = 0; x < _backgroundTiles.GetLength(1); x++)
                    {
                        Texture2D? tileTexture = null;
                        Vector2 position = new(x * tileSize, y * tileSize);
                        switch (_backgroundTiles[y, x])
                        {
                            case 0: tileTexture = _houseTile; break;
                            case 1: tileTexture = _treeTile; break;
                            case 3: tileTexture = _borderLeftUp; break;
                            case 4: tileTexture = _borderLeftDown; break;
                            case 5: tileTexture = _borderRightUp; break;
                            case 6: tileTexture = _borderRightDown; break;
                            case 7: tileTexture = _borderVertical; break;
                            case 8: tileTexture = _borderHorizontal; break;
                        }
                        if (tileTexture != null)
                        {
                            if (_backgroundTiles[y, x] is >= 3 and <= 8)
                                sb.Draw(tileTexture, new Rectangle((int)position.X, (int)position.Y, tileSize, tileSize), Color.White);
                            else
                                sb.Draw(tileTexture, position, Color.White);
                        }
                    }
                }

                _coinManager.Draw(sb);
                _player.Draw(sb);
                if (_isMultiplayer) _player2.Draw(sb);
                foreach (var enemy in _enemies)
                    enemy.Draw(sb, _player.Position);
            }

            // Helper: build a camera transform centered on a point for a specific viewport
            Matrix BuildCamera(Vector2 focus, Viewport vp)
            {
                int tileSz = 32;
                int mapWidth = _backgroundTiles.GetLength(1) * tileSz;
                int mapHeight = _backgroundTiles.GetLength(0) * tileSz;

                Vector2 cam = new(focus.X - vp.Width / 2f, focus.Y - vp.Height / 2f);
                cam.X = MathHelper.Clamp(cam.X, 0, System.Math.Max(0, mapWidth - vp.Width));
                cam.Y = MathHelper.Clamp(cam.Y, 0, System.Math.Max(0, mapHeight - vp.Height));
                return Matrix.CreateTranslation(-cam.X, -cam.Y, 0);
            }

            // Split or single camera
            if (_isMultiplayer)
            {
                var vpFull = originalViewport;
                float distX = System.Math.Abs(_player.Center.X - _player2.Center.X);
                float distY = System.Math.Abs(_player.Center.Y - _player2.Center.Y);

                // Decide split axis by normalized distance
                float normX = distX / System.Math.Max(1f, vpFull.Width);
                float normY = distY / System.Math.Max(1f, vpFull.Height);
                float threshold = 0.6f;

                if (normX >= normY && normX > threshold)
                {
                    // Vertical split (left/right)
                    var leftWidth = vpFull.Width / 2;
                    var rightWidth = vpFull.Width - leftWidth;

                    var vpLeft = new Viewport(vpFull.X, vpFull.Y, leftWidth, vpFull.Height);
                    var vpRight = new Viewport(vpFull.X + leftWidth, vpFull.Y, rightWidth, vpFull.Height);

                    // Left view (P1)
                    _graphicsDevice.Viewport = vpLeft;
                    spriteBatch.Begin(transformMatrix: BuildCamera(_player.Center, vpLeft));
                    DrawWorld(spriteBatch);
                    spriteBatch.End();

                    // Right view (P2)
                    _graphicsDevice.Viewport = vpRight;
                    spriteBatch.Begin(transformMatrix: BuildCamera(_player2.Center, vpRight));
                    DrawWorld(spriteBatch);
                    spriteBatch.End();

                    // Vertical separator
                    _graphicsDevice.Viewport = vpFull;
                    spriteBatch.Begin();
                    spriteBatch.Draw(_pixel, new Rectangle(vpLeft.Width - 1, 0, 2, vpFull.Height), Color.Black * 0.75f);
                    spriteBatch.End();
                }
                else if (normY > threshold)
                {
                    // Horizontal split (top/bottom)
                    var topHeight = vpFull.Height / 2;
                    var bottomHeight = vpFull.Height - topHeight;

                    var vpTop = new Viewport(vpFull.X, vpFull.Y, vpFull.Width, topHeight);
                    var vpBottom = new Viewport(vpFull.X, vpFull.Y + topHeight, vpFull.Width, bottomHeight);

                    // Top view (P1)
                    _graphicsDevice.Viewport = vpTop;
                    spriteBatch.Begin(transformMatrix: BuildCamera(_player.Center, vpTop));
                    DrawWorld(spriteBatch);
                    spriteBatch.End();

                    // Bottom view (P2)
                    _graphicsDevice.Viewport = vpBottom;
                    spriteBatch.Begin(transformMatrix: BuildCamera(_player2.Center, vpBottom));
                    DrawWorld(spriteBatch);
                    spriteBatch.End();

                    // Horizontal separator
                    _graphicsDevice.Viewport = vpFull;
                    spriteBatch.Begin();
                    spriteBatch.Draw(_pixel, new Rectangle(0, vpTop.Height - 1, vpFull.Width, 2), Color.Black * 0.75f);
                    spriteBatch.End();
                }
                else
                {
                    // Single shared camera centered between players
                    Vector2 mid = (_player.Center + _player2.Center) * 0.5f;
                    _graphicsDevice.Viewport = vpFull;
                    spriteBatch.Begin(transformMatrix: BuildCamera(mid, vpFull));
                    DrawWorld(spriteBatch);
                    spriteBatch.End();
                }

                // Restore viewport for UI
                _graphicsDevice.Viewport = originalViewport;
            }
            else
            {
                // Single player draw
                spriteBatch.Begin(transformMatrix: BuildCamera(_player.Center, originalViewport));
                DrawWorld(spriteBatch);
                spriteBatch.End();
            }

            // UI (shared)
            spriteBatch.Begin();
            _coinCounter.Draw(spriteBatch, _graphicsDevice);
            _bossHealthBar?.Draw(spriteBatch);
            _enemyCounter.Draw(spriteBatch);
            if (_isPaused) _pauseMenu.Draw(spriteBatch, _graphicsDevice);
            if (_dialogBox is { IsVisible: true }) _dialogBox.Draw(spriteBatch, _graphicsDevice);
            spriteBatch.End();

            // Health bars stacked top-right: Player 1 then Player 2
            var full = _graphicsDevice.Viewport;
            int margin = 16;
            int panelWidth = 260;   // UI panel width for a bar
            int blockHeight = 60;   // enough for "Player X" + bar
            int p2ExtraOffset = 48; // extra spacing between P1 and P2 (increase to move P2 lower)

            var prevVp = _graphicsDevice.Viewport;

            // Player 1 (top-right)
            var vpP1 = new Viewport(
                full.X + full.Width - panelWidth - margin,
                full.Y + margin,
                panelWidth,
                blockHeight
            );
            _graphicsDevice.Viewport = vpP1;
            spriteBatch.Begin();
            spriteBatch.DrawString(_uiFont, "Player 1", new Vector2(0, 0), Color.White);
            _healthBar.Draw(spriteBatch, _graphicsDevice);
            spriteBatch.End();

            if (_isMultiplayer && _healthBar2 != null)
            {
                // Player 2 (under Player 1 with extra spacing, clamped to screen)
                int p2Y = vpP1.Y + blockHeight + p2ExtraOffset;
                p2Y = Math.Min(full.Y + full.Height - blockHeight - margin, p2Y);

                var vpP2 = new Viewport(
                    full.X + full.Width - panelWidth - margin,
                    p2Y,
                    panelWidth,
                    blockHeight
                );
                _graphicsDevice.Viewport = vpP2;
                spriteBatch.Begin();
                spriteBatch.DrawString(_uiFont, "Player 2", new Vector2(0, 0), Color.White);
                _healthBar2.Draw(spriteBatch, _graphicsDevice);
                spriteBatch.End();
            }

            _graphicsDevice.Viewport = prevVp;
        }
    }
}