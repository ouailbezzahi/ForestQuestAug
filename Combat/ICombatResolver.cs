using System.Collections.Generic;
using ForestQuest.Entities.Player;
using ForestQuest.Entities.Enemies;

namespace ForestQuest.Combat
{
    public interface ICombatResolver
    {
        // Clear per-swing tracking when an attack just finished (based on previous-frame flags)
        void BeforeAttacks(Player player1, Player? player2);

        // Apply current attacks, return number of enemies newly killed this frame
        int ResolvePlayerAttacks(IList<Enemy> enemies);

        // Deal enemy damage to players based on enemy attack hitboxes and current frames
        void ResolveEnemyDamageToPlayers(IList<Enemy> enemies, Player player1, Player? player2);

        // Record current attack flags to compare next frame
        void AfterAttacks(Player player1, Player? player2);
    }
}