using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DungeonGame
{
    public interface ITakeDamageable
    {
        void TakeDamage(int power);
    }
    public delegate void Death(Entity deadEntity);
    public abstract class Entity : ITakeDamageable
    {
        public int health { get; set; }
        public int damage { get; set; }
        public int row { get; set; }
        public int col { get; set; }
        public Death OnDeath;
        public Entity(int h, int d, int r, int c) { health = h; damage = d; row = r; col = c; }

        public void TakeDamage(int power)
        {
            health -= power;
            if (health <= 0)
                Dying();
        }
        public void Attak(Entity target) { target.TakeDamage(damage); }

        public abstract void Dying();
    }
}
