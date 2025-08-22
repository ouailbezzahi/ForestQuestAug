using Microsoft.Xna.Framework;

namespace ForestQuest.World.Levels
{
    public sealed class LevelData
    {
        public int[,] Tiles { get; }
        public Vector2 Player1Start { get; }
        public Vector2 Player2Start { get; }
        public int CoinSpawnCount { get; }

        public LevelData(int[,] tiles, Vector2 player1Start, Vector2 player2Start, int coinSpawnCount)
        {
            Tiles = tiles;
            Player1Start = player1Start;
            Player2Start = player2Start;
            CoinSpawnCount = coinSpawnCount;
        }
    }
}