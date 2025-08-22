using Microsoft.Xna.Framework.Content;

namespace ForestQuest.Items.Coin.Factory
{
    public interface ICoinFactory
    {
        CoinManager Create(ContentManager content, int mapWidthTiles, int mapHeightTiles, int spawnCount);
    }
}