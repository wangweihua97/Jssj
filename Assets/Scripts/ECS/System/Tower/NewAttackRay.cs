using Unity.Mathematics;

namespace Game.ECS
{
    public struct TowarAttackResult
    {
        public int bulletType;
        public float2 pos;
        public float2 dir;
        public float rotation;
        public float speed;
        public float length;
        public float damage;
    }
}