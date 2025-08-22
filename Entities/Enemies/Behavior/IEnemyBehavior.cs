using Microsoft.Xna.Framework;

namespace ForestQuest.Entities.Enemies.Behavior
{
    public interface IEnemyBehavior
    {
        float Speed { get; }
        float FollowDistance { get; }
        float StopFollowDistance { get; }
        float AttackRange { get; }
    }
}