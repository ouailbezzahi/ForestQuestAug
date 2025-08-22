using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using ForestQuest.UI;

namespace ForestQuest.State
{
    public class OptionsMenu
    {
        private SpriteFont _font;

        private Rectangle _soundSliderBar, _sfxSliderBar;
        private Rectangle _soundKnob, _sfxKnob;
        private readonly int _sliderWidth = 200;
        private readonly int _sliderHeight = 8;
        private readonly int _knobWidth = 20;
        private readonly int _knobHeight = 28;

        private bool _draggingSound = false, _draggingSFX = false;
        private int _soundValue = 20; // 0-100
        private int _sfxValue = 60;   // 0-100

        private Rectangle _backButton;
        private bool _mouseWasPressed = false;

        // Cache the last viewport size to only recalc layout when needed
        private Point _lastViewportSize;

        public int SoundValue => _soundValue;
        public int SFXValue => _sfxValue;

        public OptionsMenu(ContentManager content, GraphicsDevice graphicsDevice)
        {
            _font = content.Load<SpriteFont>("Fonts/Font");
            _lastViewportSize = new Point(graphicsDevice.Viewport.Width, graphicsDevice.Viewport.Height);

            CalculateLayout(graphicsDevice);
        }

        private void EnsureLayout(GraphicsDevice graphicsDevice)
        {
            var vp = graphicsDevice.Viewport;
            if (_lastViewportSize.X != vp.Width || _lastViewportSize.Y != vp.Height)
            {
                _lastViewportSize = new Point(vp.Width, vp.Height);
                CalculateLayout(graphicsDevice);
            }
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

            _soundKnob = new Rectangle(
                _soundSliderBar.X + (_soundValue * (_sliderWidth - _knobWidth) / 100),
                _soundSliderBar.Y - 10,
                _knobWidth, _knobHeight);

            _sfxKnob = new Rectangle(
                _sfxSliderBar.X + (_sfxValue * (_sliderWidth - _knobWidth) / 100),
                _sfxSliderBar.Y - 10,
                _knobWidth, _knobHeight);

            _backButton = new Rectangle((int)(popupX + (popupWidth - 120) / 2), (int)(popupY + popupHeight - 50), 120, 32);
        }

        public bool Update(GraphicsDevice graphicsDevice)
        {
            EnsureLayout(graphicsDevice);

            MouseState mouse = Mouse.GetState();
            Point mousePos = mouse.Position;
            bool closeMenu = false;

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

            if (_backButton.Contains(mousePos) && mouse.LeftButton == ButtonState.Pressed && !_mouseWasPressed)
            {
                closeMenu = true;
            }

            _mouseWasPressed = mouse.LeftButton == ButtonState.Pressed;
            return closeMenu;
        }

        public void Draw(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice)
        {
            EnsureLayout(graphicsDevice);

            float screenWidth = graphicsDevice.Viewport.Width;
            float screenHeight = graphicsDevice.Viewport.Height;
            int popupWidth = 350;
            int popupHeight = 300;

            var popup = UiDraw.CenteredPopup(graphicsDevice, popupWidth, popupHeight);

            // Overlay
            UiDraw.Overlay(spriteBatch, graphicsDevice, Color.Black * 0.7f);

            // Popup
            UiDraw.Panel(spriteBatch, graphicsDevice, popup, Color.DarkSlateGray);

            // Title
            string title = "Options";
            Vector2 titleSize = _font.MeasureString(title);
            spriteBatch.DrawString(_font, title, new Vector2(popup.X + (popup.Width - titleSize.X) / 2, popup.Y + 20), Color.White);

            // Sound slider
            spriteBatch.DrawString(_font, $"Sound: {_soundValue}", new Vector2(_soundSliderBar.X - 70, _soundSliderBar.Y - 8), Color.White);
            spriteBatch.Draw(UiResources.Pixel(graphicsDevice), _soundSliderBar, Color.LightGray);
            spriteBatch.Draw(UiResources.Pixel(graphicsDevice), _soundKnob, Color.Orange);

            // SFX slider
            spriteBatch.DrawString(_font, $"SFX: {_sfxValue}", new Vector2(_sfxSliderBar.X - 70, _sfxSliderBar.Y - 8), Color.White);
            spriteBatch.Draw(UiResources.Pixel(graphicsDevice), _sfxSliderBar, Color.LightGray);
            spriteBatch.Draw(UiResources.Pixel(graphicsDevice), _sfxKnob, Color.Orange);

            // Back button
            MouseState mouse = Mouse.GetState();
            Point mousePos = mouse.Position;
            bool hovered = _backButton.Contains(mousePos);
            bool isDown = hovered && mouse.LeftButton == ButtonState.Pressed;
            UiDraw.Button(
                spriteBatch, graphicsDevice, _backButton, "Back", _font,
                hovered, isDown, selected: false,
                normal: Color.Gray, hover: new Color(40, 40, 40, 210), down: new Color(30, 30, 30, 220), selectedColor: new Color(70, 70, 110, 210),
                textColor: Color.White
            );
        }
    }
}