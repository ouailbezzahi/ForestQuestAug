using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;

namespace ForestQuest.Entities.Enemies.Factory
{
    public interface IEnemyFactory
    {
        List<Enemy> CreateForLevel(ContentManager content, int level);
    }
}