using System.Collections.Generic;
using ForestQuest.Entities.Player;
using ForestQuest.Entities.Enemies;

namespace ForestQuest.Physics
{
    public interface ICollisionResolver
    {
        void ResolvePlayersVsEnemies(Player player1, Player? player2, IEnumerable<Enemy> enemies, int[,] tiles);
    }
}