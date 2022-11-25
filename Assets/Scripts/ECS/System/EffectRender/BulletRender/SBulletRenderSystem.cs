using Game.Camera;
using Game.GlobalSetting;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Rendering;

namespace Game.ECS
{
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public partial class SBulletRenderSystem: SystemBase ,ICustomSystem
    {

        protected override void OnCreate()
        {
            base.OnCreate();
        }

        public void Init()
        {
            ;
        }
        
        
        protected override void OnUpdate()
        {
            Entities
                .WithName("BulletRender")
                .ForEach((ref Rotation rotation, ref Translation translation, in CAttackRay cAttackRay) =>
                {
                    rotation.Value = Quaternion.Euler(new Vector3(0, -math.atan2(cAttackRay.dir.y ,cAttackRay.dir.x), 0));
                    translation.Value = new float3(cAttackRay.pos.x , 0.5f ,cAttackRay.pos.y);
                }).ScheduleParallel();
        }

        

        protected override void OnDestroy()
        {
            base.OnDestroy();
        }
    }
}