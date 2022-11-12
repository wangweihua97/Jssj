using System;
using System.Collections.Generic;
using Unity.Mathematics;
using Random = UnityEngine.Random;

namespace Game.Map
{
    public enum ObstacleType
    {
        Rock,
        Tree,
    }
    
    public static class AutoGenerate
    {
        /*private static int ObstacleNumber = 500;
        private static int RandomObstacleDeep = 8;
        private static int2 ObstacleRange = new int2(10,50);*/
        private static int ObstacleNumber = 100;
        private static int RandomObstacleDeep = 10;
        private static int2 ObstacleRange = new int2(30,70);
        public static FlowFieldMap Generate()
        {
            FlowFieldMap fieldMap = new FlowFieldMap();
            fieldMap.center = FlowFieldMap.Size/2;
            GenerateEntrance(fieldMap);
            GenerateAllObstacles(fieldMap);
            return fieldMap;
        }

        static void GenerateEntrance(FlowFieldMap fieldMap)
        {
            var Entrance = new int2[4];
            //left
            int left = Random.Range(20, FlowFieldMap.Size.y - 20);
            Entrance[0] = new int2(0,left);
            fieldMap.SetCellType(Entrance[0], CellType.MonsterEntrance);
            for (int i = 1; i <= 2; i++)
            {
                fieldMap.SetCellType(Entrance[0] + new int2(0,i * 1), CellType.MonsterEntrance);
                fieldMap.SetCellType(Entrance[0] - new int2(0,i * 1), CellType.MonsterEntrance);
            }
            //up
            int up = Random.Range(20, FlowFieldMap.Size.x - 20);
            Entrance[1] = new int2(up,FlowFieldMap.Size.y - 1);
            fieldMap.SetCellType(Entrance[1], CellType.MonsterEntrance);
            for (int i = 1; i <= 2; i++)
            {
                fieldMap.SetCellType(Entrance[1] + new int2(i * 1,0), CellType.MonsterEntrance);
                fieldMap.SetCellType(Entrance[1] - new int2(i * 1,0), CellType.MonsterEntrance);
            }
            //right
            int right = Random.Range(20, FlowFieldMap.Size.y - 20);
            Entrance[2] = new int2(FlowFieldMap.Size.x -1,right);
            fieldMap.SetCellType(Entrance[2], CellType.MonsterEntrance);
            for (int i = 1; i <= 2; i++)
            {
                fieldMap.SetCellType(Entrance[2] + new int2(0,i * 1), CellType.MonsterEntrance);
                fieldMap.SetCellType(Entrance[2] - new int2(0,i * 1), CellType.MonsterEntrance);
            }
            //down
            int down = Random.Range(20, FlowFieldMap.Size.x - 20);
            Entrance[3] = new int2(down,0);
            fieldMap.SetCellType(Entrance[3], CellType.MonsterEntrance);
            for (int i = 1; i <= 2; i++)
            {
                fieldMap.SetCellType(Entrance[3] + new int2(i * 1,0), CellType.MonsterEntrance);
                fieldMap.SetCellType(Entrance[3] - new int2(i * 1,0), CellType.MonsterEntrance);
            }

            fieldMap.monsterEntrance = Entrance;
        }

        public static void GenerateAllObstacles(FlowFieldMap fieldMap)
        {
            for (int i = 0; i < ObstacleNumber; i++)
            {
                GenerateObstacles(fieldMap);
            }
        }

        static void GenerateObstacles(FlowFieldMap fieldMap)
        {
            int2 random_obstacle = new int2(Random.Range(0,FlowFieldMap.Size.x),
                Random.Range(0,FlowFieldMap.Size.y));
            while (!fieldMap.CanWalkAndNotMonsterEntrance(random_obstacle))
            {
                random_obstacle = new int2(Random.Range(0,FlowFieldMap.Size.x),
                    Random.Range(0,FlowFieldMap.Size.y -1));
            }

            List<(int2 ,uint)> changed_cellType = new List<(int2 ,uint)>();
            int maxObstacleCount = Random.Range(ObstacleRange.x, ObstacleRange.y);
            int r_ObstacleType = Random.Range((int)ObstacleType.Rock, (int)ObstacleType.Tree + 1);
            GenerateSingleObstacles(fieldMap, changed_cellType, maxObstacleCount, random_obstacle, 0,
                (ObstacleType) r_ObstacleType);

            if (!fieldMap.CanConnectMonsterEntrance())
            {
                UndoGenerateSingleObstacles(fieldMap, changed_cellType);
            }
            changed_cellType.Clear();
        }

        static void GenerateSingleObstacles(FlowFieldMap fieldMap ,List<(int2 ,uint)> changed_cellType ,int maxObstacleCount ,int2 curXY ,int deep ,ObstacleType obstacleType)
        {
            if(changed_cellType.Count >= maxObstacleCount)
                return;
            changed_cellType.Add((curXY ,fieldMap.GetCellType(curXY)));
            fieldMap.SetCellType(curXY,RandomObstacleCellType(obstacleType));
            int x = curXY.x;
            int y = curXY.y;
            int2 up = new int2(x, y + 1), 
                down = new int2(x, y - 1),
                left = new int2(x - 1, y),
                right = new int2(x + 1, y);
            int2[] allDir = new[] {up, down, left, right};
            for (int i = 0; i < allDir.Length; i++)
            {
                if(changed_cellType.Count >= maxObstacleCount)
                    return;
                
                if(!fieldMap.CanWalkAndNotMonsterEntrance(allDir[i]))
                    continue;
                bool is_jump = Random.Range(0, RandomObstacleDeep) < deep;
                if(is_jump)
                    continue;
                GenerateSingleObstacles(fieldMap, changed_cellType, maxObstacleCount, allDir[i], deep + 1,
                    obstacleType);
            }
        }

        static void UndoGenerateSingleObstacles(FlowFieldMap fieldMap ,List<(int2 ,uint)> changed_cellType)
        {
            foreach (var tuple in changed_cellType)
            {
                fieldMap.SetCellType(tuple.Item1 ,tuple.Item2);
            }
        }

        static uint RandomObstacleCellType(ObstacleType obstacleType)
        {
            switch (obstacleType)
            {
                case ObstacleType.Rock:
                    int r_rock = Random.Range((int) CellType.Rock_1, (int) CellType.Rock_1 + (int) ObstacleCount.Rock);
                    return (uint)r_rock;
                    break;
                case ObstacleType.Tree:
                    int r_tree = Random.Range((int) CellType.Tree_1, (int) CellType.Tree_1 + (int) ObstacleCount.Tree);
                    return (uint)r_tree;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(obstacleType), obstacleType, null);
            }
            return 10;
        }
    }
}