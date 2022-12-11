using Unity.Entities;

namespace Game.ECS
{
    public struct CHpPlane : IComponentData
    {
        public float max_show_time;
        public float cur_show_time;
        public float percent;
    }
}