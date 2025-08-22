using Microsoft.Xna.Framework;

namespace ForestQuest.Entities.Enemies.Behavior
{
    public static class EnemyBehaviorProvider
    {
        public static IEnemyBehavior Get(EnemyType type) => type switch
        {
            EnemyType.Cat => new CatBehavior(),
            EnemyType.Dog => new DogBehavior(),
            EnemyType.Wolf => new WolfBehavior(),
            _ => new CatBehavior()
        };
    }
}