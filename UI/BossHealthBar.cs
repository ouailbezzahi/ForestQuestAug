using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace ForestQuest.UI
{
    public class BossHealthBar
    {
        private readonly Texture2D _pixel;
        private int _current;
        private int _max;
        public Rectangle Area { get; set; } = new Rectangle(20, 60, 300, 18);

        public BossHealthBar(ContentManager content, GraphicsDevice gd, int max)
        {
            _max = max;
            _current = max;
            _pixel = new Texture2D(gd, 1, 1);
            _pixel.SetData(new[] { Color.White });
        }

        public void Set(int cur, int max)
        {
            _max = max;
            _current = MathHelper.Clamp(cur, 0, max);
        }

        public void SetValue(int cur) => _current = MathHelper.Clamp(cur, 0, _max);

        public void Draw(SpriteBatch sb)
        {
            if (_max <= 0) return;
            // Achtergrond
            sb.Draw(_pixel, Area, Color.Black * 0.6f);
            // Rand
            sb.Draw(_pixel, new Rectangle(Area.X - 1, Area.Y - 1, Area.Width + 2, 1), Color.DarkRed);
            sb.Draw(_pixel, new Rectangle(Area.X - 1, Area.Bottom, Area.Width + 2, 1), Color.DarkRed);
            sb.Draw(_pixel, new Rectangle(Area.X - 1, Area.Y - 1, 1, Area.Height + 2), Color.DarkRed);
            sb.Draw(_pixel, new Rectangle(Area.Right, Area.Y - 1, 1, Area.Height + 2), Color.DarkRed);

            float pct = _current / (float)_max;
            int innerW = (int)((Area.Width - 2) * pct);
            var inner = new Rectangle(Area.X + 1, Area.Y + 1, innerW, Area.Height - 2);
            var col = Color.Lerp(Color.Red, Color.Lime, pct);
            sb.Draw(_pixel, inner, col);
        }
    }
}