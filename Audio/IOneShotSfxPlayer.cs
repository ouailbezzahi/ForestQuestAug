using Microsoft.Xna.Framework.Content;

namespace ForestQuest.Audio
{
    public interface IOneShotSfxPlayer
    {
        // Attempts to load and play the first available asset from the list, only once.
        void TryPlayOnce(params string[] assetNames);
    }
}