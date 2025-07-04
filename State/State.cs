using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace ForestQuest.State
{
    public abstract class State
    {
        protected Game1 _game;
        protected ContentManager _content;
        protected GraphicsDevice _graphicsDevice;

        public State(Game1 game, ContentManager content, GraphicsDevice graphicsDevice)
        {
            _game = game;
            _content = content;
            _graphicsDevice = graphicsDevice;
        }

        public abstract void LoadContent();
        public abstract void Update(GameTime gameTime);
        public abstract void Draw(GameTime gameTime, SpriteBatch spriteBatch);
    }
}