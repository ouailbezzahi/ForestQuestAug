using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace ForestQuest.UI
{
    public class EnemyCounter
    {
        private SpriteFont _font;
        private int _killed;
        private int _total;

        // Positie configuratie
        public Vector2 Position { get; set; } = new Vector2(10, 60); // standaard onder coins

        public EnemyCounter(ContentManager content, string fontAsset = "Fonts/Font")
        {
            _font = content.Load<SpriteFont>(fontAsset);
        }

        public void Set(int killed, int total)
        {
            _killed = killed;
            _total = total;
        }

        public void IncrementKilled()
        {
            _killed++;
            if (_killed > _total) _killed = _total;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (_font == null) return;
            spriteBatch.DrawString(_font, $"Enemies: {_killed}/{_total}", Position, Color.White);
        }
    }
}