using Unity.Entities;
using Unity.Mathematics;

namespace Game.ECS
{
    public struct CMove : IComponentData
    {
        public float3 moveDir;
        public float moveSpeed;
    }
}