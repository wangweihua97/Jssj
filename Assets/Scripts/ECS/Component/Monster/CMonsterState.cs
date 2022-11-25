using Unity.Entities;

namespace Game.ECS
{
    public struct CMonsterState : IComponentData
    {
        public int MonsterType;
        public int CurState; //0 行走 ，1 攻击 ，2死亡

        public float curHP;
        public float curAtkTime;
        public float curDeathTime;
        
    }
}