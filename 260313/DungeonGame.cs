using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
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
                //SaveJson();
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
            Console.SetCursorPosition(0, 0);
            CurrentMap.PrintMap();
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
        public void SetPlayerPositionNextStage()
        {
            if (CurrentMap.grid[1,2] == ' ')
            {
                player.row = 1;
                player.col = 2;
            }
            else
            {
                player.row = 2;
                player.col = 1;
            }
            CurrentMap.grid[player.row, player.col] = 'P';
        }
        public void SetPlayerPositionPrevStage()
        {
            if (CurrentMap.grid[rows - 2, cols - 3] == ' ')
            {
                player.row = rows - 2;
                player.col = cols - 3;
            }
            else
            {
                player.row = rows - 3;
                player.col = cols - 2;
            }
            CurrentMap.grid[player.row, player.col] = 'P';
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

                    SetPlayerPositionNextStage();
                    PrintMessage("다음 맵으로!");
                    break;
                case ' ':
                    PrintMessage("이동 성공!");
                    PlayerMoveConfirm(nextRow, nextCol);
                    break;
                case 'B':
                    CurrentMap.grid[player.row, player.col] = ' ';
                    currentMapIndex--;

                    SetPlayerPositionPrevStage();
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

        public void SaveJson()
        {
            List<char[][]> mapGrids = new List<char[][]>();
            foreach (var m in maps)
            {
                mapGrids.Add(ConvertMap2(m.grid));
            }

            GameData gdata = new GameData(player, monsters, mapGrids);

            // 저장할 파일 경로
            string folderPath = "./GameData";
            string filePath = Path.Combine(folderPath, "data.json");  // 폴더와 파일 이름 합치기

            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            string result = JsonSerializer.Serialize(gdata, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(filePath, result);

            Console.WriteLine("Json으로 변환된 문자열: \n" + result);
            Console.ReadLine();

            string s = File.ReadAllText(filePath);
            GameData mm = JsonSerializer.Deserialize<GameData>(s);

            if (mm != null)
            {
                this.player = mm.player;
                this.monsters = mm.monsters;
                for (int i = 0; i < mm.maps.Count; i++)
                {
                    this.maps[i].grid = ConvertMap2Reverse(mm.maps[i]);
                }
                Console.WriteLine("읽기 성공!: " + mm);
            }
        }
        public char[][] ConvertMap2(char[,] map)
        {
            int rows = map.GetLength(0);
            int cols = map.GetLength(1);
            char[][] result = new char[rows][];

            for (int i = 0; i < rows; i++)
            {
                result[i] = new char[cols];
                for (int j = 0; j < cols; j++)
                {
                    result[i][j] = map[i, j];
                }
            }

            return result;
        }
        public char[,] ConvertMap2Reverse(char[][] jsonMap)
        {
            int rows = jsonMap.GetLength(0);
            int cols = jsonMap.GetLength(1);
            char[,] result = new char[rows, cols];

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    result[i, j] = jsonMap[i][j];
                }
            }

            return result;
        }
        public class GameData
        {
            // Json에 포함
            [JsonInclude] public Player player;
            [JsonInclude] public List<Monster> monsters;
            [JsonInclude] public List<char[][]> maps;

            // 포함하지 않음
            [JsonIgnore] public int Count { get; set; }

            public GameData(Player player, List<Monster> monsters, List<char[][]> maps)
            {
                this.player = player;
                this.monsters = monsters;
                this.maps = maps;
            }
        }

    }
}
