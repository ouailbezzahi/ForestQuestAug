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
using ForestQuest.World.Camera; // + toegevoegd
using ForestQuest.Entities.Enemies.Factory; // + factory
using ForestQuest.Items.Coin.Factory; // + coin factory
using ForestQuest.World.Levels; // + level factory
using ForestQuest.Input; // + command input
using ForestQuest.Input.Commands; // + commands

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

        // Camera strategy (nieuw)
        private readonly ICameraStrategy _cameraStrategy = new DynamicSplitCameraStrategy();

        // Enemy factory (nieuw)
        private readonly IEnemyFactory _enemyFactory;

        // Coin factory (nieuw)
        private readonly ICoinFactory _coinFactory;

        // Level factory (nieuw)
        private readonly ILevelFactory _levelFactory;
        private LevelData _levelData;
        private int[,] _backgroundTiles;

        // Command-pattern (nieuw)
        private readonly InputMapper _inputMapper = new();
        private GameInputContext _inputContext;
        private bool _p1InteractPressed; // per-frame flag i.p.v. direct kb lezen
        private bool _p2InteractPressed; // per-frame flag i.p.v. direct kb lezen

        public GameState(Game1 game, ContentManager content, GraphicsDevice graphicsDevice, bool isMultiplayer, int level = 3)
            : base(game, content, graphicsDevice)
        {
            _isMultiplayer = isMultiplayer;
            _level = level;

            // init factories
            _enemyFactory = new EnemyFactory();
            _coinFactory = new CoinFactory();
            _levelFactory = new LevelFactory();
        }

        public override void LoadContent()
        {
            // Level data ophalen (tegels, startposities, coin-spawn)
            _levelData = _levelFactory.Create(_level);
            _backgroundTiles = _levelData.Tiles;

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

            // Startposities uit leveldata (identiek aan voorheen)
            _player = new Player(_levelData.Player1Start, PlayerControls.Default1, _level);
            _player.LoadContent(_content);

            if (_isMultiplayer)
            {
                _player2 = new Player(_levelData.Player2Start, PlayerControls.Default2, _level);
                _player2.LoadContent(_content);
            }

            // Command context koppelen (kan pas na players)
            _inputContext = new GameInputContext(
                _player,
                _player2,
                _isMultiplayer,
                setP1Interact: pressed => _p1InteractPressed = pressed,
                setP2Interact: pressed => _p2InteractPressed = pressed
            );

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

            // Zelfde coins-spawn logica, maar via leveldata
            int coinSpawn = _levelData.CoinSpawnCount;
            _coinManager = _coinFactory.Create(_content, mapWidth / tileSize, mapHeight / tileSize, coinSpawn);
            _coinCounter = new CoinCounter(_content);
            _totalCoins = _coinManager.Coins.Count;

            _dialogBox = _level == 1
                ? new DialogBox(_content, "Welkom in Forest Quest! ... (Level 1)")
                : null;

            // Enemies via factory (zelfde spawns/gedrag als voorheen)
            _enemies = _enemyFactory.CreateForLevel(_content, _level);

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

            // Command mapping (per frame) — NIET invasief
            _p1InteractPressed = false;
            _p2InteractPressed = false;
            var commands = _inputMapper.Map(kb, _inputContext);
            foreach (var cmd in commands)
                cmd.Execute();

            // Update players (ongewijzigd, leest nog steeds kb intern)
            _player.Update(kb, gameTime, _graphicsDevice.Viewport, _backgroundTiles);
            if (_isMultiplayer)
                _player2.Update(kb, gameTime, _graphicsDevice.Viewport, _backgroundTiles);

            // Enemies chase the nearest player (use CENTER for accurate ranges)
            foreach (var enemy in _enemies)
            {
                Vector2 target = _player.Center;
                if (_isMultiplayer)
                {
                    float d1 = Vector2.Distance(enemy.Position, _player.Center);
                    float d2 = Vector2.Distance(enemy.Position, _player2.Center);
                    target = d1 <= d2 ? _player.Center : _player2.Center;
                }
                enemy.Update(gameTime, target, _backgroundTiles);
            }

            // Player <-> enemy blocking
            for (int iter = 0; iter < 2; iter++)
            {
                foreach (var enemy in _enemies)
                {
                    if (enemy.IsDead) continue;
                    Rectangle eRect = enemy.BoundingBox;

                    Rectangle p1Rect = new((int)_player.Position.X, (int)_player.Position.Y, _player.FrameWidth, _player.FrameHeight);
                    if (p1Rect.Intersects(eRect))
                        _player.ResolveCollisionWith(eRect, _backgroundTiles);

                    if (_isMultiplayer)
                    {
                        Rectangle p2Rect = new((int)_player2.Position.X, (int)_player2.Position.Y, _player2.FrameWidth, _player2.FrameHeight);
                        if (p2Rect.Intersects(eRect))
                            _player2.ResolveCollisionWith(eRect, _backgroundTiles);
                    }
                }
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

            // Enemy -> players damage
            Rectangle p1Rect2 = new((int)_player.Position.X, (int)_player.Position.Y, _player.FrameWidth, _player.FrameHeight);
            foreach (var enemy in _enemies)
                if (enemy.TryDealDamage(p1Rect2, out int dmg1))
                    _player.ApplyDamage(dmg1);

            if (_isMultiplayer)
            {
                Rectangle p2Rect2 = new((int)_player2.Position.X, (int)_player2.Position.Y, _player2.FrameWidth, _player2.FrameHeight);
                foreach (var enemy in _enemies)
                    if (enemy.TryDealDamage(p2Rect2, out int dmg2))
                        _player2.ApplyDamage(dmg2);
            }

            // Coins (gebruik de command-flag, met fallback naar kb)
            _coinManager.Update(gameTime);
            for (int i = _coinManager.Coins.Count - 1; i >= 0; i--)
            {
                var coin = _coinManager.Coins[i];

                bool picked = false;
                Rectangle rect1 = new((int)_player.Position.X, (int)_player.Position.Y, _player.FrameWidth, _player.FrameHeight);
                if (coin.BoundingBox.Intersects(rect1) &&
                    (_p1InteractPressed || Keyboard.GetState().IsKeyDown(_player.Controls.Interact)))
                {
                    picked = true;
                }

                if (_isMultiplayer && !picked)
                {
                    Rectangle rect2 = new((int)_player2.Position.X, (int)_player2.Position.Y, _player2.FrameWidth, _player2.FrameHeight);
                    if (coin.BoundingBox.Intersects(rect2) &&
                        (_p2InteractPressed || Keyboard.GetState().IsKeyDown(_player2.Controls.Interact)))
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

            // Footsteps
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

            // Game over
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
                cam.X = MathHelper.Clamp(cam.X, 0, Math.Max(0, mapWidth - vp.Width));
                cam.Y = MathHelper.Clamp(cam.Y, 0, Math.Max(0, mapHeight - vp.Height));
                return Matrix.CreateTranslation(-cam.X, -cam.Y, 0);
            }

            // Strategy bepaalt views (single/shared/split)
            Point mapPx = new(_backgroundTiles.GetLength(1) * tileSize, _backgroundTiles.GetLength(0) * tileSize);
            var views = _cameraStrategy.BuildViews(
                _graphicsDevice,
                originalViewport,
                _player.Center,
                _isMultiplayer ? _player2.Center : (Vector2?)null,
                mapPx
            );

            foreach (var v in views)
            {
                _graphicsDevice.Viewport = v.Viewport;
                spriteBatch.Begin(transformMatrix: BuildCamera(v.Focus, v.Viewport));
                DrawWorld(spriteBatch);
                spriteBatch.End();
            }

            // Herstel viewport en teken eventuele separator
            _graphicsDevice.Viewport = originalViewport;
            spriteBatch.Begin();
            _cameraStrategy.DrawSeparators(spriteBatch, originalViewport, views, _pixel);
            spriteBatch.End();

            // UI (shared)
            spriteBatch.Begin();
            _coinCounter.Draw(spriteBatch, _graphicsDevice);
            _bossHealthBar?.Draw(spriteBatch);
            _enemyCounter.Draw(spriteBatch);
            if (_isPaused) _pauseMenu.Draw(spriteBatch, _graphicsDevice);
            if (_dialogBox is { IsVisible: true }) _dialogBox.Draw(spriteBatch, _graphicsDevice);
            spriteBatch.End();

            // Health bars stacked top-right
            var full = _graphicsDevice.Viewport;
            int margin = 16;
            int panelWidth = 260;
            int blockHeight = 60;
            int p2ExtraOffset = 48;

            var prevVp = _graphicsDevice.Viewport;

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