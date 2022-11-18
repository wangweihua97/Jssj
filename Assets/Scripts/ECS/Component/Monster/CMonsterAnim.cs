using Unity.Entities;
using Unity.Mathematics;

namespace Game.ECS
{
    public struct CMonsterAnim: IComponentData
    {
        public float cur_playTime;
    }
}