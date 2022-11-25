using Unity.Entities;

namespace Game.ECS
{
    public struct CTowerState : IComponentData
    {
        public float curAtkInterval;
        public float curIdleTime;
        public float curHP;
    }
}