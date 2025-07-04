using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace ForestQuest.State
{
    public class MenuState : State
    {
        private SpriteFont _font;
        private string[] _menuOptions = { "Single Player", "Multiplayer", "Quit" };
        private Vector2[] _menuPositions;
        private Rectangle[] _menuBounds;

        public MenuState(Game1 game, ContentManager content, GraphicsDevice graphicsDevice)
            : base(game, content, graphicsDevice)
        {
            _menuPositions = new Vector2[3];
            _menuBounds = new Rectangle[3];
        }

        public override void LoadContent()
        {
            _font = _content.Load<SpriteFont>("Fonts/Font");

            // Calculate centered positions for menu options
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
            _graphicsDevice.Clear(Color.Black);

            spriteBatch.Begin();
            for (int i = 0; i < _menuOptions.Length; i++)
            {
                spriteBatch.DrawString(_font, _menuOptions[i], _menuPositions[i], Color.White);
            }
            spriteBatch.End();
        }
    }
}