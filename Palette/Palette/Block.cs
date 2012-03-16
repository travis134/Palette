using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Palette
{
    [Serializable]
    public class Block
    {
        public int ColorIndex { get; set; }
        public bool IsPersistent { get; set; }
    }
}
