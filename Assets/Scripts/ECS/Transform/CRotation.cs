using Unity.Entities;
using Unity.Mathematics;

namespace Game.ECS
{
    public struct CRotation : IComponentData
    {
        public float3 rotation;
    }
}