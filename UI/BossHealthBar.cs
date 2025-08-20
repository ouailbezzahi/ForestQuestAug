using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace ForestQuest.UI
{
    public class BossHealthBar
    {
        private readonly Texture2D _pixel;
        private readonly SpriteFont _font;
        private int _current;
        private int _max;

        public Rectangle Area { get; set; }
        public string Label { get; set; } = "Shadow Wolf HP";

        public BossHealthBar(ContentManager content, GraphicsDevice gd, int max)
        {
            _max = max;
            _current = max;

            _pixel = new Texture2D(gd, 1, 1);
            _pixel.SetData(new[] { Color.White });

            // Hergebruik bestaand font (pas pad aan als jouw project een andere naam gebruikt)
            _font = content.Load<SpriteFont>("Fonts/Font");
            Area = new Rectangle(180, 20, 260, 22);
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
            sb.Draw(_pixel, Area, Color.Black * 0.55f);

            // Rand
            DrawBorder(sb, Area, 1, Color.DarkRed);

            // Vulling
            float pct = _current / (float)_max;
            int innerW = (int)((Area.Width - 2) * pct);
            var inner = new Rectangle(Area.X + 1, Area.Y + 1, innerW, Area.Height - 2);

            // Kleur verloopt van rood (0%) -> oranje (50%) -> lime (100%)
            Color mid = Color.Orange;
            Color fill = pct < 0.5f
                ? Color.Lerp(Color.Red, mid, pct / 0.5f)
                : Color.Lerp(mid, Color.Lime, (pct - 0.5f) / 0.5f);

            sb.Draw(_pixel, inner, fill);

            // Tekst (HP numeriek bovenop vulling – optioneel)
            string hpText = $"{_current}/{_max}";
            Vector2 hpSize = _font.MeasureString(hpText);
            var hpPos = new Vector2(
                Area.X + (Area.Width - hpSize.X) / 2f,
                Area.Y + (Area.Height - hpSize.Y) / 2f - 1);
            sb.DrawString(_font, hpText, hpPos + Vector2.One, Color.Black * 0.7f);
            sb.DrawString(_font, hpText, hpPos, Color.White);

            // Label onder de bar
            Vector2 labelSize = _font.MeasureString(Label);
            var labelPos = new Vector2(
                Area.X + (Area.Width - labelSize.X) / 2f,
                Area.Bottom + 2);
            sb.DrawString(_font, Label, labelPos + Vector2.One, Color.Black * 0.6f);
            sb.DrawString(_font, Label, labelPos, Color.Gold);
        }

        private void DrawBorder(SpriteBatch sb, Rectangle r, int thickness, Color color)
        {
            // top
            sb.Draw(_pixel, new Rectangle(r.X, r.Y, r.Width, thickness), color);
            // bottom
            sb.Draw(_pixel, new Rectangle(r.X, r.Bottom - thickness, r.Width, thickness), color);
            // left
            sb.Draw(_pixel, new Rectangle(r.X, r.Y, thickness, r.Height), color);
            // right
            sb.Draw(_pixel, new Rectangle(r.Right - thickness, r.Y, thickness, r.Height), color);
        }
    }
}