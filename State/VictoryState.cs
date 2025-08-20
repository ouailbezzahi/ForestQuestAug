using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;

namespace ForestQuest.State
{
    public class VictoryState : State
    {
        private readonly int _coinsCollected;
        private readonly int _totalCoins;
        private readonly int _enemiesKilled;
        private readonly int _totalEnemies;

        private SpriteFont _font;
        private readonly string[] _options = { "Next Level", "Play Again", "Quit" };
        private int _selectedIndex;
        private KeyboardState _prevKb;

        private SoundEffect _sfxVictory;
        private bool _soundPlayed;

        public VictoryState(Game1 game,
                            ContentManager content,
                            GraphicsDevice graphicsDevice,
                            int coinsCollected,
                            int totalCoins,
                            int enemiesKilled,
                            int totalEnemies)
            : base(game, content, graphicsDevice)
        {
            _coinsCollected = coinsCollected;
            _totalCoins = totalCoins;
            _enemiesKilled = enemiesKilled;
            _totalEnemies = totalEnemies;
        }

        public override void LoadContent()
        {
            _font = _content.Load<SpriteFont>("Fonts/Font");

            try
            {
                _sfxVictory = _content.Load<SoundEffect>("Audio/victory");
            }
            catch
            {
                _sfxVictory = null;
            }

            PlaySoundOnce();
        }

        private void PlaySoundOnce()
        {
            if (_soundPlayed) return;
            _sfxVictory?.Play();
            _soundPlayed = true;
        }

        public override void Update(GameTime gameTime)
        {
            PlaySoundOnce();

            var kb = Keyboard.GetState();
            bool up = kb.IsKeyDown(Keys.Up) && _prevKb.IsKeyUp(Keys.Up);
            bool down = kb.IsKeyDown(Keys.Down) && _prevKb.IsKeyUp(Keys.Down);
            bool enter = kb.IsKeyDown(Keys.Enter) && _prevKb.IsKeyUp(Keys.Enter);

            if (up)
                _selectedIndex = (_selectedIndex - 1 + _options.Length) % _options.Length;
            if (down)
                _selectedIndex = (_selectedIndex + 1) % _options.Length;

            if (enter)
            {
                switch (_selectedIndex)
                {
                    case 0: // Next Level (placeholder: opnieuw GameState)
                        _game.ChangeState(new GameState(_game, _content, _graphicsDevice, isMultiplayer: false));
                        return;
                    case 1: // Play Again (zelfde level herstarten)
                        _game.ChangeState(new GameState(_game, _content, _graphicsDevice, isMultiplayer: false));
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

            string title = "VICTORY";
            string stats1 = $"Coins:   {_coinsCollected}/{_totalCoins}";
            string stats2 = $"Enemies: {_enemiesKilled}/{_totalEnemies}";

            var vp = _graphicsDevice.Viewport;
            Vector2 center = new(vp.Width / 2f, vp.Height / 2f);

            Vector2 titleSize = _font.MeasureString(title);
            Vector2 titlePos = center - new Vector2(titleSize.X / 2f, 180);
            spriteBatch.DrawString(_font, title, titlePos, Color.Gold);

            spriteBatch.DrawString(_font, stats1, center - new Vector2(_font.MeasureString(stats1).X / 2f, 110), Color.White);
            spriteBatch.DrawString(_font, stats2, center - new Vector2(_font.MeasureString(stats2).X / 2f, 80), Color.White);

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