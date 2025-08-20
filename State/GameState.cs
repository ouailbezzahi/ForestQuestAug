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

        // Player & audio
        private Player _player;
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

        // NIEUW: track enemies geraakt in huidige swing
        private readonly HashSet<Enemy> _hitEnemiesThisAttack = new();
        private bool _prevAttackActive;

        // State
        private bool _gameOverTriggered;
        private bool _victoryTriggered;

        private int _totalCoins;

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

            _player = new Player(new Vector2(15 * 32, 15 * 32), _level);
            _player.LoadContent(_content);

            _healthBar = new HealthBar(_content, 100);
            _healthBar.SetHealth(_player.Health);
            _player.OnHealthChanged += (cur, max) => _healthBar.SetHealth(cur);

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

            var keyboardState = Keyboard.GetState();

            if (!_isPaused && (keyboardState.IsKeyDown(Keys.Escape) || keyboardState.IsKeyDown(Keys.P)))
            {
                _isPaused = true;
                return;
            }
            else if (_isPaused && !_showingOptions && (keyboardState.IsKeyDown(Keys.Escape) || keyboardState.IsKeyDown(Keys.P)))
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

            var kb2 = Keyboard.GetState();
            _player.Update(kb2, gameTime, _graphicsDevice.Viewport, _backgroundTiles);

            foreach (var enemy in _enemies)
                enemy.Update(gameTime, _player.Position, _backgroundTiles);

            // Reset set bij einde van een attack
            if (!_player.IsAttackActive && _prevAttackActive)
            {
                _hitEnemiesThisAttack.Clear();
            }

            if (_player.IsAttackActive)
            {
                Rectangle attackHit = _player.GetAttackHitbox();
                foreach (var enemy in _enemies)
                {
                    if (!enemy.IsDead &&
                        attackHit.Intersects(enemy.BoundingBox) &&
                        !_hitEnemiesThisAttack.Contains(enemy))
                    {
                        if (enemy.Type == EnemyType.Wolf)
                            enemy.ApplyDamage(PLAYER_WOLF_ATTACK_DAMAGE);
                        else
                            enemy.Kill();

                        _hitEnemiesThisAttack.Add(enemy);

                        if (enemy.IsDead)
                        {
                            _enemiesKilled++;
                            _enemyCounter.Set(_enemiesKilled, _totalEnemies);
                        }
                    }
                }
            }

            _prevAttackActive = _player.IsAttackActive;

            Rectangle playerRect = new((int)_player.Position.X, (int)_player.Position.Y, _player.FrameWidth, _player.FrameHeight);
            foreach (var enemy in _enemies)
                if (enemy.TryDealDamage(playerRect, out int dmg))
                    _player.ApplyDamage(dmg);

            _coinManager.Update(gameTime);
            for (int i = _coinManager.Coins.Count - 1; i >= 0; i--)
            {
                var coin = _coinManager.Coins[i];
                Rectangle playerRect2 = new((int)_player.Position.X, (int)_player.Position.Y, _player.FrameWidth, _player.FrameHeight);
                if (coin.BoundingBox.Intersects(playerRect2) && kb2.IsKeyDown(Keys.E))
                {
                    _coinManager.Coins.RemoveAt(i);
                    _coinCounter.AddCoins(1);
                    _coinCount++;
                    float sfxVolume = (_optionsMenu != null) ? _optionsMenu.SFXValue / 100f : 1f;
                    _coinPickupSound?.Play(sfxVolume, 0f, 0f);
                }
            }

            if (!_gameOverTriggered && !_victoryTriggered && !_player.IsDead)
            {
                const float footstepInterval = 0.3f;
                _footstepTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
                bool isMoving = kb2.IsKeyDown(Keys.Z) || kb2.IsKeyDown(Keys.Q) ||
                                kb2.IsKeyDown(Keys.S) || kb2.IsKeyDown(Keys.D);

                if (isMoving && _footstepTimer >= footstepInterval)
                {
                    if (_playerMoveSoundInstance.State != SoundState.Playing)
                        _playerMoveSoundInstance.Play();
                    _footstepTimer = 0f;
                }
                else if (!isMoving && _playerMoveSoundInstance.State == SoundState.Playing)
                {
                    _playerMoveSoundInstance.Stop();
                }
            }
            else
            {
                StopFootsteps();
            }

            if (_isMultiplayer)
            {
                // future logic
            }

            if (!_victoryTriggered &&
                !_player.IsDead &&
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
                    _level,
                    coinsCollected: _coinCount,
                    totalCoins: _totalCoins,
                    enemiesKilled: _enemiesKilled,
                    totalEnemies: _totalEnemies));
                return;
            }

            if (!_gameOverTriggered && _player.IsDead && !_victoryTriggered)
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
            spriteBatch.Begin(transformMatrix: _player.CameraTransform);

            int tileSize = 32;
            float grassScale = 2f;

            for (int y = 0; y < _backgroundTiles.GetLength(0); y++)
                for (int x = 0; x < _backgroundTiles.GetLength(1); x++)
                    if (_backgroundTiles[y, x] == 2)
                        spriteBatch.Draw(_grassTile, new Vector2(x * tileSize, y * tileSize),
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
                            spriteBatch.Draw(tileTexture, new Rectangle((int)position.X, (int)position.Y, tileSize, tileSize), Color.White);
                        else
                            spriteBatch.Draw(tileTexture, position, Color.White);
                    }
                }
            }

            _coinManager.Draw(spriteBatch);
            _player.Draw(spriteBatch);
            foreach (var enemy in _enemies)
                enemy.Draw(spriteBatch, _player.Position);

            spriteBatch.End();

            spriteBatch.Begin();
            _coinCounter.Draw(spriteBatch, _graphicsDevice);
            _bossHealthBar?.Draw(spriteBatch);
            _healthBar.Draw(spriteBatch, _graphicsDevice);
            _enemyCounter.Draw(spriteBatch);
            if (_isPaused) _pauseMenu.Draw(spriteBatch, _graphicsDevice);
            if (_dialogBox is { IsVisible: true }) _dialogBox.Draw(spriteBatch, _graphicsDevice);
            spriteBatch.End();
        }
    }
}