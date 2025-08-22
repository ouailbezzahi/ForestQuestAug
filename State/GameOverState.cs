using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;
using ForestQuest.Audio; // NEW

namespace ForestQuest.State
{
    public class GameOverState : State
    {
        private readonly int _coinsCollected;
        private readonly int _enemiesKilled;
        private readonly int _totalEnemies;

        private SpriteFont _font;
        private string[] _options = { "Play Again", "Main Menu", "Quit" };
        private int _selectedIndex;
        private KeyboardState _prevKb;

        // SFX via abstraction (DIP)
        private readonly IOneShotSfxPlayer _sfx;

        public GameOverState(Game1 game,
                             ContentManager content,
                             GraphicsDevice graphicsDevice,
                             int coinsCollected,
                             int enemiesKilled,
                             int totalEnemies,
                             IOneShotSfxPlayer? sfxPlayer = null)
            : base(game, content, graphicsDevice)
        {
            _coinsCollected = coinsCollected;
            _enemiesKilled = enemiesKilled;
            _totalEnemies = totalEnemies;
            _sfx = sfxPlayer ?? new OneShotSfxPlayer(content);
        }

        public override void LoadContent()
        {
            _font = _content.Load<SpriteFont>("Fonts/Font");
            _sfx.TryPlayOnce("Audio/game_over");
        }

        public override void Update(GameTime gameTime)
        {
            // Ensure sound not replayed
            _sfx.TryPlayOnce("Audio/game_over");

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
                switch (_selectedIndex)
                {
                    case 0: // Play Again
                        _game.ChangeState(new GameState(_game, _content, _graphicsDevice, isMultiplayer: false));
                        return;
                    case 1: // Main Menu
                        _game.ChangeState(new MenuState(_game, _content, _graphicsDevice));
                        return;
                    case 2: // Quit
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

            string title = "GAME OVER";
            string stats1 = $"Coins:   {_coinsCollected}";
            string stats2 = $"Enemies: {_enemiesKilled}/{_totalEnemies}";

            Vector2 titleSize = _font.MeasureString(title);
            var vp = _graphicsDevice.Viewport;
            Vector2 center = new(vp.Width / 2f, vp.Height / 2f);

            Vector2 titlePos = center - new Vector2(titleSize.X / 2f, 180);
            spriteBatch.DrawString(_font, title, titlePos, Color.Red);

            spriteBatch.DrawString(_font, stats1, center - new Vector2(_font.MeasureString(stats1).X / 2f, 110), Color.White);
            spriteBatch.DrawString(_font, stats2, center - new Vector2(_font.MeasureString(stats2).X / 2f, 80), Color.White);

            // Options
            float startY = center.Y - 10;
            for (int i = 0; i < _options.Length; i++)
            {
                string opt = _options[i];
                Vector2 size = _font.MeasureString(opt);
                Vector2 pos = new(center.X - size.X / 2f, startY + i * 40);
                Color col = (i == _selectedIndex) ? Color.Yellow : Color.Gray;
                spriteBatch.DrawString(_font, opt, pos, col);
            }

            spriteBatch.End();
        }
    }
}