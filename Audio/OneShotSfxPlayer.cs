using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;

namespace ForestQuest.Audio
{
    public sealed class OneShotSfxPlayer : IOneShotSfxPlayer
    {
        private readonly ContentManager _content;
        private SoundEffect? _sfx;
        private bool _played;

        public OneShotSfxPlayer(ContentManager content)
        {
            _content = content;
        }

        public void TryPlayOnce(params string[] assetNames)
        {
            if (_played) return;

            if (_sfx == null)
            {
                foreach (var name in assetNames)
                {
                    try
                    {
                        _sfx = _content.Load<SoundEffect>(name);
                        if (_sfx != null) break;
                    }
                    catch
                    {
                        // try next
                    }
                }
            }

            _sfx?.Play();
            _played = true;
        }
    }
}