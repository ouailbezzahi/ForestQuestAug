using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForestQuest.Entities.Enemies.Behavior
{
    public sealed class CatBehavior : IEnemyBehavior
    {
        public float Speed => 1.2f;
        public float FollowDistance => 140f;
        public float StopFollowDistance => 190f;
        public float AttackRange => 42f;
    }
}
