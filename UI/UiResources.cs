using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace ForestQuest.UI
{
    public static class UiResources
    {
        private static GraphicsDevice _device;
        private static Texture2D _pixel;

        public static Texture2D Pixel(GraphicsDevice graphicsDevice)
        {
            if (_pixel == null || _device != graphicsDevice)
            {
                _device = graphicsDevice;
                _pixel?.Dispose();
                _pixel = new Texture2D(graphicsDevice, 1, 1);
                _pixel.SetData(new[] { Color.White });
            }
            return _pixel;
        }
    }
}