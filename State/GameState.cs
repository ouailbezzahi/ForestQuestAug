using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;
using System.Collections.Generic;
using ForestQuest.Items.Coin;
using ForestQuest.UI;
using ForestQuest.Entities.Enemies;
using ForestQuest.Entities.Player;

namespace ForestQuest.State
{
    public class GameState : State
    {
        private Texture2D _houseTile;
        private Texture2D _treeTile;
        private Texture2D _grassTile;

        private Texture2D _borderLeftUp;
        private Texture2D _borderLeftDown;
        private Texture2D _borderRightUp;
        private Texture2D _borderRightDown;

        private Texture2D _borderVertical;
        private Texture2D _borderHorizontal;

        private Player _player;
        private Song _backgroundMusic;
        private SoundEffect _playerMoveSound;
        private SoundEffectInstance _playerMoveSoundInstance;
        private float _footstepTimer = 0f;
        private bool _isMultiplayer;
        private bool _isPaused = false;
        private PauseMenu _pauseMenu;
        private OptionsMenu _optionsMenu;
        private bool _showingOptions = false;
        private int _coinCount = 0;
        private Texture2D _coinIcon;
        private CoinManager _coinManager;
        private CoinCounter _coinCounter;
        private HealthBar _healthBar;

        private DialogBox _dialogBox;

        private List<Enemy> _enemies;

        // Enemy statistieken + UI
        private EnemyCounter _enemyCounter;
        private int _totalEnemies;
        private int _enemiesKilled;
        public int TotalEnemies => _totalEnemies;
        public int EnemiesKilled => _enemiesKilled;

        // Game over transition guard
        private bool _gameOverTriggered;

        private int[,] _backgroundTiles = new int[,]
        {
            { 3, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 5 },
            { 7, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 7 },
            { 7, 2, 2, 2, 1, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 1, 2, 7 },
            { 7, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 7 },
            { 7, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 0, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 7 },
            { 7, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 7 },
            { 7, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 7 },
            { 7, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 7 },
            { 7, 2, 2, 2, 2, 2, 2, 1, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 7 },
            { 7, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 7 },
            { 7, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 7 },
            { 7, 2, 2, 2, 2, 2, 2, 1, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 7 },
            { 7, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 7 },
            { 7, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 7 },
            { 7, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 7 },
            { 7, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 7 },
            { 7, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 1, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 7 },
            { 7, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 7 },
            { 7, 2, 2, 2, 1, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 7 },
            { 7, 2, 2, 1, 1, 2, 2, 2, 2, 2, 1, 1, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 7 },
            { 7, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 1, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 7 },
            { 7, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 7 },
            { 7, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 7 },
            { 7, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 7 },
            { 7, 2, 1, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 7 },
            { 7, 2, 1, 1, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 7 },
            { 7, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 0, 2, 2, 2, 2, 7 },
            { 7, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 7 },
            { 7, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 7 },
            { 4, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 6 },
        };

        public GameState(Game1 game, ContentManager content, GraphicsDevice graphicsDevice, bool isMultiplayer)
            : base(game, content, graphicsDevice)
        {
            _isMultiplayer = isMultiplayer;
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

            MediaPlayer.IsRepeating = true;
            MediaPlayer.Play(_backgroundMusic);

            _player = new Player(new Vector2(15 * 32, 15 * 32));
            _player.LoadContent(_content);

            _healthBar = new HealthBar(_content, 100);
            _healthBar.SetHealth(_player.Health);
            _player.OnHealthChanged += (cur, max) => _healthBar.SetHealth(cur);

            _pauseMenu = new PauseMenu(_content, _graphicsDevice);
            _optionsMenu = new OptionsMenu(_content, _graphicsDevice);

            int tileSize = 32;
            int mapWidth = _backgroundTiles.GetLength(1) * tileSize;
            int mapHeight = _backgroundTiles.GetLength(0) * tileSize;

            _coinManager = new CoinManager(_content, mapWidth / tileSize, mapHeight / tileSize);
            _coinCounter = new CoinCounter(_content);
            _healthBar = new HealthBar(_content, 100);

            string introText = "Welkom in Forest Quest!\nJe bent Lina, een jonge avonturier die haar dorp wil redden van een mysterieuze duisternis in het Verloren Bos. Versla vijandige dieren, verzamel items en vind de bron van de duisternis: de Shadow Wolf.\nGebruik WASD om te bewegen, Spatie om aan te vallen, en E om items op te rapen. Verzamel genoeg munten en vind de sleutel om naar het volgende level te gaan!";
            _dialogBox = new DialogBox(_content, introText);

            _enemies = new List<Enemy>();
            var spawnPositions = new[]
            {
                new Vector2(100,100),
                new Vector2(300,100),
                new Vector2(500,100),
                new Vector2(100,300),
                new Vector2(300,300)
            };
            for (int i = 0; i < spawnPositions.Length; i++)
            {
                var e = new Enemy(spawnPositions[i]);
                e.LoadContent(_content);
                _enemies.Add(e);
            }

            _totalEnemies = _enemies.Count;
            _enemiesKilled = 0;

            _enemyCounter = new EnemyCounter(_content);
            _enemyCounter.Set(_enemiesKilled, _totalEnemies);
        }

        private void StopFootsteps()
        {
            if (_playerMoveSoundInstance != null && _playerMoveSoundInstance.State == SoundState.Playing)
                _playerMoveSoundInstance.Stop();
        }

        public override void Update(GameTime gameTime)
        {
            if (_dialogBox != null && _dialogBox.IsVisible)
            {
                _dialogBox.Update();
                return;
            }

            KeyboardState keyboardState = Keyboard.GetState();

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

            var keyboardState2 = Keyboard.GetState();
            _player.Update(keyboardState2, gameTime, _graphicsDevice.Viewport, _backgroundTiles);

            foreach (var enemy in _enemies)
                enemy.Update(gameTime, _player.Position, _backgroundTiles);

            // Player attack -> instant kill
            if (_player.IsAttackActive)
            {
                Rectangle attackHit = _player.GetAttackHitbox();
                foreach (var enemy in _enemies)
                {
                    if (!enemy.IsDead && attackHit.Intersects(enemy.BoundingBox))
                    {
                        enemy.Kill();
                        _enemiesKilled++;
                        _enemyCounter.Set(_enemiesKilled, _totalEnemies);
                    }
                }
            }

            // Enemy damage to player
            Rectangle playerRect = new Rectangle((int)_player.Position.X, (int)_player.Position.Y, _player.FrameWidth, _player.FrameHeight);
            foreach (var enemy in _enemies)
            {
                if (enemy.TryDealDamage(playerRect, out int dmg))
                    _player.ApplyDamage(dmg);
            }

            _coinManager.Update(gameTime);
            for (int i = _coinManager.Coins.Count - 1; i >= 0; i--)
            {
                var coin = _coinManager.Coins[i];
                Rectangle playerRect2 = new((int)_player.Position.X, (int)_player.Position.Y, _player.FrameWidth, _player.FrameHeight);
                if (coin.BoundingBox.Intersects(playerRect2) && keyboardState2.IsKeyDown(Keys.E))
                {
                    _coinManager.Coins.RemoveAt(i);
                    _coinCounter.AddCoins(1);
                    _coinCount++; // bijhouden voor game over scherm
                }
            }

            // Footsteps enkel als niet dood en geen game over in gang
            if (!_gameOverTriggered && !_player.IsDead)
            {
                const float footstepInterval = 0.3f;
                _footstepTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
                bool isMoving = keyboardState2.IsKeyDown(Keys.Z) || keyboardState2.IsKeyDown(Keys.Q) ||
                                keyboardState2.IsKeyDown(Keys.S) || keyboardState2.IsKeyDown(Keys.D);

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
                // Forceer stoppen zodra dood / game over
                StopFootsteps();
            }

            if (_isMultiplayer)
            {
                // future logic
            }

            // Game over transition
            if (!_gameOverTriggered && _player.IsDead)
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
                return;
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
                        spriteBatch.Draw(_grassTile, new Vector2(x * tileSize, y * tileSize), null, Color.White, 0f, Vector2.Zero, new Vector2(grassScale), SpriteEffects.None, 0f);

            for (int y = 0; y < _backgroundTiles.GetLength(0); y++)
            {
                for (int x = 0; x < _backgroundTiles.GetLength(1); x++)
                {
                    Texture2D tileTexture = null;
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
                        if (_backgroundTiles[y, x] >= 3 && _backgroundTiles[y, x] <= 8)
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
            _healthBar.Draw(spriteBatch, _graphicsDevice);
            _coinCounter.Draw(spriteBatch, _graphicsDevice);
            _enemyCounter?.Draw(spriteBatch);
            if (_isPaused) _pauseMenu.Draw(spriteBatch, _graphicsDevice);
            if (_dialogBox != null && _dialogBox.IsVisible) _dialogBox.Draw(spriteBatch, _graphicsDevice);
            spriteBatch.End();
        }
    }
}