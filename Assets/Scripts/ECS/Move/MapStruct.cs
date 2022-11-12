﻿using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;

namespace Game.ECS
{
    [BurstCompile]
    public struct MapNodeStruct
    {
        public int2 xy;
        public int startPos;
        public int count;
        public bool canWalk;
    }

    [BurstCompile]
    public struct EntitieInfo
    {
        public int index;
        public float3 rotation;
        public float2 position;
        public float radius;
        public uint id;
        public float2 v;
        public float2 f;
        public float i_m;
    }
}