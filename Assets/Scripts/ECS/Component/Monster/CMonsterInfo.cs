using Unity.Entities;

namespace Game.ECS
{
    public struct CMonsterInfo  : IComponentData
    {
        public float maxBeforeAtkTime; //攻击前摇时间
        public float maxDeathTime;
        public float maxHP;
        
        public float atkInterval;
        public float atkDamage;
        public float atkRange;
    }
}