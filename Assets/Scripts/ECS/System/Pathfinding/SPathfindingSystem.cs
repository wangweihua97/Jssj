using Game.GlobalSetting;
using Game.Map;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Game.ECS
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateBefore(typeof(SPhysicsJob2System))]
    public partial class SPathfindingSystem : SystemBase
    {
        protected override void OnCreate()
        {
        }

        protected override void OnUpdate()
        {
            return;
            Entities
                .WithName("RotationSpeedSystem_SpawnAndRemove")
                .WithoutBurst()
                .ForEach((ref CMove move ,in CPosition position) =>
                {
                    
                    int2 xy = new int2((int)position.position.x ,(int)position.position.y);
                    if (MapService.FlowFieldMap.CanWalk(xy))
                    {
                        float2 dir = MapService.FlowFieldMap.map[xy.y * Setting.MapSize.x + xy.x].Dir;
                        move.f =  4 * (dir * move.i_m  - move.v);
                    }
                    else
                    {
                        move.f = - 2 * move.v;
                    }
                    
                }).Run();    
                //}).ScheduleParallel();
        }
        public void OnDestroy(ref SystemState state)
        {
        }
    }
}