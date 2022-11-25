using Unity.Entities;

namespace Game.ECS
{
    public struct CMonsterInfo  : IComponentData
    {
        public float maxAtkTime;
        public float maxDeathTime;
        public float maxHP;
    }
}