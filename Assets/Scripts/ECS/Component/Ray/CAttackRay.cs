using Unity.Entities;
using Unity.Mathematics;

namespace Game.ECS
{
    public struct CAttackRay  : IComponentData
    {
        public int bulletType;
        public float2 pos;
        public float2 dir;
        public float speed;
        public float length;
        public float damage;

        public int monsterIndex;
        public float2 hitPos;
    }
}