using Unity.Entities;
using Unity.Mathematics;
namespace Game.ECS
{
    public struct CPosition : IComponentData
    {
        public float2 position;
        public float radius;
        public uint id;
    }
}