using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Net.Http.Headers;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace DungeonGame
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.CursorVisible = false;
            Console.Write("세로: ");
            int row = int.Parse(Console.ReadLine());
            Console.Write("가로: ");
            int col = int.Parse(Console.ReadLine());
            Console.Write("맵: ");
            int mapCount = int.Parse(Console.ReadLine());
            Console.SetCursorPosition(0, 0);
            DungeonGame game = new DungeonGame(row, col);

            game.PlayGame(mapCount);
            game.SaveJson();
        }
    }
}
