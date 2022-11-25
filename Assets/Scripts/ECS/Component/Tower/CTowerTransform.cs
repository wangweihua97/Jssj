using Unity.Entities;
using Unity.Mathematics;

namespace Game.ECS
{
    public struct CTowerTransform : IComponentData
    {
        public float rotation;
        public int2 xy;
    }
}