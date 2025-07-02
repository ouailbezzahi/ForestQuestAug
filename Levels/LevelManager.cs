using System.Collections.Generic;
using ForestQuest.Entities;

namespace ForestQuest.Levels
{
    public class LevelManager
    {
        public List<Enemy> Enemies { get; private set; }

        public LevelManager()
        {
            Enemies = new List<Enemy>();
        }

        public void LoadLevel(int levelNumber)
        {
            // Laad vijanden en items afhankelijk van het level
            if (levelNumber == 1)
            {
                Enemies.Add(new Enemy(new Microsoft.Xna.Framework.Vector2(200, 200)));
            }
        }
    }
}
