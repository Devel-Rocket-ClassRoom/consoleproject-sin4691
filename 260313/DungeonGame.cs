using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _260313
{
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
}
