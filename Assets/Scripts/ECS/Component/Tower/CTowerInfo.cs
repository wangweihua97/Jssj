using Unity.Entities;

namespace Game.ECS
{
    public struct CTowerInfo : IComponentData
    {
        public int type;
        public float atkDamage;
        public float atkInterval;
        public float atkRange;
        public int maxHP;
        public float atkPosOffset;
        public float bulletSpeed;
    }
}