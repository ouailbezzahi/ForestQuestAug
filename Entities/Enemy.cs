using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace ForestQuest.Entities
{
    public class Enemy
    {
        private Texture2D _texture;
        private Vector2 _position;

        public Enemy(Vector2 startPosition)
        {
            _position = startPosition;
        }

        public void LoadContent(ContentManager content, string textureName)
        {
            _texture = content.Load<Texture2D>(textureName);
        }

        public void Update()
        {
            // Vijand logica hier
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(_texture, _position, Color.White);
        }
    }
}