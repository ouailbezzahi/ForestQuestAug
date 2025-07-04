using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace ForestQuest.State
{
    public class PauseMenu
    {
        private SpriteFont _font;
        private string[] _options = { "Resume", "Back to menu", "Options", "Quit" };
        private Vector2[] _optionPositions;
        private Rectangle[] _optionBounds;
        private int _selectedIndex = -1;
        private bool _mouseWasPressed = false;

        public PauseMenu(ContentManager content, GraphicsDevice graphicsDevice)
        {
            _font = content.Load<SpriteFont>("Fonts/Font");
            _optionPositions = new Vector2[_options.Length];
            _optionBounds = new Rectangle[_options.Length];
            CalculatePositions(graphicsDevice);
        }

        private void CalculatePositions(GraphicsDevice graphicsDevice)
        {
            float screenWidth = graphicsDevice.Viewport.Width;
            float screenHeight = graphicsDevice.Viewport.Height;
            float spacing = 50f;
            float popupWidth = 350f;
            float popupHeight = 300f;
            float popupX = (screenWidth - popupWidth) / 2;
            float popupY = (screenHeight - popupHeight) / 2;

            for (int i = 0; i < _options.Length; i++)
            {
                Vector2 textSize = _font.MeasureString(_options[i]);
                _optionPositions[i] = new Vector2(
                    popupX + (popupWidth - textSize.X) / 2,
                    popupY + 60 + i * spacing
                );
                _optionBounds[i] = new Rectangle(
                    (int)_optionPositions[i].X,
                    (int)_optionPositions[i].Y,
                    (int)textSize.X,
                    (int)textSize.Y
                );
            }
        }

        public int Update()
        {
            MouseState mouse = Mouse.GetState();
            Point mousePos = mouse.Position;
            _selectedIndex = -1;
            for (int i = 0; i < _optionBounds.Length; i++)
            {
                if (_optionBounds[i].Contains(mousePos))
                {
                    _selectedIndex = i;
                }
            }

            // Muis klik
            if (mouse.LeftButton == ButtonState.Pressed && !_mouseWasPressed && _selectedIndex != -1)
            {
                _mouseWasPressed = true;
                return _selectedIndex;
            }
            if (mouse.LeftButton == ButtonState.Released)
            {
                _mouseWasPressed = false;
            }

            // Keyboard (optioneel: pijltjes en enter)
            KeyboardState keyboard = Keyboard.GetState();
            if (keyboard.IsKeyDown(Keys.Down))
            {
                _selectedIndex = (_selectedIndex + 1) % _options.Length;
            }
            if (keyboard.IsKeyDown(Keys.Up))
            {
                _selectedIndex = (_selectedIndex - 1 + _options.Length) % _options.Length;
            }
            if (keyboard.IsKeyDown(Keys.Enter) && _selectedIndex != -1)
            {
                return _selectedIndex;
            }

            return -1;
        }

        public void Draw(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice)
        {
            float screenWidth = graphicsDevice.Viewport.Width;
            float screenHeight = graphicsDevice.Viewport.Height;
            float popupWidth = 350f;
            float popupHeight = 300f;
            float popupX = (screenWidth - popupWidth) / 2;
            float popupY = (screenHeight - popupHeight) / 2;

            // Donkere overlay
            Texture2D overlay = new Texture2D(graphicsDevice, 1, 1);
            overlay.SetData(new[] { new Color(0, 0, 0, 0.7f) });
            spriteBatch.Draw(overlay, new Rectangle(0, 0, (int)screenWidth, (int)screenHeight), Color.Black * 0.7f);

            // Popup venster
            Texture2D popup = new Texture2D(graphicsDevice, 1, 1);
            popup.SetData(new[] { Color.Gray });
            spriteBatch.Draw(popup, new Rectangle((int)popupX, (int)popupY, (int)popupWidth, (int)popupHeight), Color.Gray);

            // Titel
            string title = "Paused";
            Vector2 titleSize = _font.MeasureString(title);
            spriteBatch.DrawString(_font, title, new Vector2(popupX + (popupWidth - titleSize.X) / 2, popupY + 20), Color.White);

            // Opties
            for (int i = 0; i < _options.Length; i++)
            {
                Color color = (_selectedIndex == i) ? Color.Yellow : Color.White;
                spriteBatch.DrawString(_font, _options[i], _optionPositions[i], color);
            }
        }
    }
} 