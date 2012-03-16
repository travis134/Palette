using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Palette
{
    [Serializable]
    public class Level
    {
        public int Number { get; set; }
        public string Name { get; set; }
        public Block[][] Data { get; set; }
        public Vector2 PlayerStart { get; set; }
        public Vector2 Goal { get; set; }
        public int PlayerColorIndex { get; set; }
        public int BackgroundColorIndex { get; set; }
        public Color[] ColorPalette { get; set; }
        public DrawStyle DrawStyle { get; set; }
    }
}
