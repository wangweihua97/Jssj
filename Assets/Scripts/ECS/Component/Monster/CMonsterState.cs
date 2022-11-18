using Unity.Entities;

namespace Game.ECS
{
    public struct CMonsterState : IComponentData
    {
        public int MonsterType;
        public int CurState;
    }
}