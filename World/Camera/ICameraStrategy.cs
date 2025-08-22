using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace ForestQuest.World.Camera
{
    public interface ICameraStrategy
    {
        IReadOnlyList<CameraView> BuildViews(GraphicsDevice graphicsDevice,
                                             Viewport fullViewport,
                                             Vector2 player1Center,
                                             Vector2? player2Center,
                                             Point mapPixelSize);

        void DrawSeparators(SpriteBatch spriteBatch,
                            Viewport fullViewport,
                            IReadOnlyList<CameraView> views,
                            Texture2D pixel);
    }
}