using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace ForestQuest.Entities.Enemies.Factory
{
    public sealed class EnemyFactory : IEnemyFactory
    {
        public List<Enemy> CreateForLevel(ContentManager content, int level)
        {
            var enemies = new List<Enemy>();

            if (level == 1)
            {
                var catSpawns = new[]
                {
                    new Vector2(100,100),
                    new Vector2(300,100),
                    new Vector2(500,100),
                    new Vector2(100,300),
                    new Vector2(300,300)
                };
                foreach (var pos in catSpawns)
                {
                    var e = new Enemy(pos, EnemyType.Cat, 1);
                    e.LoadContent(content);
                    enemies.Add(e);
                }
            }
            else if (level == 2)
            {
                var dogSpawns = new[]
                {
                    new Vector2(120,120),
                    new Vector2(480,140),
                    new Vector2(720,180),
                    new Vector2(200,420),
                    new Vector2(520,440),
                    new Vector2(840,300),
                    new Vector2(300,600),
                    new Vector2(660,620)
                };
                foreach (var pos in dogSpawns)
                {
                    var e = new Enemy(pos, EnemyType.Dog, 2);
                    e.LoadContent(content);
                    enemies.Add(e);
                }
            }
            else if (level == 3)
            {
                var wolf = new Enemy(new Vector2(600, 400), EnemyType.Wolf, 3);
                wolf.LoadContent(content);
                enemies.Add(wolf);
                // (Optionally add extra dogs here later)
            }

            return enemies;
        }
    }
}