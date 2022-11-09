using Unity.Entities;
using Unity.Mathematics;
namespace Game.ECS
{
    public struct CPosition : IComponentData
    {
        public float3 position;
        public int indexInCell;
    }
}