using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Mathematics;
using UnityEngine;

namespace Game.Map
{
    public class FlowFieldMap
    {
        public static int2 Size = new int2(200,200);
        private const float MAX_WEIGHT = 99999f;
        public MapCell[] map;
        public int2[] monsterEntrance;
        public int2 center;
        public FlowFieldMap()
        {
            map = new MapCell[Size.x * Size.y];
            for (int i = 0; i < Size.x; i++)
            {
                for (int j = 0; j < Size.y; j++)
                {
                    map[j * Size.x + i].CellType = (int)CellType.CementFloor;
                    map[j * Size.x + i].Dir = new half2(new half(0.0f),new half(0.0f));
                    map[j * Size.x + i].EntityInCell.is_contarin_1 = false;
                    map[j * Size.x + i].EntityInCell.is_contarin_2 = false;
                    map[j * Size.x + i].EntityInCell.is_contarin_3 = false;
                    map[j * Size.x + i].EntityInCell.is_contarin_4 = false;
                    map[j * Size.x + i].EntityInCell.is_contarin_5 = false;
                    map[j * Size.x + i].EntityInCell.is_contarin_6 = false;
                }
            }
        }

        public void RefreshFlowField(int2 center)
        {
            this.center = center;
            float[] MapWeight = new float[Size.x * Size.y];
            bool[] IsVisitedMap = new bool[Size.x * Size.y];
            for (int i = 0; i < Size.x; i++)
            {
                for (int j = 0; j < Size.y; j++)
                {
                    MapWeight[j * Size.x + i] = MAX_WEIGHT;
                    IsVisitedMap[j * Size.x + i] = false;
                }
            }

            IsVisitedMap[center.y * Size.x + center.x] = true;
            MapWeight[center.y * Size.x + center.x] = 0;
            map[center.y * Size.x + center.x].CellType = (int)CellType.PlayerCenter;
            FlowField(MapWeight, IsVisitedMap, ADDNeighboursToNextCalculation(IsVisitedMap ,center));
        }

        public void SetCellType(int2 xy, CellType cellType)
        {
            map[xy.y * Size.x + xy.x].CellType = (uint)cellType;
        }
        
        public void SetCellType(int2 xy, uint cellType)
        {
            map[xy.y * Size.x + xy.x].CellType = cellType;
        }
        
        public uint GetCellType(int2 xy)
        {
            return map[xy.y * Size.x + xy.x].CellType;
        }

        public bool CanConnectMonsterEntrance()
        {
            bool[] IsVisitedMap = new bool[Size.x * Size.y];
            for (int i = 0; i < Size.x; i++)
            {
                for (int j = 0; j < Size.y; j++)
                {
                    IsVisitedMap[j * Size.x + i] = false;
                }
            }
            Stack<int2> stack = new Stack<int2>();
            stack.Push(center);
            StackPush(IsVisitedMap, center, stack);
            int connectMonsterEntranceCount = 0;
            while (stack.Count > 0)
            {
                var pop = stack.Pop();
                foreach (var entrance in monsterEntrance)
                {
                    if (entrance.Equals(pop))
                    {
                        connectMonsterEntranceCount += 1;
                        break;
                    }
                }
                DFS(IsVisitedMap, pop, stack);
            }
            return connectMonsterEntranceCount == monsterEntrance.Length;
        }

        void DFS(bool[] IsVisitedMap ,int2 curXY ,Stack<int2> stack)
        {
            int x = curXY.x;
            int y = curXY.y;
            bool up = IsValidAndNotVisited(IsVisitedMap,x, y + 1), 
                down = IsValidAndNotVisited(IsVisitedMap,x, y - 1),
                left = IsValidAndNotVisited(IsVisitedMap,x - 1, y),
                right = IsValidAndNotVisited(IsVisitedMap,x + 1, y);
            if (up)
            {
                StackPush(IsVisitedMap,curXY + new int2(0, 1), stack);
            }
            if(down)
            {
                StackPush(IsVisitedMap, curXY + new int2(0, -1), stack);
            }

            if(left)
            {
                StackPush(IsVisitedMap, curXY + new int2(-1, 0), stack);
            }

            if (right)
            {
                StackPush(IsVisitedMap, curXY + new int2(1, 0), stack);
            }
        }

        void FlowField(float[] mapWeight ,bool[] IsVisitedMap ,List<int2> points)
        {
            if(points.Count <= 0)
                return;
            List<int2> new_points = new List<int2>();
            foreach (var point in points)
            {
                var neighbours = AllNeighboursOf(point);
                float minWeight = MAX_WEIGHT;
                int2 minDir = new int2(0,0);
                foreach (var neighbour in neighbours)
                {
                    if(IsMaxWeight(mapWeight[neighbour.y * Size.x + neighbour.x]))
                        continue;
                    int2 dir = neighbour - point;
                    float distance = math.length(new float2(dir.x, dir.y));
                    float cur_weight = mapWeight[neighbour.y * Size.x + neighbour.x] + distance;

                    if (cur_weight < minWeight)
                    {
                        minWeight = cur_weight;
                        minDir = dir;
                    }
                }

                if (minDir.x != 0 || minDir.y != 0)
                {
                    float2 fMinDir = math.normalize(new float2(minDir.x, minDir.y));
                    map[point.y * Size.x + point.x].Dir = new half2(new half(fMinDir.x) ,new half(fMinDir.y));
                    mapWeight[point.y * Size.x + point.x] = minWeight;
                }
                
                new_points.AddRange(ADDNeighboursToNextCalculation(IsVisitedMap ,point));
            }
            points.Clear();
            FlowField(mapWeight, IsVisitedMap, new_points);

        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void StackPush(bool[] IsVisitedMap ,int2 xy ,Stack<int2> stack)
        {
            IsVisitedMap[xy.y * Size.x + xy.x] = true;
            stack.Push(xy);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        bool IsMaxWeight(float weight)
        {
            return (MAX_WEIGHT < weight + 1.0f);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        MapCell GetCell(int x, int y)
        {
            return map[y * Size.x + x];
        }
        
        public bool CanWalk(int2 cellXY)
        {
            return IsValid(cellXY.x ,cellXY.y);
        }
        
        public bool CanWalk(uint cellType)
        {
            return cellType < 10;
        }
        
        public bool CanWalkAndNotMonsterEntrance(int2 cellXY)
        {
            return IsValid(cellXY.x ,cellXY.y) && GetCellType(cellXY) != (uint)CellType.MonsterEntrance;
        }
        
        public bool CanWalkAndNotMonsterEntrance(uint cellType)
        {
            return CanWalk(cellType) && cellType != (uint)CellType.MonsterEntrance;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        bool IsValid(int x, int y) {
            return x >= 0 && y >= 0 && x < Size.x && y < Size.y && CanWalk(map[y * Size.x + x].CellType);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        bool IsValidOrIsPlayerCenter(int x, int y) {
            return x >= 0 && y >= 0 && x < Size.x && y < Size.y && (CanWalk(map[y * Size.x + x].CellType) || map[y * Size.x + x].CellType == (uint)CellType.PlayerCenter);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        bool IsValidAndNotVisited(bool[] IsVisitedMap ,int x, int y) {
            return x >= 0 && y >= 0 && x < Size.x && y < Size.y && CanWalk(map[y * Size.x + x].CellType) && !IsVisitedMap[y * Size.x + x];
        }

        
        
        List<int2> AllNeighboursOf(int2 v)
        {
            List<int2> res = new List<int2>();
            int x = v.x;
            int y = v.y;
            bool up = IsValidOrIsPlayerCenter(x, y + 1), 
                down = IsValidOrIsPlayerCenter(x, y - 1),
                left = IsValidOrIsPlayerCenter(x - 1, y),
                right = IsValidOrIsPlayerCenter(x + 1, y);


            if (left) {
                res.Add(new int2(x - 1, y));

                //left up
                if (up && IsValidOrIsPlayerCenter(x - 1, y + 1)) {
                    res.Add(new int2(x - 1, y + 1));
                }
            }

            if (up) {
                res.Add(new int2(x, y + 1));

                //up right
                if (right && IsValidOrIsPlayerCenter(x + 1, y + 1)) {
                    res.Add(new int2(x + 1, y + 1));
                }
            }

            if (right) {
                res.Add(new int2(x + 1, y));

                //right down
                if (down && IsValidOrIsPlayerCenter(x + 1, y - 1)) {
                    res.Add(new int2(x + 1, y - 1));
                }
            }

            if (down) {
                res.Add(new int2(x, y - 1));

                //down left
                if (left && IsValidOrIsPlayerCenter(x - 1, y - 1)) {
                    res.Add(new int2(x - 1, y - 1));
                }
            }
            return res;
        }

        void AddAndVisit(bool[] IsVisitedMap ,List<int2> res ,int2 pos)
        {
            IsVisitedMap[pos.y * Size.x + pos.x] = true;
            res.Add(pos);
            
        }
        
        List<int2> ADDNeighboursToNextCalculation(bool[] IsVisitedMap,int2 v)
        {
            List<int2> res = new List<int2>();
            int x = v.x;
            int y = v.y;
            bool up = IsValidAndNotVisited(IsVisitedMap,x, y + 1), 
                down = IsValidAndNotVisited(IsVisitedMap,x, y - 1),
                left = IsValidAndNotVisited(IsVisitedMap,x - 1, y),
                right = IsValidAndNotVisited(IsVisitedMap,x + 1, y);


            if (left) {
                AddAndVisit(IsVisitedMap, res, new int2(x - 1, y));

                //left up
                if (up && IsValidAndNotVisited(IsVisitedMap,x - 1, y + 1)) {
                    AddAndVisit(IsVisitedMap, res, new int2(x - 1, y + 1));
                }
            }

            if (up) {
                AddAndVisit(IsVisitedMap, res, new int2(x, y + 1));

                //up right
                if (right && IsValidAndNotVisited(IsVisitedMap,x + 1, y + 1)) {
                    AddAndVisit(IsVisitedMap, res, new int2(x + 1, y + 1));
                }
            }

            if (right) {
                AddAndVisit(IsVisitedMap, res, new int2(x + 1, y));

                //right down
                if (down && IsValidAndNotVisited(IsVisitedMap,x + 1, y - 1)) {
                    AddAndVisit(IsVisitedMap, res, new int2(x + 1, y - 1));
                }
            }

            if (down) {
                AddAndVisit(IsVisitedMap, res, new int2(x, y - 1));

                //down left
                if (left && IsValidAndNotVisited(IsVisitedMap,x - 1, y - 1)) {
                    AddAndVisit(IsVisitedMap, res, new int2(x - 1, y - 1));
                }
            }
            return res;
        }
    }
}