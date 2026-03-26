using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DungeonGame
{
    // 맵 매니저
    public class Map
    {
        public char[,] grid;

        // 맵 생성
        public char[,] CreateMap(int row, int col, int mapIdx, int totalMaps)
        {
            grid = new char[row, col];

            // 테두리 벽 생성
            for (int i = 0; i < row; i++)
            {
                for (int j = 0; j < col; j++)
                {
                    if (i == 0 || i == row - 1 || j == 0 || j == col - 1)
                        grid[i, j] = '#';
                    else
                        grid[i, j] = ' ';
                }
            }
            // 문 생성

            if (mapIdx > 0)
                grid[1, 1] = 'B';
            if (mapIdx < totalMaps - 1)
                grid[row - 2, col - 2] = 'D';


            // 랜덤 벽 생성
            Random rand = new Random();
            int randCount = rand.Next(3, 5);
            for (int k = 0; k < randCount; k++)
            {
                int randomRow = rand.Next(1, row - 1);
                int randomCol = rand.Next(1, col - 1);
                if (grid[randomRow, randomCol] == ' ')
                    grid[randomRow, randomCol] = '#';
                else
                    k--;
            }
            return grid;
        }

        // 플레이어 생성 랜덤 좌표
        public (int, int) GetRandomPlayer()
        {
            Random rand = new Random();
            int row = grid.GetLength(0);
            int col = grid.GetLength(1);
            while (true)
            {
                int randomRow = rand.Next(1, row - 1);
                int randomCol = rand.Next(1, col - 1);
                if (grid[randomRow, randomCol] == ' ')
                    return (randomRow, randomCol);
            }
        }

        // 몬스터 생성
        public void InsertMonster(int count, List<Monster> monsters, int mapIdx)
        {
            int row = grid.GetLength(0);
            int col = grid.GetLength(1);
            Random rand = new Random();
            for (int i = 0; i < count; i++)
            {
                int randomRow = rand.Next(0, row);
                int randomCol = rand.Next(0, col);
                if (grid[randomRow, randomCol] == ' ')
                {
                    grid[randomRow, randomCol] = 'M';
                    monsters.Add(new Monster(20, 5, randomRow, randomCol, mapIdx));
                }
                else
                    i--;
            }
        }

        // 맵 프린트
        public void PrintMap()
        {
            int row = grid.GetLength(0);
            int col = grid.GetLength(1);

            for (int i = 0; i < row; i++)
            {
                for (int j = 0; j < col; j++)
                    Console.Write(grid[i, j]);
                Console.WriteLine();
            }
        }
    }
}
