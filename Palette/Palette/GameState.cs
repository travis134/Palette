using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Palette
{
    [Serializable]
    public class GameState
    {
        public int Number { get; set; }
        public Block[][] Data { get; set; }
        public Vector2 PlayerPosition { get; set; }
        public int PlayerColorIndex { get; set; }
        public int BackgroundColorIndex { get; set; }
    }
}
