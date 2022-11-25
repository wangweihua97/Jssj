using Unity.Entities;
using Unity.Mathematics;

namespace Game.ECS
{
    public struct RayHit
    {
        public float2 startPos;
        public float2 dir;
        public float len;
    }
}