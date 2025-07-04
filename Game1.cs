using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ForestQuest.State;

namespace ForestQuest
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private ForestQuest.State.State _currentState;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            _graphics.PreferredBackBufferWidth = 800;
            _graphics.PreferredBackBufferHeight = 600;
            Window.AllowUserResizing = true;
        }

        protected override void Initialize()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _currentState = new MenuState(this, Content, GraphicsDevice);
            _currentState.LoadContent();
            base.Initialize();
        }

        public void ChangeState(ForestQuest.State.State newState)
        {
            _currentState = newState;
            _currentState.LoadContent();
        }

        protected override void Update(GameTime gameTime)
        {
            _currentState.Update(gameTime);
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            _currentState.Draw(gameTime, _spriteBatch);
            base.Draw(gameTime);
        }
    }
}