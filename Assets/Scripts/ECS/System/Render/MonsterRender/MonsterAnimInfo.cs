using Unity.Burst;
using UnityEngine;

namespace Game.ECS
{
    [BurstCompile]
    public struct MonsterAnimInfo
    {
        public float run_anim_pos;
        public float run_anim_time;
        
        public float atk_anim_pos;
        public float atk_anim_time;
        
        public float death_anim_pos;
        public float death_anim_time;

        public float i_vat_size;
        public Matrix4x4 self_mat;
    }
}