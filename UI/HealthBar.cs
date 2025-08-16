using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace ForestQuest.UI
{
    public class HealthBar
    {
        private Texture2D _healthBarSprite;
        private SpriteFont _font;
        private int _maxHealth;
        private int _currentHealth;
        private Vector2 _position;
        private int _rows = 3;
        private int _cols = 2;
        private int _spriteWidth;
        private int _spriteHeight;

        public HealthBar(ContentManager content, int maxHealth = 100)
        {
            _maxHealth = maxHealth;
            _currentHealth = maxHealth;
            LoadContent(content);
        }

        private void LoadContent(ContentManager content)
        {
            _font = content.Load<SpriteFont>("Fonts/Font");
            _healthBarSprite = content.Load<Texture2D>("Player/healthbar");
            _spriteWidth = _healthBarSprite.Width / _cols;
            _spriteHeight = _healthBarSprite.Height / _rows;
        }

        public void UpdatePosition(GraphicsDevice graphicsDevice)
        {
            _position = new Vector2(graphicsDevice.Viewport.Width - _spriteWidth - 20, 20);
        }

        public void SetHealth(int currentHealth)
        {
            _currentHealth = MathHelper.Clamp(currentHealth, 0, _maxHealth);
        }

        public void Draw(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice)
        {
            UpdatePosition(graphicsDevice);

            // Bepaal welke sprite (rij, kolom) we moeten tekenen
            float healthPercent = (float)_currentHealth / _maxHealth;
            int row = 0, col = 0;
            if (healthPercent >= 0.99f) { row = 0; col = 0; } // 100%
            else if (healthPercent >= 0.66f) { row = 1; col = 0; } // 80%
            else if (healthPercent >= 0.49f) { row = 2; col = 0; } // 60%
            else if (healthPercent >= 0.32f) { row = 0; col = 1; } // 40%
            else if (healthPercent >= 0.15f) { row = 1; col = 1; } // 20%
            else { row = 2; col = 1; } // 0%

            Rectangle sourceRect = new Rectangle(col * _spriteWidth, row * _spriteHeight, _spriteWidth, _spriteHeight);
            Rectangle destRect = new Rectangle((int)_position.X, (int)_position.Y, _spriteWidth, _spriteHeight);
            spriteBatch.Draw(_healthBarSprite, destRect, sourceRect, Color.White);

            // Teken tekst boven de bar
            string healthText = $"{_currentHealth}/{_maxHealth}";
            Vector2 textSize = _font.MeasureString(healthText);
            Vector2 textPosition = new Vector2(_position.X + (_spriteWidth - textSize.X) / 2, _position.Y - textSize.Y - 2);
            spriteBatch.DrawString(_font, healthText, textPosition, Color.White);
        }

        public int SpriteWidth => _spriteWidth;
        public int SpriteHeight => _spriteHeight;
        public int Y => (int)_position.Y;
    }
} 