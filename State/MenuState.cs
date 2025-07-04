using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using ForestQuest.State;

namespace ForestQuest.State
{
    public class MenuState : State
    {
        private SpriteFont _font;
        private string[] _menuOptions = { "Single Player", "Multiplayer", "Quit" };
        private Vector2[] _menuPositions;
        private Rectangle[] _menuBounds;
        private Song _menuMusic;
        private ScrollingBackground _background;
        private State _nextState;

        public MenuState(Game1 game, ContentManager content, GraphicsDevice graphicsDevice)
            : base(game, content, graphicsDevice)
        {
            _menuPositions = new Vector2[3];
            _menuBounds = new Rectangle[3];
        }

        public override void LoadContent()
        {
            _font = _content.Load<SpriteFont>("Fonts/Font");

            // Achtergrondmuziek
            _menuMusic = _content.Load<Song>("Audio/forest_menu");
            MediaPlayer.IsRepeating = true;
            MediaPlayer.Play(_menuMusic);

            // Scrollende achtergrond
            _background = new ScrollingBackground(_content, _graphicsDevice, "Background/Menu/menu_background", 60f);

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

        public override void Update(GameTime gameTime)
        {
            _background.Update(gameTime);
            MouseState mouseState = Mouse.GetState();
            if (mouseState.LeftButton == ButtonState.Pressed)
            {
                Point mousePosition = mouseState.Position;

                for (int i = 0; i < _menuOptions.Length; i++)
                {
                    if (_menuBounds[i].Contains(mousePosition))
                    {
                        switch (i)
                        {
                            case 0: // Single Player
                                _game.ChangeState(new GameState(_game, _content, _graphicsDevice, false));
                                break;
                            case 1: // Multiplayer
                                _game.ChangeState(new GameState(_game, _content, _graphicsDevice, true));
                                break;
                            case 2: // Quit
                                _game.Exit();
                                break;
                        }
                    }
                }
            }
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            UpdateMenuLayout(); // Zorg dat alles gecentreerd blijft
            _graphicsDevice.Clear(Color.Black);

            spriteBatch.Begin();
            // Achtergrond vullen over het hele scherm
            int texWidth = _background != null ? _backgroundTextureWidth() : 0;
            int screenWidth = _graphicsDevice.Viewport.Width;
            int screenHeight = _graphicsDevice.Viewport.Height;
            float offset = _background != null ? _backgroundOffset() : 0f;
            float x = -offset;
            while (x < screenWidth)
            {
                spriteBatch.Draw(_backgroundTexture(), new Rectangle((int)x, 0, texWidth, screenHeight), Color.White);
                x += texWidth;
            }

            // Menu knoppen tekenen
            MouseState mouse = Mouse.GetState();
            Point mousePos = mouse.Position;
            for (int i = 0; i < _menuOptions.Length; i++)
            {
                Rectangle rect = _menuBounds[i];
                int paddingX = 30;
                int paddingY = 10;
                Rectangle buttonRect = new Rectangle(rect.X - paddingX/2, rect.Y - paddingY/2, rect.Width + paddingX, rect.Height + paddingY);

                Color buttonColor = new Color(60, 60, 60, 200); // normaal
                if (buttonRect.Contains(mousePos))
                {
                    if (mouse.LeftButton == ButtonState.Pressed)
                        buttonColor = new Color(30, 30, 30, 220); // klik
                    else
                        buttonColor = new Color(40, 40, 40, 210); // hover
                }

                Texture2D rectTex = new Texture2D(_graphicsDevice, 1, 1);
                rectTex.SetData(new[] { Color.White });
                spriteBatch.Draw(rectTex, buttonRect, buttonColor);

                Vector2 textPos = new Vector2(
                    buttonRect.X + (buttonRect.Width - rect.Width) / 2,
                    buttonRect.Y + (buttonRect.Height - rect.Height) / 2
                );
                spriteBatch.DrawString(_font, _menuOptions[i], textPos, Color.White);
            }
            spriteBatch.End();
        }

        // Helper methodes om bij de texture en offset van de achtergrond te komen
        private Texture2D _backgroundTexture()
        {
            // Toegang tot de texture van ScrollingBackground
            var field = typeof(ScrollingBackground).GetField("_texture", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            return (Texture2D)field.GetValue(_background);
        }
        private float _backgroundOffset()
        {
            var field = typeof(ScrollingBackground).GetField("_offset", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            return (float)field.GetValue(_background);
        }
        private int _backgroundTextureWidth()
        {
            return _backgroundTexture().Width;
        }
    }
}