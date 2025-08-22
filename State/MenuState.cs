using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using ForestQuest.State;
using ForestQuest.World.Background;
using ForestQuest.UI; // NEW

namespace ForestQuest.State
{
    public class MenuState : State
    {
        private SpriteFont _font;
        private string[] _menuOptions = { "Single Player", "Multiplayer", "Quit" };
        private Vector2[] _menuPositions;
        private Rectangle[] _menuBounds;
        private Song _menuMusic;
        private IScrollingBackground _background; // CHANGED: use abstraction
        private State _nextState;
        private Texture2D _logo;

        private MouseState _prevMouse;

        // Keyboard navigation
        private int _selectedIndex = 0;
        private KeyboardState _prevKeyboard;

        // NEW: reusable 1x1 pixel
        private Texture2D _pixel;

        public MenuState(Game1 game, ContentManager content, GraphicsDevice graphicsDevice)
            : base(game, content, graphicsDevice)
        {
            _menuPositions = new Vector2[3];
            _menuBounds = new Rectangle[3];
        }

        public override void LoadContent()
        {
            _font = _content.Load<SpriteFont>("Fonts/Font");

            // Logo laden
            _logo = _content.Load<Texture2D>("Background/Logo/ForestQuestLogo");

            // Achtergrondmuziek
            _menuMusic = _content.Load<Song>("Audio/forest_menu");
            MediaPlayer.IsRepeating = true;
            MediaPlayer.Play(_menuMusic);

            // Scrollende achtergrond via adapter (no reflection here)
            var impl = new ForestQuest.World.Background.ScrollingBackground(_content, _graphicsDevice, "Background/Menu/menu_background", 60f);
            _background = new ForestQuest.World.Background.ScrollingBackgroundAdapter(impl);

            // 1x1 pixel
            _pixel = new Texture2D(_graphicsDevice, 1, 1);
            _pixel.SetData(new[] { Color.White });

            // Calculate centered positions for menu options
            UpdateMenuLayout();
        }

        private void UpdateMenuLayout()
        {
            float screenWidth = _graphicsDevice.Viewport.Width;
            float screenHeight = _graphicsDevice.Viewport.Height;
            float spacing = 50f; // Vertical spacing between options
            for (int i = 0; i < _menuOptions.Length; i++)
            {
                Vector2 textSize = _font.MeasureString(_menuOptions[i]);
                _menuPositions[i] = new Vector2(
                    (screenWidth - textSize.X) / 2,
                    screenHeight / 2 - (_menuOptions.Length - 1) * spacing / 2 + i * spacing
                );
                _menuBounds[i] = new Rectangle(
                    (int)_menuPositions[i].X,
                    (int)_menuPositions[i].Y,
                    (int)textSize.X,
                    (int)textSize.Y
                );
            }
        }

        // Build hit-test rectangles that match the drawn buttons (with padding)
        private void RebuildButtonBoundsForHitTest()
        {
            int screenWidth = _graphicsDevice.Viewport.Width;
            int screenHeight = _graphicsDevice.Viewport.Height;
            float spacing = 40f;

            int logoWidth = screenWidth / 4;
            int logoHeight = _logo != null ? (int)((float)_logo.Height / _logo.Width * logoWidth) : 0;
            int logoY = screenHeight / 12;
            int buttonsStartY = logoY + logoHeight + (int)spacing;
            int currentY = buttonsStartY;

            for (int i = 0; i < _menuOptions.Length; i++)
            {
                Vector2 textSize = _font.MeasureString(_menuOptions[i]);
                int paddingX = 30;
                int paddingY = 10;
                int buttonWidth = (int)textSize.X + paddingX;
                int buttonHeight = (int)textSize.Y + paddingY;
                int buttonX = (screenWidth - buttonWidth) / 2;
                int buttonY = currentY;
                _menuBounds[i] = new Rectangle(buttonX, buttonY, buttonWidth, buttonHeight);

                currentY += buttonHeight + (int)spacing;
            }
        }

        public override void Update(GameTime gameTime)
        {
            _background.Update(gameTime);

            // Keep hit-test bounds in sync with what Draw renders
            RebuildButtonBoundsForHitTest();

            // Keyboard navigation (Up/Down to select, Enter to confirm)
            var kb = Keyboard.GetState();
            bool pressed(Keys k) => kb.IsKeyDown(k) && !_prevKeyboard.IsKeyDown(k);

            if (pressed(Keys.Up))
                _selectedIndex = (_selectedIndex - 1 + _menuOptions.Length) % _menuOptions.Length;

            if (pressed(Keys.Down))
                _selectedIndex = (_selectedIndex + 1) % _menuOptions.Length;

            if (pressed(Keys.Enter))
            {
                ExecuteMenuOption(_selectedIndex);
                _prevKeyboard = kb; // ensure no double-trigger
                return;
            }

            // Mouse handling
            MouseState mouse = Mouse.GetState();
            bool clickReleased = _prevMouse.LeftButton == ButtonState.Pressed && mouse.LeftButton == ButtonState.Released;

            // Hover updates selection for consistency with keyboard
            Point pos = mouse.Position;
            for (int i = 0; i < _menuOptions.Length; i++)
            {
                if (_menuBounds[i].Contains(pos))
                {
                    _selectedIndex = i;
                    if (clickReleased)
                    {
                        ExecuteMenuOption(i);
                        _prevMouse = mouse;
                        _prevKeyboard = kb;
                        return;
                    }
                    break;
                }
            }

            _prevMouse = mouse;
            _prevKeyboard = kb;
        }

        private void ExecuteMenuOption(int index)
        {
            switch (index)
            {
                case 0:
                    System.Diagnostics.Debug.WriteLine("[Menu] Start Single Player");
                    _game.ChangeState(new GameState(_game, _content, _graphicsDevice, false, 1));
                    break;
                case 1:
                    System.Diagnostics.Debug.WriteLine("[Menu] Start Multiplayer");
                    _game.ChangeState(new GameState(_game, _content, _graphicsDevice, true, 1));
                    break;
                case 2:
                    _game.Exit();
                    break;
            }
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            UpdateMenuLayout(); // Zorg dat alles gecentreerd blijft
            _graphicsDevice.Clear(Color.Black);

            spriteBatch.Begin();

            // Achtergrond vullen over het hele scherm
            int texWidth = _background.TextureWidth;
            int screenWidth = _graphicsDevice.Viewport.Width;
            int screenHeight = _graphicsDevice.Viewport.Height;
            float offset = _background.Offset;
            float x = -offset;
            while (x < screenWidth)
            {
                spriteBatch.Draw(_background.Texture, new Rectangle((int)x, 0, texWidth, screenHeight), Color.White);
                x += texWidth;
            }

            // Logo tekenen boven de knoppen
            int logoWidth = screenWidth / 4; // 2x kleiner dan voorheen
            int logoHeight = _logo != null ? (int)((float)_logo.Height / _logo.Width * logoWidth) : 0;
            int logoX = (screenWidth - logoWidth) / 2;
            int logoY = screenHeight / 12; // iets hoger voor meer ruimte
            if (_logo != null)
            {
                spriteBatch.Draw(_logo, new Rectangle(logoX, logoY, logoWidth, logoHeight), Color.White);
            }

            // Menu knoppen tekenen ONDER het logo
            MouseState mouse = Mouse.GetState();
            Point mousePos = mouse.Position;
            float spacing = 40f;
            int buttonsStartY = logoY + logoHeight + (int)spacing;
            int currentY = buttonsStartY;

            for (int i = 0; i < _menuOptions.Length; i++)
            {
                Vector2 textSize = _font.MeasureString(_menuOptions[i]);
                int paddingX = 30;
                int paddingY = 10;
                int buttonWidth = (int)textSize.X + paddingX;
                int buttonHeight = (int)textSize.Y + paddingY;
                int buttonX = (screenWidth - buttonWidth) / 2;
                int buttonY = currentY;
                Rectangle buttonRect = new Rectangle(buttonX, buttonY, buttonWidth, buttonHeight);

                // Update hit bounds to match Draw pass (safe if called multiple times)
                _menuBounds[i] = buttonRect;

                bool hovered = buttonRect.Contains(mousePos);
                bool selected = (i == _selectedIndex);
                bool isDown = hovered && mouse.LeftButton == ButtonState.Pressed;

                UiDraw.Button(
                    spriteBatch, _graphicsDevice, buttonRect, _menuOptions[i], _font,
                    hovered, isDown, selected,
                    normal: new Color(60, 60, 60, 200),
                    hover: new Color(40, 40, 40, 210),
                    down:  new Color(30, 30, 30, 220),
                    selectedColor: new Color(70, 70, 110, 210),
                    textColor: Color.White
                );

                currentY += buttonHeight + (int)spacing;
            }
            spriteBatch.End();
        }
    }
}