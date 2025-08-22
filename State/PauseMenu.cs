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

        // NEW: reusable 1x1 pixel
        private readonly Texture2D _pixel;

        public PauseMenu(ContentManager content, GraphicsDevice graphicsDevice)
        {
            _font = content.Load<SpriteFont>("Fonts/Font");
            _optionPositions = new Vector2[_options.Length];
            _optionBounds = new Rectangle[_options.Length];
            CalculatePositions(graphicsDevice);

            _pixel = new Texture2D(graphicsDevice, 1, 1);
            _pixel.SetData(new[] { Color.White });
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

            // Keyboard (zelfde gedrag behouden)
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
            float spacing = 50f;
            float popupWidth = 350f;
            float popupHeight = 300f;
            float popupX = (screenWidth - popupWidth) / 2;
            float popupY = (screenHeight - popupHeight) / 2;

            // Herbereken optieposities en bounds bij elke draw (voor resize)
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

            // Donkere overlay
            spriteBatch.Draw(_pixel, new Rectangle(0, 0, (int)screenWidth, (int)screenHeight), Color.Black * 0.7f);

            // Popup venster
            spriteBatch.Draw(_pixel, new Rectangle((int)popupX, (int)popupY, (int)popupWidth, (int)popupHeight), Color.Gray);

            // Titel
            string title = "Paused";
            Vector2 titleSize = _font.MeasureString(title);
            spriteBatch.DrawString(_font, title, new Vector2(popupX + (popupWidth - titleSize.X) / 2, popupY + 20), Color.White);

            // Opties als buttons (zelfde stijl als hoofdmenu)
            MouseState mouse = Mouse.GetState();
            Point mousePos = mouse.Position;
            for (int i = 0; i < _options.Length; i++)
            {
                Rectangle rect = _optionBounds[i];
                int paddingX = 30;
                int paddingY = 10;
                Rectangle buttonRect = new Rectangle(rect.X - paddingX/2, rect.Y - paddingY/2, rect.Width + paddingX, rect.Height + paddingY);

                Color buttonColor = new Color(60, 60, 60, 200); // normaal
                if (buttonRect.Contains(mousePos) || _selectedIndex == i)
                {
                    if (mouse.LeftButton == ButtonState.Pressed && buttonRect.Contains(mousePos))
                        buttonColor = new Color(30, 30, 30, 220); // klik
                    else
                        buttonColor = new Color(40, 40, 40, 210); // hover/selected
                }

                spriteBatch.Draw(_pixel, buttonRect, buttonColor);

                Vector2 textPos = new Vector2(
                    buttonRect.X + (buttonRect.Width - rect.Width) / 2,
                    buttonRect.Y + (buttonRect.Height - rect.Height) / 2
                );
                spriteBatch.DrawString(_font, _options[i], textPos, Color.White);
            }
        }
    }
}