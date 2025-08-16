using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace ForestQuest.UI
{
    public class DialogBox
    {
        private Texture2D _background;
        private Texture2D _linaPortrait;
        private SpriteFont _font;
        private string[] _lines;
        private int _currentLine = 0;
        private bool _isVisible = true;
        private KeyboardState _prevState;

        public DialogBox(ContentManager content, string text)
        {
            _background = content.Load<Texture2D>("UI/Dialogbox");
            _linaPortrait = content.Load<Texture2D>("Player/LinaPortrait");
            _font = content.Load<SpriteFont>("Fonts/Font");
            _lines = text.Split('\n');
            _prevState = Keyboard.GetState();
        }

        public void Update()
        {
            KeyboardState state = Keyboard.GetState();
            if (_isVisible && _prevState.IsKeyUp(Keys.Enter) && state.IsKeyDown(Keys.Enter))
            {
                _currentLine++;
                if (_currentLine >= _lines.Length)
                {
                    _isVisible = false;
                }
            }
            _prevState = state;
        }

        public void Draw(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice)
        {
            if (!_isVisible) return;

            float screenWidth = graphicsDevice.Viewport.Width;
            float screenHeight = graphicsDevice.Viewport.Height;
            float popupWidth = 750f;
            float popupHeight = 400f;
            float popupX = (screenWidth - popupWidth) / 2;
            float popupY = screenHeight - popupHeight - 40;

            // Donkere overlay zoals PauseMenu, maar met lagere alpha zodat de game goed zichtbaar blijft
            Texture2D overlay = new Texture2D(graphicsDevice, 1, 1);
            overlay.SetData(new[] { Color.White });
            spriteBatch.Draw(overlay, new Rectangle(0, 0, (int)screenWidth, (int)screenHeight), Color.Black * 0.35f);

            // Dialogbox background
            spriteBatch.Draw(_background, new Rectangle((int)popupX, (int)popupY, (int)popupWidth, (int)popupHeight), Color.White * 0.85f);

            // Portrait settings
            int portraitSize = 110;
            int portraitMargin = 30;
            int portraitX = (int)popupX + portraitMargin;
            int portraitY = (int)popupY + ((int)popupHeight - portraitSize) / 2;
            spriteBatch.Draw(_linaPortrait, new Rectangle(portraitX, portraitY, portraitSize, portraitSize), Color.White);

            // Text settings
            int textMargin = 20;
            int textX = portraitX + portraitSize;
            int textY = (int)popupY + (int)popupHeight / 2 - textMargin;
            int textWidth = (int)popupWidth - (portraitSize + 2 * textMargin + portraitMargin);

            string wrappedText = WrapText(_font, _lines[_currentLine], textWidth);
            spriteBatch.DrawString(_font, wrappedText, new Vector2(textX, textY), Color.Black);
        }

        private string WrapText(SpriteFont font, string text, float maxLineWidth)
        {
            string[] words = text.Split(' ');
            string line = "";
            string result = "";
            foreach (var word in words)
            {
                string testLine = (line.Length == 0) ? word : line + " " + word;
                if (font.MeasureString(testLine).X > maxLineWidth)
                {
                    result += line + "\n";
                    line = word;
                }
                else
                {
                    line = testLine;
                }
            }
            result += line;
            return result;
        }

        public bool IsVisible => _isVisible;
    }
}