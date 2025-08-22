using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForestQuest.Entities.Player
{
    public class PlayerControls
    {
        public Keys Up;
        public Keys Down;
        public Keys Left;
        public Keys Right;
        public Keys Attack;
        public Keys Interact;

        public static PlayerControls Default1 => new PlayerControls
        {
            Up = Keys.Z,
            Down = Keys.S,
            Left = Keys.Q,
            Right = Keys.D,
            Attack = Keys.Space,
            Interact = Keys.E
        };
        public static PlayerControls Default2 => new PlayerControls
        {
            Up = Keys.Up,
            Down = Keys.Down,
            Left = Keys.Left,
            Right = Keys.Right,
            Attack = Keys.K,
            Interact = Keys.L
        };
    }
}
