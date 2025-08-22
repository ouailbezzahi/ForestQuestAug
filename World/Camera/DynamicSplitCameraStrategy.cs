using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace ForestQuest.World.Camera
{
    public class DynamicSplitCameraStrategy : ICameraStrategy
    {
        private SplitKind _lastSplitKind = SplitKind.None;

        public IReadOnlyList<CameraView> BuildViews(GraphicsDevice graphicsDevice,
                                                    Viewport fullViewport,
                                                    Vector2 player1Center,
                                                    Vector2? player2Center,
                                                    Point mapPixelSize)
        {
            // Single player: full view focused on player1
            if (player2Center is null)
            {
                _lastSplitKind = SplitKind.None;
                return new[]
                {
                    new CameraView(fullViewport, player1Center)
                };
            }

            var p2 = player2Center.Value;

            float distX = Math.Abs(player1Center.X - p2.X);
            float distY = Math.Abs(player1Center.Y - p2.Y);

            float normX = distX / Math.Max(1f, fullViewport.Width);
            float normY = distY / Math.Max(1f, fullViewport.Height);
            float threshold = 0.6f;

            // Vertical split (left/right), P1 left, P2 right
            if (normX >= normY && normX > threshold)
            {
                int leftWidth = fullViewport.Width / 2;
                int rightWidth = fullViewport.Width - leftWidth;

                var vpLeft = new Viewport(fullViewport.X, fullViewport.Y, leftWidth, fullViewport.Height);
                var vpRight = new Viewport(fullViewport.X + leftWidth, fullViewport.Y, rightWidth, fullViewport.Height);

                _lastSplitKind = SplitKind.Vertical;
                return new[]
                {
                    new CameraView(vpLeft,  player1Center),
                    new CameraView(vpRight, p2)
                };
            }

            // Horizontal split (top/bottom), P1 top, P2 bottom
            if (normY > threshold)
            {
                int topHeight = fullViewport.Height / 2;
                int bottomHeight = fullViewport.Height - topHeight;

                var vpTop    = new Viewport(fullViewport.X, fullViewport.Y, fullViewport.Width, topHeight);
                var vpBottom = new Viewport(fullViewport.X, fullViewport.Y + topHeight, fullViewport.Width, bottomHeight);

                _lastSplitKind = SplitKind.Horizontal;
                return new[]
                {
                    new CameraView(vpTop,    player1Center),
                    new CameraView(vpBottom, p2)
                };
            }

            // Shared single camera centered between players
            _lastSplitKind = SplitKind.None;
            Vector2 mid = (player1Center + p2) * 0.5f;
            return new[]
            {
                new CameraView(fullViewport, mid)
            };
        }

        public void DrawSeparators(SpriteBatch spriteBatch,
                                   Viewport fullViewport,
                                   IReadOnlyList<CameraView> views,
                                   Texture2D pixel)
        {
            if (views.Count != 2) return;

            switch (_lastSplitKind)
            {
                case SplitKind.Vertical:
                    // Vertical line between left and right
                    int x = views[0].Viewport.Width - 1;
                    spriteBatch.Draw(pixel, new Rectangle(x, 0, 2, fullViewport.Height), Color.Black * 0.75f);
                    break;

                case SplitKind.Horizontal:
                    // Horizontal line between top and bottom
                    int y = views[0].Viewport.Height - 1;
                    spriteBatch.Draw(pixel, new Rectangle(0, y, fullViewport.Width, 2), Color.Black * 0.75f);
                    break;

                case SplitKind.None:
                default:
                    break;
            }
        }
    }
}