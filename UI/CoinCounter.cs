using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace ForestQuest.UI
{
    public class CoinCounter
    {
        private Texture2D _coinTexture;
        private SpriteFont _font;
        private int _coins;
        private Vector2 _position;
        private int _width = 200;
        private int _height = 30;

        public CoinCounter(ContentManager content)
        {
            _coins = 0;
            LoadContent(content);
        }

        private void LoadContent(ContentManager content)
        {
            _font = content.Load<SpriteFont>("Fonts/Font");
            _coinTexture = content.Load<Texture2D>("Items/coin");
        }

        public void UpdatePosition(GraphicsDevice graphicsDevice, int healthBarX, int healthBarY, int healthBarWidth, int healthBarHeight)
        {
            int spacing = 10;
            _position = new Vector2(healthBarX, healthBarY + healthBarHeight + spacing);
            _width = healthBarWidth;
            _height = 28; // iets lager
        }

        public void AddCoins(int amount)
        {
            _coins += amount;
        }

        public void SetCoins(int coins)
        {
            _coins = Math.Max(0, coins);
        }

        public int GetCoins()
        {
            return _coins;
        }

        public void Draw(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice)
        {
            // _position, _width, _height zijn nu correct gezet
            // Teken subtiele transparante achtergrond
            Texture2D backgroundTexture = new Texture2D(graphicsDevice, 1, 1);
            backgroundTexture.SetData(new[] { Color.Black * 0.3f });
            Rectangle backgroundRect = new Rectangle((int)_position.X, (int)_position.Y, _width, _height);
            spriteBatch.Draw(backgroundTexture, backgroundRect, Color.White);

            // Teken coin icoon en tekst gecentreerd
            int coinSize = 20;
            Vector2 textSize = _font.MeasureString($"Coins: {_coins}");
            int totalWidth = coinSize + 8 + (int)textSize.X;
            int startX = (int)_position.X + (_width - totalWidth) / 2;
            int iconY = (int)_position.Y + (_height - coinSize) / 2;
            spriteBatch.Draw(_coinTexture, new Rectangle(startX, iconY, coinSize, coinSize), Color.White);
            Vector2 textPos = new Vector2(startX + coinSize + 8, _position.Y + (_height - textSize.Y) / 2);
            spriteBatch.DrawString(_font, $"Coins: {_coins}", textPos, Color.Yellow);
        }
    }
} 