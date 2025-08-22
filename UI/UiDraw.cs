using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ForestQuest.UI
{
    public static class UiDraw
    {
        public static Rectangle CenteredPopup(GraphicsDevice gd, int width, int height)
        {
            var vp = gd.Viewport;
            int x = (vp.Width - width) / 2;
            int y = (vp.Height - height) / 2;
            return new Rectangle(x, y, width, height);
        }

        public static void Overlay(SpriteBatch sb, GraphicsDevice gd, Color color)
        {
            sb.Draw(UiResources.Pixel(gd), new Rectangle(0, 0, gd.Viewport.Width, gd.Viewport.Height), color);
        }

        public static void Panel(SpriteBatch sb, GraphicsDevice gd, Rectangle rect, Color color)
        {
            sb.Draw(UiResources.Pixel(gd), rect, color);
        }

        public static Color ButtonColor(bool hovered, bool isDown, bool selected, Color normal, Color hover, Color down, Color selectedColor)
        {
            if (isDown && hovered) return down;
            if (selected) return selectedColor;
            if (hovered) return hover;
            return normal;
        }

        public static void Button(SpriteBatch sb, GraphicsDevice gd, Rectangle rect, string text, SpriteFont font,
                                  bool hovered, bool isDown, bool selected,
                                  Color normal, Color hover, Color down, Color selectedColor, Color textColor)
        {
            var pixel = UiResources.Pixel(gd);
            var color = ButtonColor(hovered, isDown, selected, normal, hover, down, selectedColor);
            sb.Draw(pixel, rect, color);

            var size = font.MeasureString(text);
            var textPos = new Vector2(rect.X + (rect.Width - size.X) / 2f, rect.Y + (rect.Height - size.Y) / 2f);
            sb.DrawString(font, text, textPos, textColor);
        }
    }
}