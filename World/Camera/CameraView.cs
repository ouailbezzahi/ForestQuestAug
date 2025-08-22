using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForestQuest.World.Camera
{    public sealed class CameraView
    {
        public Viewport Viewport { get; }
        public Vector2 Focus { get; }

        public CameraView(Viewport viewport, Vector2 focus)
        {
            Viewport = viewport;
            Focus = focus;
        }
    }
}
