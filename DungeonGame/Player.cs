using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DungeonGame
{
    public class Player : Entity
    {
        public Player(int h, int d, int r, int c) : base(h, d, r, c) { }
        public void PlayerInput(ref int nextRow, ref int nextCol, int mapRows)
        {
            Console.SetCursorPosition(0, mapRows + 3);
            Console.Write("이동 (W, A, S, D): ");
            ConsoleKeyInfo keyInfo = Console.ReadKey(true);

            switch (keyInfo.Key)
            {
                case ConsoleKey.W: nextRow--; break;
                case ConsoleKey.S: nextRow++; break;
                case ConsoleKey.A: nextCol--; break;
                case ConsoleKey.D: nextCol++; break;
            }
        }

        public override void Dying()
        {
            Console.WriteLine("플레이어 사망!");
            OnDeath(this);
        }
    }
}
