using System.Collections.Generic;
using Microsoft.Xna.Framework;
using ForestQuest.Entities.Player;
using ForestQuest.Entities.Enemies;

namespace ForestQuest.Combat
{
    public sealed class DefaultCombatResolver : ICombatResolver
    {
        private readonly int _wolfHitDamage;

        // Per-swing tracking
        private readonly HashSet<Enemy> _hitEnemiesP1 = new();
        private readonly HashSet<Enemy> _hitEnemiesP2 = new();
        private bool _prevAttackActiveP1;
        private bool _prevAttackActiveP2;

        public DefaultCombatResolver(int wolfHitDamage)
        {
            _wolfHitDamage = wolfHitDamage;
        }

        public void BeforeAttacks(Player player1, Player? player2)
        {
            // Reset sets when attack finished this frame (like original logic)
            if (!player1.IsAttackActive && _prevAttackActiveP1)
                _hitEnemiesP1.Clear();

            if (player2 != null && !player2.IsAttackActive && _prevAttackActiveP2)
                _hitEnemiesP2.Clear();
        }

        public int ResolvePlayerAttacks(IList<Enemy> enemies)
        {
            int newlyKilled = 0;

            // P1
            ResolveOnePlayerAttack(enemies, isP2: false, ref newlyKilled);

            // P2 (if exists)
            ResolveOnePlayerAttack(enemies, isP2: true, ref newlyKilled);

            return newlyKilled;
        }

        public void ResolveEnemyDamageToPlayers(IList<Enemy> enemies, Player player1, Player? player2)
        {
            Rectangle p1Rect = new((int)player1.Position.X, (int)player1.Position.Y, player1.FrameWidth, player1.FrameHeight);
            foreach (var enemy in enemies)
                if (enemy.TryDealDamage(p1Rect, out int dmg1))
                    player1.ApplyDamage(dmg1);

            if (player2 != null)
            {
                Rectangle p2Rect = new((int)player2.Position.X, (int)player2.Position.Y, player2.FrameWidth, player2.FrameHeight);
                foreach (var enemy in enemies)
                    if (enemy.TryDealDamage(p2Rect, out int dmg2))
                        player2.ApplyDamage(dmg2);
            }
        }

        public void AfterAttacks(Player player1, Player? player2)
        {
            _prevAttackActiveP1 = player1.IsAttackActive;
            _prevAttackActiveP2 = player2?.IsAttackActive == true;
        }

        // Internals

        private Player? _p1;
        private Player? _p2;

        // The caller must have assigned players via BeforeAttacks/AfterAttacks call sequence.
        private void ResolveOnePlayerAttack(IList<Enemy> enemies, bool isP2, ref int newlyKilled)
        {
            Player? attacker = isP2 ? _p2 : _p1;
            if (attacker == null) return; // will be null until set in Before/After by GameState wrapper

            if (!attacker.IsAttackActive) return;

            Rectangle attackHit = attacker.GetAttackHitbox();
            var hitSet = isP2 ? _hitEnemiesP2 : _hitEnemiesP1;

            foreach (var enemy in enemies)
            {
                if (!enemy.IsDead &&
                    attackHit.Intersects(enemy.BoundingBox) &&
                    !hitSet.Contains(enemy))
                {
                    if (enemy.Type == EnemyType.Wolf)
                        enemy.ApplyDamage(_wolfHitDamage);
                    else
                        enemy.Kill();

                    hitSet.Add(enemy);

                    if (enemy.IsDead)
                        newlyKilled++;
                }
            }
        }

        // Provide players to resolver (tie lifecycle to GameState Update)
        public void SetPlayers(Player p1, Player? p2)
        {
            _p1 = p1;
            _p2 = p2;
        }
    }
}