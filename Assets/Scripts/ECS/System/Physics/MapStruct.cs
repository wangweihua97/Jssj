using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;

namespace Game.ECS
{
    [BurstCompile]
    public struct MapNodeStruct
    {
        public int startPos;
        public int count;
        public bool canWalk;
    }
    
    [BurstCompile]
    public struct MapNode2Struct
    {
        public int count;
        public bool canWalk;
    }
    
    [BurstCompile]
    public struct MapNodeInfo
    {
        public float2 dir;
        public bool canWalk;
        public bool isTowar;
        public float damage;
    }

    [BurstCompile]
    public struct EntitieInfo
    {
        public int index;
        public float2 rotation;
        public float2 position;
        public float radius;
        public uint id;
        public float2 v;
        public float2 f;
        public float2 last_f;
        public float i_m;
        public bool isAlive;
        public bool isAtk;
    }
}