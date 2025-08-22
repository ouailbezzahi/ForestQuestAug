using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;
using ForestQuest.Audio; // NEW

namespace ForestQuest.State
{
    public class VictoryState : State
    {
        private readonly int _currentLevel;
        private readonly bool _isMultiplayer;
        private readonly int _coinsCollected;
        private readonly int _totalCoins;
        private readonly int _enemiesKilled;
        private readonly int _totalEnemies;

        private SpriteFont _font;

        private string[] _options;
        private int _selectedIndex;
        private KeyboardState _prevKb;

        // Victory SFX via abstraction (DIP)
        private readonly IOneShotSfxPlayer _sfx;

        public VictoryState(Game1 game,
                            ContentManager content,
                            GraphicsDevice graphicsDevice,
                            int currentLevel,
                            bool isMultiplayer,
                            int coinsCollected,
                            int totalCoins,
                            int enemiesKilled,
                            int totalEnemies,
                            IOneShotSfxPlayer? sfxPlayer = null)
            : base(game, content, graphicsDevice)
        {
            _currentLevel = currentLevel;
            _isMultiplayer = isMultiplayer;
            _coinsCollected = coinsCollected;
            _totalCoins = totalCoins;
            _enemiesKilled = enemiesKilled;
            _totalEnemies = totalEnemies;

            if (_currentLevel < 3)
                _options = new[] { "Next Level", "Main Menu", "Quit" };
            else
                _options = new[] { "Main Menu", "Quit" };

            _sfx = sfxPlayer ?? new OneShotSfxPlayer(content);
        }

        public override void LoadContent()
        {
            _font = _content.Load<SpriteFont>("Fonts/Font");
            // Try load victory sound with fallback, play once
            _sfx.TryPlayOnce("Audio/victory", "Audio/level_complete");
        }

        public override void Update(GameTime gameTime)
        {
            // Ensure sound doesn't replay on refocus
            _sfx.TryPlayOnce("Audio/victory", "Audio/level_complete");

            var kb = Keyboard.GetState();

            bool upPressed = kb.IsKeyDown(Keys.Up) && _prevKb.IsKeyUp(Keys.Up);
            bool downPressed = kb.IsKeyDown(Keys.Down) && _prevKb.IsKeyUp(Keys.Down);
            bool enterPressed = kb.IsKeyDown(Keys.Enter) && _prevKb.IsKeyUp(Keys.Enter);

            if (upPressed)
                _selectedIndex = (_selectedIndex - 1 + _options.Length) % _options.Length;

            if (downPressed)
                _selectedIndex = (_selectedIndex + 1) % _options.Length;

            if (enterPressed)
            {
                var choice = _options[_selectedIndex];
                if (choice == "Next Level")
                {
                    int nextLevel = _currentLevel + 1;
                    _game.ChangeState(new GameState(_game, _content, _graphicsDevice, isMultiplayer: _isMultiplayer, level: nextLevel));
                    return;
                }
                else if (choice == "Main Menu")
                {
                    _game.ChangeState(new MenuState(_game, _content, _graphicsDevice));
                    return;
                }
                else if (choice == "Quit")
                {
                    _game.Exit();
                    return;
                }
            }

            _prevKb = kb;
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            _graphicsDevice.Clear(Color.Black);
            spriteBatch.Begin();

            string title = $"Level {_currentLevel} Completed!";
            Vector2 ts = _font.MeasureString(title);
            float cx = _graphicsDevice.Viewport.Width / 2f;
            float cy = _graphicsDevice.Viewport.Height / 4f;

            spriteBatch.DrawString(_font, title, new Vector2(cx - ts.X / 2f, cy), Color.Gold);

            string stats = $"Coins: {_coinsCollected}/{_totalCoins}   Enemies: {_enemiesKilled}/{_totalEnemies}";
            Vector2 ss = _font.MeasureString(stats);
            spriteBatch.DrawString(_font, stats, new Vector2(cx - ss.X / 2f, cy + ts.Y + 10), Color.White);

            float startY = cy + ts.Y + ss.Y + 40;
            for (int i = 0; i < _options.Length; i++)
            {
                string opt = (i == _selectedIndex ? "> " : "  ") + _options[i];
                Vector2 os = _font.MeasureString(opt);
                spriteBatch.DrawString(_font, opt, new Vector2(cx - os.X / 2f, startY + i * 30f), i == _selectedIndex ? Color.Yellow : Color.White);
            }

            spriteBatch.End();
        }
    }
}