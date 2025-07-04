using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace ForestQuest.State
{
    public class OptionsMenu
    {
        private SpriteFont _font;
        private Texture2D _sliderBar, _sliderKnob;
        private Rectangle _soundSliderBar, _sfxSliderBar;
        private Rectangle _soundKnob, _sfxKnob;
        private int _sliderWidth = 200;
        private int _sliderHeight = 8;
        private int _knobWidth = 20;
        private int _knobHeight = 28;
        private bool _draggingSound = false, _draggingSFX = false;
        private int _soundValue = 20; // 0-100
        private int _sfxValue = 60;   // 0-100
        private Rectangle _backButton;
        private bool _mouseWasPressed = false;

        public int SoundValue => _soundValue;
        public int SFXValue => _sfxValue;

        public OptionsMenu(ContentManager content, GraphicsDevice graphicsDevice)
        {
            _font = content.Load<SpriteFont>("Fonts/Font");
            _sliderBar = new Texture2D(graphicsDevice, 1, 1);
            _sliderBar.SetData(new[] { Color.White });
            _sliderKnob = new Texture2D(graphicsDevice, 1, 1);
            _sliderKnob.SetData(new[] { Color.Gray });
            CalculateLayout(graphicsDevice);
        }

        private void CalculateLayout(GraphicsDevice graphicsDevice)
        {
            float screenWidth = graphicsDevice.Viewport.Width;
            float screenHeight = graphicsDevice.Viewport.Height;
            float popupWidth = 350f;
            float popupHeight = 300f;
            float popupX = (screenWidth - popupWidth) / 2;
            float popupY = (screenHeight - popupHeight) / 2;

            _soundSliderBar = new Rectangle((int)(popupX + 80), (int)(popupY + 80), _sliderWidth, _sliderHeight);
            _sfxSliderBar = new Rectangle((int)(popupX + 80), (int)(popupY + 140), _sliderWidth, _sliderHeight);
            _soundKnob = new Rectangle(_soundSliderBar.X + (_soundValue * (_sliderWidth - _knobWidth) / 100), _soundSliderBar.Y - 10, _knobWidth, _knobHeight);
            _sfxKnob = new Rectangle(_sfxSliderBar.X + (_sfxValue * (_sliderWidth - _knobWidth) / 100), _sfxSliderBar.Y - 10, _knobWidth, _knobHeight);
            _backButton = new Rectangle((int)(popupX + (popupWidth - 120) / 2), (int)(popupY + popupHeight - 50), 120, 32);
        }

        public bool Update(GraphicsDevice graphicsDevice)
        {
            MouseState mouse = Mouse.GetState();
            Point mousePos = mouse.Position;
            bool closeMenu = false;

            // Dragging logic
            if (mouse.LeftButton == ButtonState.Pressed)
            {
                if (!_mouseWasPressed)
                {
                    if (_soundKnob.Contains(mousePos)) _draggingSound = true;
                    if (_sfxKnob.Contains(mousePos)) _draggingSFX = true;
                }
                if (_draggingSound)
                {
                    int x = Math.Clamp(mousePos.X, _soundSliderBar.X, _soundSliderBar.X + _sliderWidth - _knobWidth);
                    _soundKnob.X = x;
                    _soundValue = (int)(((float)(x - _soundSliderBar.X) / (_sliderWidth - _knobWidth)) * 100);
                }
                if (_draggingSFX)
                {
                    int x = Math.Clamp(mousePos.X, _sfxSliderBar.X, _sfxSliderBar.X + _sliderWidth - _knobWidth);
                    _sfxKnob.X = x;
                    _sfxValue = (int)(((float)(x - _sfxSliderBar.X) / (_sliderWidth - _knobWidth)) * 100);
                }
            }
            else
            {
                _draggingSound = false;
                _draggingSFX = false;
            }

            // Back button
            if (_backButton.Contains(mousePos) && mouse.LeftButton == ButtonState.Pressed && !_mouseWasPressed)
            {
                closeMenu = true;
            }

            _mouseWasPressed = mouse.LeftButton == ButtonState.Pressed;
            CalculateLayout(graphicsDevice); // update knob positions
            return closeMenu;
        }

        public void Draw(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice)
        {
            float screenWidth = graphicsDevice.Viewport.Width;
            float screenHeight = graphicsDevice.Viewport.Height;
            float popupWidth = 350f;
            float popupHeight = 300f;
            float popupX = (screenWidth - popupWidth) / 2;
            float popupY = (screenHeight - popupHeight) / 2;

            // Overlay
            Texture2D overlay = new Texture2D(graphicsDevice, 1, 1);
            overlay.SetData(new[] { new Color(0, 0, 0, 0.7f) });
            spriteBatch.Draw(overlay, new Rectangle(0, 0, (int)screenWidth, (int)screenHeight), Color.Black * 0.7f);

            // Popup
            Texture2D popup = new Texture2D(graphicsDevice, 1, 1);
            popup.SetData(new[] { Color.DarkSlateGray });
            spriteBatch.Draw(popup, new Rectangle((int)popupX, (int)popupY, (int)popupWidth, (int)popupHeight), Color.DarkSlateGray);

            // Titel
            string title = "Options";
            Vector2 titleSize = _font.MeasureString(title);
            spriteBatch.DrawString(_font, title, new Vector2(popupX + (popupWidth - titleSize.X) / 2, popupY + 20), Color.White);

            // Sound slider
            spriteBatch.DrawString(_font, $"Sound: {_soundValue}", new Vector2(_soundSliderBar.X - 70, _soundSliderBar.Y - 8), Color.White);
            spriteBatch.Draw(_sliderBar, _soundSliderBar, Color.LightGray);
            spriteBatch.Draw(_sliderKnob, _soundKnob, Color.Orange);

            // SFX slider
            spriteBatch.DrawString(_font, $"SFX: {_sfxValue}", new Vector2(_sfxSliderBar.X - 70, _sfxSliderBar.Y - 8), Color.White);
            spriteBatch.Draw(_sliderBar, _sfxSliderBar, Color.LightGray);
            spriteBatch.Draw(_sliderKnob, _sfxKnob, Color.Orange);

            // Back button
            Texture2D btn = new Texture2D(graphicsDevice, 1, 1);
            btn.SetData(new[] { Color.Gray });
            spriteBatch.Draw(btn, _backButton, Color.Gray);
            string backText = "Back";
            Vector2 backSize = _font.MeasureString(backText);
            spriteBatch.DrawString(_font, backText, new Vector2(_backButton.X + (_backButton.Width - backSize.X) / 2, _backButton.Y + 4), Color.White);
        }
    }
} 