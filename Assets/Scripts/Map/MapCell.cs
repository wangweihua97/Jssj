using System.Collections.Generic;
using System.Runtime.InteropServices;
using Entitas;
using Unity.Burst;
using Unity.Mathematics;

namespace Game.Map
{
    public struct Collider
    {
        public uint shape;
        public float2 pos;
        public float radius;
        public uint id;
    }
    
    /*public struct EntityInCell
    {
        public bool is_contarin_1;
        public bool is_contarin_2;
        public bool is_contarin_3;
        public bool is_contarin_4;
        public bool is_contarin_5;
        public bool is_contarin_6;
        public Collider entity1;
        public Collider entity2;
        public Collider entity3;
        public Collider entity4;
        public Collider entity5;
        public Collider entity6;
    }
    */

    public struct MapCell
    {
        public half2 Dir;
        //public EntityInCell EntityInCell;
        public List<Collider> Colliders;
        public uint CellType;
    }

    public enum CellType
    {
        CementFloor = 0,
        Grassland = 1,
        MonsterEntrance = 9,
        Rock_1 = 10,
        Rock_2 = 11,
        Rock_3 = 12,
        Tree_1 = 20,
        Tree_2 = 21,
        PlayerCenter = 100,
    }

    public enum ObstacleCount
    {
        Rock = 3,
        Tree = 2,
    }


}