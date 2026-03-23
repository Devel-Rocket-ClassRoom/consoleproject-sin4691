using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Net.Http.Headers;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace _260313
{
    // 확장성: 맵 이동을 아예 자유롭게 해서 갈 수도 있지 않을까
    // 맵을 여러개 만들어서 리스트에 담고 문을 자동으로 왓다갔다 하는
    // currentMap = map[i] 같은걸 활용?
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
        }
    }

    // 게임 매니저
    public class DungeonGame
    {
        Player player;
        List<Monster> monsters = new List<Monster>();
        List<Map> maps = new List<Map>();
        int rows;
        int cols;
        int monsterCount = 0;
        int currentMapIndex = 0;
        public Map CurrentMap => maps[currentMapIndex];

        public DungeonGame(int r, int c)
        {
            rows = r;
            cols = c;
        }

        // 맵 3개를 만들고 플레이어를 제일 마지막에 생성해야함

        // 게임 플레이
        public void PlayGame(int stageCount)
        {
            Random rand = new Random();
            for (int i = 0; i < stageCount; i++)
            {
                int count = rand.Next(1, 5);
                monsterCount += count;
                maps.Add(new Map());
                maps[i].CreateMap(rows, cols, i, stageCount);

                maps[i].InsertMonster(count, monsters, i);
                foreach (Monster m in monsters)
                {
                    m.OnDeath = OnMonsterDead;
                }
            }
            var spawnPos = maps[0].GetRandomPlayer();
            player = new Player(100, 10, spawnPos.Item1, spawnPos.Item2);
            maps[0].grid[player.row, player.col] = 'P';
            player.OnDeath = OnpPlayerDead;

            bool stageClear = false;
            int monsterTurnCount = 3;
            while (!stageClear)
            {
                Console.SetCursorPosition(0, 0);
                CurrentMap.PrintMap();
                PlayerMoveSet();
                if (monsterCount <= 0)
                {
                    PrintMessage("스테이지 클리어!");
                    stageClear = true;
                }
                monsterTurnCount--;
                if (monsterTurnCount <= 0)
                {
                    MonsterMoveSet();
                    monsterTurnCount = 3;
                }
            }
            Console.WriteLine("게임 클리어!");
        }
        public void OnpPlayerDead(Entity deadEntity)
        {
            Environment.Exit(0);
        }
        public void OnMonsterDead(Entity deadEntity)
        {
            Monster deadMonster = (Monster)deadEntity;
            monsters.Remove(deadMonster);
            CurrentMap.grid[deadMonster.row, deadMonster.col] = ' ';
            monsterCount--;
        }

        public void SetPlayerPosition()
        {

            var nextPos = CurrentMap.GetRandomPlayer();
            player.row = nextPos.Item1;
            player.col = nextPos.Item2;
            CurrentMap.grid[player.row, player.col] = 'P';
        }

        // 몬스터의 플레이어 추적 이동
        public void MonsterMoveSet()
        {
            foreach (Monster m in monsters)
            {
                if (m.MapIndex != currentMapIndex) continue;
                int nextMRow = m.row;
                int nextMCol = m.col;
                if (player.col < nextMCol)
                    nextMCol--;
                else if (player.col > nextMCol)
                    nextMCol++;
                else if (player.row > nextMRow)
                    nextMRow++;
                else if (player.row < nextMRow)
                    nextMRow--;
                if (CurrentMap.grid[nextMRow, nextMCol] == ' ')
                {
                    CurrentMap.grid[m.row, m.col] = ' ';
                    m.row = nextMRow;
                    m.col = nextMCol;
                    CurrentMap.grid[m.row, m.col] = 'M';

                }
                else if (CurrentMap.grid[nextMRow, nextMCol] == 'P')
                {
                    m.Attak(player);
                }
            }
        }

        // 이동
        public void PlayerMoveSet()
        {
            int nextRow = player.row;
            int nextCol = player.col;

            player.PlayerInput(ref nextRow, ref nextCol, CurrentMap.grid.GetLength(0));


            switch (Check(nextRow, nextCol))
            {
                case '#':
                    PrintMessage("벽에 막혔습니다!");
                    break;
                case 'M':
                    Monster targetMonster = null;
                    foreach (Monster m in monsters)
                    {
                        if (m.row == nextRow && m.col == nextCol)
                        {
                            targetMonster = m;
                            break;
                        }
                    }
                    player.Attak(targetMonster);
                    PrintMessage("몬스터 공격!");
                    break;
                case 'D':
                    CurrentMap.grid[player.row, player.col] = ' ';
                    currentMapIndex++;

                    SetPlayerPosition();
                    PrintMessage("다음 맵으로!");
                    break;
                case ' ':
                    PrintMessage("이동 성공!");
                    PlayerMoveConfirm(nextRow, nextCol);
                    break;
                case 'B':
                    CurrentMap.grid[player.row, player.col] = ' ';
                    currentMapIndex--;

                    SetPlayerPosition();
                    PrintMessage("이전 구역으로 이동!");
                    break;
                default:
                    PrintMessage("조작 오류!");
                    break;
            }
        }

        public void PrintMessage(string msg)
        {
            Console.SetCursorPosition(0, rows + 1);

            Console.WriteLine(msg.PadRight(50));
        }

        // 플레이어 이동 확정
        public void PlayerMoveConfirm(int nextRow, int nextCol)
        {
            CurrentMap.grid[player.row, player.col] = ' ';
            player.row = nextRow;
            player.col = nextCol;
            CurrentMap.grid[player.row, player.col] = 'P';
        }
        public char Check(int nextRow, int nextCol) { return CurrentMap.grid[nextRow, nextCol]; }

    }

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

    public class Player : Entity
    {
        public Player(int h, int d, int r, int c) : base(h, d, r, c) { }
        public void PlayerInput(ref int nextRow, ref int nextCol, int mapRows)
        {
            Console.SetCursorPosition(0, mapRows + 3);
            Console.Write("이동 (W, A, S, D): ");
            string input = Console.ReadLine().ToUpper();

            switch (input)
            {
                case "W": nextRow--; break;
                case "S": nextRow++; break;
                case "A": nextCol--; break;
                case "D": nextCol++; break;
            }
        }

        public override void Dying()
        {
            Console.WriteLine("플레이어 사망!");
            OnDeath(this);
        }
    }
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
