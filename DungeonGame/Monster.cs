using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DungeonGame
{
    public class Monster : Entity
    {
        public int MapIndex { get; set; }
        public Monster(int h, int d, int r, int c, int mapIndex) : base(h, d, r, c)
        {
            MapIndex = mapIndex;
        }
        public override void Dying()
        {
            OnDeath(this);
        }
    }
}
