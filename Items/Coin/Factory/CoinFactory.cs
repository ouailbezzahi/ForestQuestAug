using Microsoft.Xna.Framework.Content;

namespace ForestQuest.Items.Coin.Factory
{
    public sealed class CoinFactory : ICoinFactory
    {
        public CoinManager Create(ContentManager content, int mapWidthTiles, int mapHeightTiles, int spawnCount)
        {
            // CoinManager already handles spawning internally via its constructor
            return new CoinManager(content, mapWidthTiles, mapHeightTiles, spawnCount);
        }
    }
}