using Unity.Entities;
using Unity.Mathematics;

namespace Game.ECS
{
    public struct CMove : IComponentData
    {
        public float2 v;
        public float2 f;
        public float i_m;

        public float2 last_f;
    }
}