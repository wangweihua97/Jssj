using Unity.Entities;

namespace Game.ECS
{
    public struct CTowerState : IComponentData
    {
        public int curState;//0一般 2死亡
        
        public float curAtkInterval;
        public float curIdleTime;
        public float curHP;
    }
}