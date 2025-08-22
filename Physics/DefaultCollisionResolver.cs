using System.Collections.Generic;
using Microsoft.Xna.Framework;
using ForestQuest.Entities.Player;
using ForestQuest.Entities.Enemies;

namespace ForestQuest.Physics
{
    public sealed class DefaultCollisionResolver : ICollisionResolver
    {
        public void ResolvePlayersVsEnemies(Player player1, Player? player2, IEnumerable<Enemy> enemies, int[,] tiles)
        {
            // Run twice like before to better separate overlaps
            for (int iter = 0; iter < 2; iter++)
            {
                foreach (var enemy in enemies)
                {
                    if (enemy.IsDead) continue;
                    Rectangle eRect = enemy.BoundingBox;

                    Rectangle p1Rect = new((int)player1.Position.X, (int)player1.Position.Y, player1.FrameWidth, player1.FrameHeight);
                    if (p1Rect.Intersects(eRect))
                        player1.ResolveCollisionWith(eRect, tiles);

                    if (player2 != null)
                    {
                        Rectangle p2Rect = new((int)player2.Position.X, (int)player2.Position.Y, player2.FrameWidth, player2.FrameHeight);
                        if (p2Rect.Intersects(eRect))
                            player2.ResolveCollisionWith(eRect, tiles);
                    }
                }
            }
        }
    }
}